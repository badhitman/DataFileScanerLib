////////////////////////////////////////////////
// © https://github.com/badhitman 
////////////////////////////////////////////////

namespace DataFileScanerLib.ExtendedFinder
{
    public class ResultScanning
    {
        public bool IsMatch => !(MatchData is null && string.IsNullOrEmpty(Error));
        public string MatchedString => MatchData?.FindString;

        public DataMatch MatchData { get; set; }
        public string Error { get; set; }
        public long IndexOf { get; set; }
    }
}
