////////////////////////////////////////////////
// © https://github.com/badhitman - @fakegov 
////////////////////////////////////////////////

using System.Text.RegularExpressions;
using TextFileScannerLib.scan.matches;

namespace TextFileScannerLib.Matches;

/// <summary>
/// Match unit regexp
/// </summary>
public class MatchUnitRegexp : AbstractMatchUnitText
{
    /// <summary>
    /// Search regex
    /// </summary>
    public Regex SearchRegex { get; }

    /// <summary>
    /// Detected match
    /// </summary>
    public Match? DetectedMatch { get; private set; }

    private byte[]? DetectedSearchExpressionData { get; set; }

    /// <summary>
    /// Match unit regexp
    /// </summary>
    public MatchUnitRegexp(Regex regex, int SetBufferSize, byte[]? SetReplacementData = null) : base(SetReplacementData)
    {
        ArgumentNullException.ThrowIfNull(regex);

        if (!regex.Options.HasFlag(RegexOptions.Compiled))
            throw new ArgumentException("Регулярное выражение должно быть скомпилировано", nameof(regex));

        if (SetBufferSize < 1)
            throw new ArgumentOutOfRangeException(nameof(SetBufferSize), "Размер буфера должен быть больше нуля");

        if (string.IsNullOrEmpty(regex.ToString()))
            throw new ArgumentOutOfRangeException(nameof(regex), "Отсутствуют данные поиска");

        BufferSize = SetBufferSize;
        SearchRegex = regex;
        SearchExpression = regex.ToString();
        IgnoreCase = regex.Options.HasFlag(RegexOptions.IgnoreCase);
    }

    /// <summary>
    /// Checking
    /// </summary>
    public override void Checking(string data_for_check)
    {
        DetectedSearchExpressionData = [];
        base.Checking(data_for_check);
        DetectedMatch = SearchRegex.Match(data_for_check);
        if (DetectedMatch.Success)
        {
            IndexOf = DetectedMatch.Index;
            DetectedSearchExpressionData = AdapterFileReader.EncodingMode.GetBytes(DetectedMatch.Value);
        }
    }

    /// <inheritdoc/>
    public override byte[]? GetDetectedSearchData() => DetectedSearchExpressionData;
}