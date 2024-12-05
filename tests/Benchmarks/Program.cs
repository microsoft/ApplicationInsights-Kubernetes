using Benchmark;
using BenchmarkDotNet.Running;

var summary = BenchmarkRunner.Run<StringUtilsBenchmark>();
Console.WriteLine(summary);