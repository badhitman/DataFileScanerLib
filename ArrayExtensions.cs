////////////////////////////////////////////////
// © https://github.com/badhitman - @fakegov 
////////////////////////////////////////////////

namespace TextFileScannerLib;

static class ArrayExtensions
{
    public static IEnumerable<int> StartingIndex(this byte[] x, byte[] y)
    {
        if (x.Length < y.Length)
            return [];

        IEnumerable<int> index = Enumerable.Range(0, x.Length - y.Length + 1);//16 24
        for (int i = 0; i < y.Length; i++)
            index = index.Where(n => x[n + i] == y[i]).ToArray();

        return index;
    }
}