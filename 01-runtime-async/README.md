# Demo 1: Runtime Async

Fino a .NET 10 il compilatore C# trasforma ogni metodo `async` in una state machine.
In .NET 11 può invece lasciare il lavoro al runtime, che gestisce sospensione e
ripresa direttamente. Il codice sorgente non cambia: cambia cosa finisce sullo stack.

`Program.cs` contiene tre metodi async annidati; il più interno stampa lo stack
corrente, frame per frame.

## Provarla

```
dotnet run -c Release -p:AsyncMode=classic   # state machine del compilatore
dotnet run -c Release -p:AsyncMode=runtime   # Runtime Async (il default del progetto)
```

## Cosa osservare

In modalità `classic` ogni metodo async porta con sé tre frame di infrastruttura:

```
Frame sullo stack: 17
------------------------------------------------------------
   ApplicaScontiAsync.MoveNext  in Program.cs:38   <- state machine
   AsyncMethodBuilderCore.Start
   AsyncTaskMethodBuilder.Start
   Program.ApplicaScontiAsync
   ...
```

In modalità `runtime` resta solo la catena di chiamate scritta nel codice:

```
Frame sullo stack: 5
------------------------------------------------------------
   Program.ApplicaScontiAsync  in Program.cs:38
   Program.CalcolaTotaleAsync  in Program.cs:27
   Program.CaricaOrdineAsync  in Program.cs:21
   Program.<Main>$  in Program.cs:16
   Program.<Main>
```

L'output completo è in [`output-atteso/`](output-atteso).

## Come funziona

- Runtime Async si attiva con un flag del compilatore, impostato in
  `RuntimeAsyncDemo.csproj` quando `AsyncMode` vale `runtime`:
  ```xml
  <Features>$(Features);runtime-async=on</Features>
  ```
- Le due modalità compilano in cartelle separate (`bin/Release/classic`,
  `bin/Release/runtime`). Il build incrementale non ricompila quando cambia solo una
  proprietà MSBuild: senza cartelle separate, passando da una modalità all'altra
  si eseguirebbe la DLL della volta precedente.
- I frame sono stampati uno a uno con `StackTrace.GetFrames()` invece di
  `StackTrace.ToString()`, che salta alcuni frame di infrastruttura (come
  `AsyncTaskMethodBuilder.Start`) e mostra i `MoveNext` delle state machine come se
  fossero i metodi originali.

Per allungare la catena basta aggiungere altri livelli: in modalità `classic` ogni
livello aggiunge quattro frame, in modalità `runtime` uno solo.
