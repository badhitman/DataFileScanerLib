////////////////////////////////////////////////
// © https://github.com/badhitman - @fakegov 
////////////////////////////////////////////////

using TextFileScannerLib.Matches;

namespace TextFileScannerLib.scan.matches;

/// <summary>
/// Match unit bytes
/// </summary>
public class MatchUnitBytes : AbstractMatchUnitCore
{
    readonly byte[] SearchBytes;

    /// <summary>
    /// Match unit bytes
    /// </summary>
    public MatchUnitBytes(byte[] setSearchBytes, byte[]? setReplacementData = null) : base(setReplacementData)
    {
        if (setSearchBytes is null || setSearchBytes.Length == 0)
            throw new ArgumentNullException(nameof(setSearchBytes), "Укажите данные поиска");

        SearchBytes = setSearchBytes;
        BufferSize = setSearchBytes.Length;
    }

    /// <summary>
    /// Checking
    /// </summary>
    public void Checking(byte[] bytesForCheck)
    {
        ArgumentNullException.ThrowIfNull(bytesForCheck);

        IndexOf = -1;
        if (SearchBytes.Length > bytesForCheck.Length)
            return;

        int[] indexes = bytesForCheck.StartingIndex(SearchBytes).ToArray();
        if (indexes.Length > 0)
            IndexOf = indexes[0];
    }

    /// <inheritdoc/>
    public override bool Equals(object? other)
    {
        if (!base.Equals(other))
            return false;

        return ((MatchUnitBytes)other).SearchBytes.SequenceEqual(SearchBytes);
    }

    /// <inheritdoc/>
    public override byte[] GetDetectedSearchData() => SearchBytes;

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        HashCode hash = new();
        hash.AddBytes(SearchBytes);
        return hash.ToHashCode();
    }
}