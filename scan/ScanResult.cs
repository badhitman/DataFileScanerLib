////////////////////////////////////////////////
// © https://github.com/badhitman 
////////////////////////////////////////////////
using TextFileScanerLib.Matches;

namespace TextFileScanerLib.scan
{
    /// <summary>
    /// Результат сканирования
    /// </summary>
    public class ScanResult
    {
        public bool SuccessMatch => MatchUnit is null ? false : MatchUnit.SuccessMatch;
        
        public AbstractMatchUnitCore MatchUnit { get; set; }
    }
}
