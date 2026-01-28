////////////////////////////////////////////////
// © https://github.com/badhitman - @fakegov 
////////////////////////////////////////////////

using System.Globalization;
using TextFileScannerLib.scan.matches;

namespace TextFileScannerLib.Matches;

/// <summary>
/// Match unit text
/// </summary>
public class MatchUnitText : AbstractMatchUnitText
{
    /// <summary>
    /// Match unit text
    /// </summary>
    public MatchUnitText(string searchString, bool setIgnoreCase, byte[]? setReplacementData = null) : base(setReplacementData)
    {
        if (string.IsNullOrEmpty(searchString) || searchString.Length == 0)
            throw new ArgumentOutOfRangeException(nameof(searchString), "Отсутствуют данные поиска");

        SearchExpression = searchString;
        IgnoreCase = setIgnoreCase;
        BufferSize = AdapterFileReader.EncodingMode.GetByteCount(searchString);
    }

    /// <inheritdoc/>
    public override void Checking(string stringForCheck)
    {
        ArgumentNullException.ThrowIfNull(stringForCheck);

        base.Checking(stringForCheck);

        if (IgnoreCase)
            IndexOf = stringForCheck.ToLower(CultureInfo.CurrentCulture).IndexOf(SearchExpression.ToLower(CultureInfo.CurrentCulture), StringComparison.CurrentCulture);
        else
            IndexOf = stringForCheck.IndexOf(SearchExpression, StringComparison.CurrentCulture);

    }

    /// <inheritdoc/>
    public override byte[] GetDetectedSearchData() => AdapterFileReader.EncodingMode.GetBytes(SearchExpression);
}