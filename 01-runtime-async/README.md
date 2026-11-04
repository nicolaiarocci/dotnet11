# Demo 1: Runtime Async

Fino a .NET 10 il compilatore C# trasforma ogni metodo `async` in una state machine.
In .NET 11 può invece lasciare il lavoro al runtime, che gestisce sospensione e
ripresa direttamente. Il codice sorgente non cambia: cambia cosa finisce sullo stack.

`Program.cs` contiene tre metodi async annidati; il più interno stampa lo stack
corrente, frame per frame.

## Provarla

```
dotnet run -c Release
```

Così com'è, il progetto usa le state machine del compilatore. Per attivare Runtime Async
decommenta in `RuntimeAsyncDemo.csproj` la riga

```xml
<Features>$(Features);runtime-async=on</Features>
```

e rilancia lo stesso comando. Ricommentandola si torna alle state machine.

## Cosa osservare

In modalità `classic` ogni metodo async porta con sé tre frame di infrastruttura:

```
Frame sullo stack: 17
------------------------------------------------------------
   ApplicaScontiAsync.MoveNext  in Program.cs:32   <- state machine
   AsyncMethodBuilderCore.Start
   AsyncTaskMethodBuilder.Start
   Program.ApplicaScontiAsync
   ...
```

In modalità `runtime` resta solo la catena di chiamate scritta nel codice:

```
Frame sullo stack: 5
------------------------------------------------------------
   Program.ApplicaScontiAsync  in Program.cs:32
   Program.CalcolaTotaleAsync  in Program.cs:23
   Program.CaricaOrdineAsync  in Program.cs:18
   Program.<Main>$  in Program.cs:14
   Program.<Main>
```

L'output completo è in [`output-atteso/`](output-atteso).

## Come funziona

- Runtime Async si attiva con un flag del compilatore, la riga `<Features>` del
  `.csproj`; non esiste una proprietà MSBuild dedicata. Modificare il `.csproj` basta a
  far ricompilare il progetto, senza `dotnet clean`.
- La modalità stampata in testa non viene dal `.csproj`: è dedotta dallo stack, dove i
  frame delle state machine (tipi che implementano `IAsyncStateMachine`) ci sono solo
  in `classic`.
- I frame sono stampati uno a uno con `StackTrace.GetFrames()` invece di
  `StackTrace.ToString()`, che salta alcuni frame di infrastruttura (come
  `AsyncTaskMethodBuilder.Start`) e mostra i `MoveNext` delle state machine come se
  fossero i metodi originali.

Per allungare la catena basta aggiungere altri livelli: in modalità `classic` ogni
livello aggiunge quattro frame, in modalità `runtime` uno solo.
