using System.Diagnostics;
using System.Text.RegularExpressions;

// Demo 1 — Runtime Async: lo stack trace "dal vivo".
//
//   dotnet run -c Release -p:AsyncMode=classic   -> state machine del compilatore
//   dotnet run -c Release -p:AsyncMode=runtime   -> async gestito dal runtime
//
// La catena è la stessa dell'esempio della documentazione ufficiale:
// tre metodi async annidati, e il più interno stampa lo stack "live".
// Per una catena più lunga, aggiungere livelli: la differenza cresce.

Console.WriteLine($"Modalità: {AppContext.GetData("DemoAsyncMode")}");
Console.WriteLine();

await CaricaOrdineAsync(42);

static async Task CaricaOrdineAsync(int id)
{
    await Task.CompletedTask;
    await CalcolaTotaleAsync(id);
}

static async Task CalcolaTotaleAsync(int id)
{
    await Task.CompletedTask;
    await ApplicaScontiAsync(id);
}

static async Task ApplicaScontiAsync(int id)
{
    await Task.CompletedTask;

    // Si stampano i frame uno a uno invece di usare StackTrace.ToString(),
    // che salta alcuni frame di infrastruttura (es. AsyncTaskMethodBuilder.Start)
    // e "ripulisce" i MoveNext delle state machine: così si vede tutto
    // quello che c'è davvero sullo stack.
    var trace = new StackTrace(fNeedFileInfo: true);
    Console.WriteLine($"Frame sullo stack: {trace.FrameCount}");
    Console.WriteLine(new string('-', 60));
    foreach (var frame in trace.GetFrames())
    {
        var method = frame.GetMethod();
        var type = method?.DeclaringType?.Name ?? "";
        // I tipi che iniziano con '<' sono le state machine generate dal compilatore.
        var stateMachine = type.StartsWith('<');
        var name = stateMachine ? $"{Pulisci(type)}.{method?.Name}" : $"{type}.{Pulisci(method?.Name ?? "")}";
        var file = frame.GetFileName() is { } path ? $"  in {Path.GetFileName(path)}:{frame.GetFileLineNumber()}" : "";
        Console.WriteLine($"   {name}{file}{(stateMachine ? "   <- state machine" : "")}");
    }
}

// "<<Main>$>g__ApplicaScontiAsync|0_2" -> "ApplicaScontiAsync", "<<Main>$>d__0" -> "<Main>$"
static string Pulisci(string nome)
{
    var match = Regex.Match(nome, @"g__(?<n>\w+)\||^<(?<n><Main>\$)>d__");
    return match.Success ? match.Groups["n"].Value : nome;
}
