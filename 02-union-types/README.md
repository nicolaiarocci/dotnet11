# Demo 2: union types e closed hierarchy

C# 15 introduce due modi per dire al compilatore "i casi possibili sono questi e
solo questi". Una sola feature del linguaggio attraversa quattro livelli: compilatore,
serializzazione, framework web e contratto OpenAPI.

[`Model.cs`](Model.cs) contiene:

- una **union**: `MetodoDiPagamento` è una `CartaDiCredito`, un `Bonifico` oppure un
  `Contrassegno`, tre record senza alcuna base comune;
  ```csharp
  public union MetodoDiPagamento(CartaDiCredito, Bonifico, Contrassegno);
  ```
- una **closed hierarchy**: `StatoOrdine` si può derivare solo dentro questo assembly,
  quindi il compilatore ne conosce tutti i figli;
  ```csharp
  public closed record class StatoOrdine;
  ```
- due `switch` esaustivi, uno per ciascuno, senza ramo `default`.

[`Program.cs`](Program.cs) espone gli ordini con una Minimal API.

## Provarla

```
dotnet run
```

L'app risponde su `http://localhost:5080`. Da un secondo terminale, con
[`jq`](https://jqlang.org) per formattare il JSON:

```
curl -s localhost:5080/ordini | jq
curl -s localhost:5080/ordini/2/pagamento | jq
curl -s localhost:5080/openapi/v1.json | jq '.components.schemas.MetodoDiPagamento'
```

Gli endpoint:

| URL | Cosa mostra |
|---|---|
| `/ordini` | Riepilogo leggibile costruito con gli switch esaustivi |
| `/ordini/1/pagamento`, `/2/`, `/3/` | La union serializzata da System.Text.Json |
| `/openapi/v1.json` | Il documento OpenAPI 3.2 generato |

## Cosa osservare

**1. Il compilatore controlla i casi.** In `Model.cs` decommentate la riga

```csharp
// public record class Consegnato(DateOnly Il) : StatoOrdine;
```

e lanciate `dotnet build`: il build fallisce perché lo switch su `StatoOrdine` non
gestisce il nuovo caso.

```
Model.cs(40,63): error CS8509: The switch expression does not handle all possible values
of its input type (it is not exhaustive). For example, the pattern 'UnionDemo.Consegnato'
is not covered.
```

Di norma CS8509 è un warning; qui `WarningsAsErrors` in `UnionDemo.csproj` lo rende un
errore. Per sistemare, decommentate il ramo già pronto nello switch:

```csharp
Consegnato c => $"Consegnato il {c.Il:dd/MM}",
```

**2. La union si serializza senza converter.** Ogni endpoint `/pagamento` restituisce
solo il caso attivo:

```json
{"circuito":"Visa","ultimeCifre":"4242"}
{"iban":"IT60X0542811101000000123456"}
{"supplemento":4.00}
```

**3. OpenAPI la descrive come `anyOf`.** Chi genera un client dal documento vede lo
stesso contratto del codice C#:

```json
"MetodoDiPagamento": {
  "type": "object",
  "anyOf": [
    { "$ref": "#/components/schemas/CartaDiCredito" },
    { "$ref": "#/components/schemas/Bonifico" },
    { "$ref": "#/components/schemas/Contrassegno" }
  ]
}
```

Tutte le risposte complete sono in [`output-atteso/`](output-atteso).

### E in deserializzazione?

Il JSON della union non ha un discriminatore come `$type`. Di default System.Text.Json
distingue i casi solo dal tipo di valore JSON (oggetto, stringa, numero...): qui i tre
casi sono tutti oggetti, quindi `JsonSerializer.Deserialize<MetodoDiPagamento>` fallisce
con un errore di ambiguità. La demo non ne risente perché la union viene solo restituita.

Per leggerla serve un classificatore, indicato con `[JsonUnion]`. Quello strutturale
riconosce il caso dai nomi delle proprietà:

```csharp
[JsonUnion(TypeClassifier = typeof(JsonUnionTypeStructuralClassifier))]
public union MetodoDiPagamento(CartaDiCredito, Bonifico, Contrassegno);
```

Se due casi hanno la stessa forma JSON, il classificatore strutturale non tira a
indovinare: lancia un `NotSupportedException`. In quel caso serve un classificatore
scritto a mano.

### E la gerarchia chiusa?

Di default System.Text.Json tratta una gerarchia `closed` come una normale gerarchia
`abstract`: per un valore dichiarato `StatoOrdine` vede solo le proprietà del tipo dichiarato,
che qui non ne ha. Non dà errore, ma i dati si perdono.

```csharp
StatoOrdine stato = new Spedito(new(2026, 11, 20), "BRT");
```

```json
{}
```

Rileggere quel JSON con `Deserialize<StatoOrdine>` lancia un `NotSupportedException`, perché il
tipo è astratto. Con l'opzione `InferClosedTypePolymorphism`, che funziona solo sui tipi `closed`,
System.Text.Json scopre da solo i tipi derivati e scrive un discriminatore:

```csharp
JsonSerializerOptions opzioni = new(JsonSerializerDefaults.Web) { InferClosedTypePolymorphism = true };
```

```json
{"$type":"Spedito","il":"2026-11-20","corriere":"BRT"}
```

Questo JSON si rilegge, anche quando due casi hanno la stessa forma. Lo stesso risultato si ottiene
sul singolo tipo con `[JsonPolymorphic(InferClosedTypePolymorphism = true)]`. Il discriminatore è
il nome semplice del tipo, quindi rinominare una classe cambia il contratto JSON.

La demo non usa questa opzione: `/ordini` espone lo stato come stringa.

## Note

- La versione di `Microsoft.AspNetCore.OpenApi` in `UnionDemo.csproj` è allineata alla
  RC 3 dell'SDK; con un SDK più recente conviene aggiornarla alla versione corrispondente.
- `Program.cs` imposta la cultura `it-IT`, così importi e date sono formattati
  all'italiana qualunque sia il locale della macchina.
