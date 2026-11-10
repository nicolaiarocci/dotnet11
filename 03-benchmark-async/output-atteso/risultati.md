```

BenchmarkDotNet v0.16.0-preview.2, macOS Sequoia 15.7.4 (24G517) [Darwin 24.6.0]
Apple M2 Pro, 1 CPU, 12 logical and 12 physical cores
Memory: 32 GB Total, 0.34 GB Available
.NET SDK 11.0.100
  [Host]  : .NET 11.0.0 (11.0.0, 11.0.26.53018), Arm64 RyuJIT armv8.0-a
  classic : .NET 11.0.0 (11.0.0, 11.0.26.53018), Arm64 RyuJIT armv8.0-a
  runtime : .NET 11.0.0 (11.0.0, 11.0.26.53018), Arm64 RyuJIT armv8.0-a


```
| Method   | Job     | Mean       | Error      | StdDev     | Median       | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|--------- |-------- |-----------:|-----------:|-----------:|-------------:|------:|--------:|-------:|----------:|------------:|
| Sincrono | classic |  13.475 ns |  0.0212 ns |  0.0166 ns |    13.480 ns |  1.00 |    0.00 |      - |         - |          NA |
| Sincrono | runtime |   7.075 ns |  0.0123 ns |  0.0109 ns |     7.072 ns |  0.53 |    0.00 |      - |         - |          NA |
|          |         |            |            |            |              |       |         |        |           |             |
| ConYield | classic | 996.551 ns | 19.7292 ns | 48.7657 ns | 1,022.892 ns |  1.00 |    0.00 | 0.0439 |     365 B |        1.00 |
| ConYield | runtime | 377.853 ns |  7.5114 ns | 17.4089 ns |   383.158 ns |  0.38 |    0.03 | 0.0248 |     208 B |        0.57 |
