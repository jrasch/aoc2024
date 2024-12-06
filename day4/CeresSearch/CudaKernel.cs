using CeresSearch;
using ILGPU;
using ILGPU.Runtime;

/// <summary>
/// Code that runs on the GPU in a CUDA kernel
/// </summary>
public static class CudaKernel
{
    public static void CountWords(Index2D index, ArrayView2D<Cell, Stride2D.DenseX> puzzleView, ArrayView2D<int, Stride2D.DenseX> output)
    {
        if (puzzleView[index].Value != 1)
            return;

        int x = index.X;
        int y = index.Y;

        int wordCount =
            PossibleWord(puzzleView, (x - 1, y - 1), (x - 2, y - 2), (x - 3, y - 3))  // up-left
          + PossibleWord(puzzleView, (x, y - 1), (x, y - 2), (x, y - 3))  // up
          + PossibleWord(puzzleView, (x + 1, y - 1), (x + 2, y - 2), (x + 3, y - 3))  // up-right
          + PossibleWord(puzzleView, (x + 1, y), (x + 2, y), (x + 3, y))  // right
          + PossibleWord(puzzleView, (x + 1, y + 1), (x + 2, y + 2), (x + 3, y + 3))  // down-right
          + PossibleWord(puzzleView, (x, y + 1), (x, y + 2), (x, y + 3))  // down
          + PossibleWord(puzzleView, (x - 1, y + 1), (x - 2, y + 2), (x - 3, y + 3))  // down-left
          + PossibleWord(puzzleView, (x - 1, y), (x - 2, y), (x - 3, y)); // left

        output[index] = wordCount;
    }

    private static int PossibleWord(ArrayView2D<Cell, Stride2D.DenseX> puzzleView, (int x, int y) c2, (int x, int y) c3, (int x, int y) c4)
    {
        if (c4.x < 0 || c4.x >= puzzleView.IntExtent.X || c4.y < 0 || c4.y >= puzzleView.IntExtent.Y)
        {
            return 0; // out of bounds
        }

        return puzzleView[c2.x, c2.y].Value == 2
            && puzzleView[c3.x, c3.y].Value == 3
            && puzzleView[c4.x, c4.y].Value == 4 ? 1 : 0;
    }
}
