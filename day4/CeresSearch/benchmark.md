```

BenchmarkDotNet v0.14.0, Windows 10 (10.0.19045.5131/22H2/2022Update)
AMD Ryzen 9 3900X, 1 CPU, 24 logical and 12 physical cores
.NET SDK 9.0.101
  [Host]     : .NET 9.0.0 (9.0.24.52809), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.0 (9.0.24.52809), X64 RyuJIT AVX2


```

| Method  | PreferCpu |         Mean |       Error |      StdDev |
| ------- | --------- | -----------: | ----------: | ----------: |
| **Run** | **False** | **198.4 ms** | **3.96 ms** | **6.73 ms** |
| **Run** | **True**  | **240.9 ms** | **4.60 ms** | **5.30 ms** |

Input file size: ~1MB
