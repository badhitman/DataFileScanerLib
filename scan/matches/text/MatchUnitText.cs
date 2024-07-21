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
    public MatchUnitText(string SearchString, bool SetIgnoreCase, byte[]? SetReplacementData = null) : base(SetReplacementData)
    {
        if (string.IsNullOrEmpty(SearchString) || SearchString.Length == 0)
            throw new ArgumentOutOfRangeException(nameof(SearchString), "Отсутствуют данные поиска");

        SearchExpression = SearchString;
        IgnoreCase = SetIgnoreCase;
        BufferSize = AdapterFileReader.EncodingMode.GetByteCount(SearchString);
    }

    /// <inheritdoc/>
    public override void Checking(string StringForCheck)
    {
        ArgumentNullException.ThrowIfNull(StringForCheck);

        base.Checking(StringForCheck);

        if (IgnoreCase)
            IndexOf = StringForCheck.ToLower(CultureInfo.CurrentCulture).IndexOf(SearchExpression.ToLower(CultureInfo.CurrentCulture), StringComparison.CurrentCulture);
        else
            IndexOf = StringForCheck.IndexOf(SearchExpression, StringComparison.CurrentCulture);

    }

    /// <inheritdoc/>
    public override byte[] GetDetectedSearchData() => AdapterFileReader.EncodingMode.GetBytes(SearchExpression);
}