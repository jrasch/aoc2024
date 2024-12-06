using BenchmarkDotNet.Attributes;
using CeresSearch;
using ILGPU;
using ILGPU.Runtime;
using System.Diagnostics;

public class Runner
{
    [Params(true, false)]
    public bool PreferCpu { get; set; }

    [Benchmark]
    public void Run()
    {
        var puzzleText = LoadPuzzle();
        var puzzle = ConvertFromPuzzleString(puzzleText);

        //PrintPuzzle(puzzle);

        using var context = Context.CreateDefault();
        using var accelerator = context
            .GetPreferredDevice(preferCPU: this.PreferCpu)
            .CreateAccelerator(context);

        var launchKernel = accelerator.LoadAutoGroupedStreamKernel<Index2D, ArrayView2D<Cell, Stride2D.DenseX>, ArrayView2D<int, Stride2D.DenseX>>(CudaKernel.CountWords);

        using var devicePuzzleData = accelerator.Allocate2DDenseX(puzzle);
        using var deviceOutput = accelerator.Allocate2DDenseX<int>(devicePuzzleData.IntExtent);
        deviceOutput.MemSetToZero();

        devicePuzzleData.CopyFromCPU(puzzle);

        launchKernel(new Index2D(puzzle.GetLength(0), puzzle.GetLength(1)), devicePuzzleData.View, deviceOutput.View);

        accelerator.Synchronize();

        int wordCount = deviceOutput.GetAsArray2D().Cast<int>().Sum();

        Console.WriteLine("Total word count: {0}", wordCount);
    }

    //private void PrintPuzzle(Cell[,] puzzle)
    //{
    //    for (int x = 0; x < puzzle.GetLength(0); x++)
    //    {
    //        for (int y = 0; y < puzzle.GetLength(1); y++)
    //        {
    //            Console.Write(puzzle[x, y] + " ");
    //        }
    //        Console.WriteLine();
    //    }
    //    Console.WriteLine();
    //}

    private Cell[,] ConvertFromPuzzleString(string puzzleText)
    {
        var lines = puzzleText.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(line => line.ToUpperInvariant())
            .ToArray();

        var result = new Cell[lines.First().Length, lines.Length];

        for (int lineIndex = 0; lineIndex < lines.Length; lineIndex++)
        {
            for (int charIndex = 0; charIndex < lines[lineIndex].Length; charIndex++)
            {
                var letter = lines[lineIndex][charIndex];
                result[charIndex, lineIndex] = letter switch
                {
                    'X' => new Cell(1, charIndex, lineIndex),
                    'M' => new Cell(2, charIndex, lineIndex),
                    'A' => new Cell(3, charIndex, lineIndex),
                    'S' => new Cell(4, charIndex, lineIndex),
                    _ => new Cell(0, charIndex, lineIndex)
                };
            }
        }
        return result;
    }

    private string LoadPuzzle()
    {
        try
        {
            string puzzle = File.ReadAllText("input.txt");
            Console.WriteLine("Loaded from file");
            return puzzle;
        }
        catch
        { 
        }

        Console.WriteLine("Loaded from sample");

        return """
            MMMSXXMASM
            MSAMXMSMSA
            AMXSXMAAMM
            MSAMASMSMX
            XMASAMXAMM
            XXAMMXXAMA
            SMSMSASXSS
            SAXAMASAAA
            MAMMMXMMMM
            MXMXAXMASX
            """;
    }
}
