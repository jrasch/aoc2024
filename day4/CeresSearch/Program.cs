using ILGPU;
using ILGPU.Runtime;

namespace CeresSearch
{
    public readonly struct Cell(int value, int x, int y)
    {
        public int Value { get; } = value;
        public int X { get; } = x;
        public int Y { get; } = y;

        public override string ToString() => Value.ToString();
    }

    class Program
    {
        static void Main(string[] args)
        {
            var puzzleText = LoadPuzzle(args.Length > 0 ? args[0] : null);
            var puzzle = ConvertFromPuzzleString(puzzleText);

            PrintPuzzle(puzzle);

            using var context = Context.CreateDefault();
            using var accelerator = context
                .GetPreferredDevice(preferCPU: false)
                .CreateAccelerator(context);

            var launchKernel = accelerator.LoadAutoGroupedStreamKernel<Index2D, ArrayView2D<Cell, Stride2D.DenseX>, ArrayView2D<int, Stride2D.DenseX>>(KernelCountWords);

            using var devicePuzzleData = accelerator.Allocate2DDenseX(puzzle);
            using var deviceOutput = accelerator.Allocate2DDenseX<int>(devicePuzzleData.Extent);

            devicePuzzleData.CopyFromCPU(puzzle);

            launchKernel(new Index2D(puzzle.GetLength(0), puzzle.GetLength(1)), devicePuzzleData.View, deviceOutput.View);

            accelerator.Synchronize();
            
            int wordCount = deviceOutput.GetAsArray2D().Cast<int>().Sum();

            Console.WriteLine("Total word count: {0}", wordCount);
        }

        static void KernelCountWords(Index2D index, ArrayView2D<Cell, Stride2D.DenseX> puzzleView, ArrayView2D<int, Stride2D.DenseX> output)
        {
            if (puzzleView[index].Value != 1)
                return;

            int x = index.X;
            int y = index.Y;

            int wordCount =
                PossibleWord(puzzleView, (x - 1, y - 1), (x - 2, y - 2), (x - 3, y - 3))  // up-left
              + PossibleWord(puzzleView, (x    , y - 1), (x    , y - 2), (x    , y - 3))  // up
              + PossibleWord(puzzleView, (x + 1, y - 1), (x + 2, y - 2), (x + 3, y - 3))  // up-right
              + PossibleWord(puzzleView, (x + 1, y    ), (x + 2, y    ), (x + 3, y    ))  // right
              + PossibleWord(puzzleView, (x + 1, y + 1), (x + 2, y + 2), (x + 3, y + 3))  // down-right
              + PossibleWord(puzzleView, (x    , y + 1), (x    , y + 2), (x    , y + 3))  // down
              + PossibleWord(puzzleView, (x - 1, y + 1), (x - 2, y + 2), (x - 3, y + 3))  // down-left
              + PossibleWord(puzzleView, (x - 1, y    ), (x - 2, y    ), (x - 3, y    )); // left

            output[index] = wordCount;
        }

        static int PossibleWord(ArrayView2D<Cell, Stride2D.DenseX> puzzleView, (int x, int y) c2, (int x, int y) c3, (int x, int y) c4)
        {
            if (c4.x < 0 || c4.x >= puzzleView.IntExtent.X || c4.y < 0 || c4.y >= puzzleView.IntExtent.Y)
            {
                return 0; // out of bounds
            }

            return puzzleView[c2.x, c2.y].Value == 2
                && puzzleView[c3.x, c3.y].Value == 3
                && puzzleView[c4.x, c4.y].Value == 4 ? 1 : 0;
        }

        static void PrintPuzzle(Cell[,] puzzle)
        {
            for (int x = 0; x < puzzle.GetLength(0); x++)
            {
                for (int y = 0; y < puzzle.GetLength(1); y++)
                {
                    Console.Write(puzzle[x, y] + " ");
                }
                Console.WriteLine();
            }
            Console.WriteLine();
        }

        static string LoadPuzzle(string? filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
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

            return File.ReadAllText(filePath);
        }

        static Cell[,] ConvertFromPuzzleString(string puzzleText)
        {
            var jagged = puzzleText.Split(Environment.NewLine)
                .Select(line => line.Trim().ToUpperInvariant())
                .ToArray();

            var result = new Cell[jagged[0].Length, jagged.Length];
            for (int x = 0; x < jagged.Length; x++)
            {
                for (int y = 0; y < jagged[x].Length; y++)
                {
                    var letter = jagged[x][y];
                    result[x, y] = letter switch
                    {
                        'X' => new Cell(1, x, y),
                        'M' => new Cell(2, x, y),
                        'A' => new Cell(3, x, y),
                        'S' => new Cell(4, x, y),
                        _ => new Cell(0, x, y)
                    };
                }
            }
            return result;
        }
    }
}
