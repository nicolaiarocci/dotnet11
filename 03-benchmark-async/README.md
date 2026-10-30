# Demo 3: quanto costa l'infrastruttura async

La demo 1 mostra che Runtime Async cambia cosa finisce sullo stack. Questa misura quanto
costa, in tempo e in memoria, attraversare una catena di tre metodi async nelle due
modalità, con [BenchmarkDotNet](https://benchmarkdotnet.org) e `[MemoryDiagnoser]`.

> **Cosa non misura.** Questo benchmark misura il costo dell'infrastruttura async, non le
> prestazioni di un'applicazione, dove I/O e rete dominano. La differenza in valore assoluto
> è di centinaia di nanosecondi per catena: una singola chiamata di rete da 1 ms ne vale un
> milione. Non aspettatevi che la vostra applicazione diventi due volte più veloce.

## Cosa misura

Due scenari, ciascuno con una catena di tre metodi async marcati `NoInlining`, così il JIT
non la appiattisce:

| Benchmark | Livello più interno | Cosa succede |
|---|---|---|
| `Sincrono` | `await Task.CompletedTask` | Nessuna sospensione: la catena completa in modo sincrono. |
| `ConYield` | `await Task.Yield()` | Il livello interno si sospende davvero e riprende sul thread pool. |

Ogni scenario viene eseguito due volte, con il codice compilato in modalità `classic` e
`runtime`.

## Lanciarlo

```
dotnet run -c Release
```

Dura circa due minuti. Per misure confrontabili chiudete il resto delle applicazioni.

## Risultati

| Scenario | Modalità | Tempo medio | Errore | Ratio | Allocati/op | Alloc ratio |
|---|---|---:|---:|---:|---:|---:|
| `Sincrono` | classic | 13,48 ns | ±0,05 ns | 1,00 | 0 B | – |
| `Sincrono` | runtime | **7,10 ns** | ±0,02 ns | **0,53** | 0 B | – |
| `ConYield` | classic | 929,8 ns | ±10,0 ns | 1,00 | 372 B | 1,00 |
| `ConYield` | runtime | **395,3 ns** | ±4,3 ns | **0,43** | **208 B** | **0,56** |

Configurazione della macchina:

- BenchmarkDotNet v0.16.0-preview.2, job di default per entrambe le modalità
- macOS Sequoia 15.7.4 (24G517), Darwin 24.6.0
- Apple M2 Pro, 12 core, 32 GB di RAM
- .NET SDK 11.0.100-rc.3.26522.148, runtime .NET 11.0.0-rc.3, Arm64 RyuJIT armv8.0-a

Il report originale è in [`output-atteso/`](output-atteso). I numeri sono stati misurati su
una RC e su una sola macchina: su hardware diverso i valori assoluti cambiano, quindi
confrontate i rapporti.

## Come leggerli

- **`Sincrono`:** nessuna delle due modalità alloca, perché i metodi completano subito e
  restituiscono un `Task` già completato e riusato. In tempo runtime costa circa la metà,
  ma si parla di 6 ns su tre livelli.
- **`ConYield`:** qui c'è una sospensione vera. Runtime impiega meno della metà del tempo e
  alloca il 44% in meno (208 B contro 372 B). In `classic` le state machine sospese
  finiscono sullo heap; in `runtime` la sospensione la gestisce il runtime.
- **Il risultato vale per questa catena.** Tre livelli e una sola sospensione sono un caso
  scelto apposta per isolare l'infrastruttura: con più livelli, altri tipi di task o altre
  sospensioni i rapporti possono cambiare.
- **Errore e ripetibilità.** La colonna Errore descrive un singolo run. In tre run su questa
  macchina `ConYield` è andato da 793 a 930 ns in classic e da 343 a 430 ns in runtime, una
  variazione più ampia dell'errore indicato, mentre il rapporto è rimasto tra 0,43 e 0,47:
  i rapporti sono più affidabili delle singole cifre.

## Come funziona

- Le due modalità si scelgono in fase di compilazione, con lo stesso flag della
  [demo 1](../01-runtime-async):
  ```xml
  <Features>$(Features);runtime-async=on</Features>
  ```
- Ogni modalità è un job di BenchmarkDotNet che ricompila il progetto con un proprio
  `/p:AsyncMode=...`, in cartelle separate.
- All'avvio di ogni job, `VerificaModalita` conta i frame `AsyncMethodBuilder` sullo stack e
  stampa il risultato nel log: 2 in `classic`, 0 in `runtime`. Così si vede che il job sta
  eseguendo la modalità prevista ([`verifica-modalita.txt`](output-atteso/verifica-modalita.txt)).

## Note

- Il progetto usa una preview di BenchmarkDotNet perché la 0.15.8, ultima stabile, non
  riconosce .NET 11: si ferma con `GetRuntimeVersion not implemented for NotRecognized`.
- I numeri saranno rimisurati e aggiornati dopo la GA di .NET 11.
