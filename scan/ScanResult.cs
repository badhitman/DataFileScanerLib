////////////////////////////////////////////////
// © https://github.com/badhitman - @fakegov 
////////////////////////////////////////////////

using TextFileScannerLib.Matches;

namespace TextFileScannerLib.scan;

/// <summary>
/// Результат сканирования
/// </summary>
public class ScanResult
{
    /// <summary>
    /// MatchUnit is not null AND MatchUnit.SuccessMatch
    /// </summary>
    public bool SuccessMatch => MatchUnit is not null && MatchUnit.SuccessMatch;

    /// <summary>
    /// MatchUnit
    /// </summary>
    public AbstractMatchUnitCore? MatchUnit { get; set; }
}