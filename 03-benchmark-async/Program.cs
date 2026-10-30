using System.Diagnostics;
using System.Runtime.CompilerServices;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;

_ = BenchmarkRunner.Run<CatenaAsync>(args: args);

// Due job identici tranne che per la modalità di compilazione: BenchmarkDotNet
// ricompila il progetto per ciascuno, passando -p:AsyncMode.
public class ConfigModalita : ManualConfig
{
    public ConfigModalita()
    {
        AddJob(Job.Default.WithArguments([new MsBuildArgument("/p:AsyncMode=classic")]).WithId("classic").AsBaseline());
        AddJob(Job.Default.WithArguments([new MsBuildArgument("/p:AsyncMode=runtime")]).WithId("runtime"));
        HideColumns("Arguments");
    }
}

[MemoryDiagnoser]
[Config(typeof(ConfigModalita))]
public class CatenaAsync
{
    [GlobalSetup]
    public void VerificaModalita()
    {
        // Controllo che il job stia davvero eseguendo la modalità richiesta:
        // in classic lo stack contiene i frame del builder, in runtime no.
        var frameBuilder = ContaFrameBuilder().GetAwaiter().GetResult();
        Console.WriteLine($"// Modalità: {AppContext.GetData("BenchAsyncMode")}, frame AsyncMethodBuilder sullo stack: {frameBuilder}");
    }

    // Scenario 1: nessuna sospensione, la catena completa in modo sincrono.
    [Benchmark]
    public Task Sincrono() => SincronoLivello1();

    // Scenario 2: il livello più interno si sospende davvero con Task.Yield().
    [Benchmark]
    public Task ConYield() => YieldLivello1();

    [MethodImpl(MethodImplOptions.NoInlining)]
    static async Task SincronoLivello1() => await SincronoLivello2();

    [MethodImpl(MethodImplOptions.NoInlining)]
    static async Task SincronoLivello2() => await SincronoLivello3();

    [MethodImpl(MethodImplOptions.NoInlining)]
    static async Task SincronoLivello3() => await Task.CompletedTask;

    [MethodImpl(MethodImplOptions.NoInlining)]
    static async Task YieldLivello1() => await YieldLivello2();

    [MethodImpl(MethodImplOptions.NoInlining)]
    static async Task YieldLivello2() => await YieldLivello3();

    [MethodImpl(MethodImplOptions.NoInlining)]
    static async Task YieldLivello3() => await Task.Yield();

    static async Task<int> ContaFrameBuilder()
    {
        await Task.CompletedTask;
        return new StackTrace().GetFrames().Count(f => f.GetMethod()?.DeclaringType?.Name.Contains("MethodBuilder") is true);
    }
}
