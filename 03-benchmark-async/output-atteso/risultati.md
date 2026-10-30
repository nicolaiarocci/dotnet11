```

BenchmarkDotNet v0.16.0-preview.2, macOS Sequoia 15.7.4 (24G517) [Darwin 24.6.0]
Apple M2 Pro, 1 CPU, 12 logical and 12 physical cores
Memory: 32 GB Total, 1.97 GB Available
.NET SDK 11.0.100-rc.3.26522.148
  [Host]  : .NET 11.0.0 (11.0.0-rc.3.26522.148, 11.0.26.52348), Arm64 RyuJIT armv8.0-a
  classic : .NET 11.0.0 (11.0.0-rc.3.26522.148, 11.0.26.52348), Arm64 RyuJIT armv8.0-a
  runtime : .NET 11.0.0 (11.0.0-rc.3.26522.148, 11.0.26.52348), Arm64 RyuJIT armv8.0-a


```
| Method   | Job     | Mean       | Error      | StdDev    | Ratio | Gen0   | Allocated | Alloc Ratio |
|--------- |-------- |-----------:|-----------:|----------:|------:|-------:|----------:|------------:|
| Sincrono | classic |  13.479 ns |  0.0455 ns | 0.0403 ns |  1.00 |      - |         - |          NA |
| Sincrono | runtime |   7.102 ns |  0.0188 ns | 0.0167 ns |  0.53 |      - |         - |          NA |
|          |         |            |            |           |       |        |           |             |
| ConYield | classic | 929.818 ns | 10.0448 ns | 9.3959 ns |  1.00 | 0.0448 |     372 B |        1.00 |
| ConYield | runtime | 395.283 ns |  4.2533 ns | 3.7705 ns |  0.43 | 0.0248 |     208 B |        0.56 |
