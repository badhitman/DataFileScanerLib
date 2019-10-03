////////////////////////////////////////////////
// © https://github.com/badhitman 
////////////////////////////////////////////////
using System;
using System.Globalization;
using System.Linq;
using TextFileScanerLib.Matches;

namespace TextFileScanerLib.scan.matches
{
    public class MatchUnitBytes : AbstractMatchUnitCore
    {
        readonly byte[] SearchBytes;

        public MatchUnitBytes(byte[] SetSearchBytes, byte[] SetReplacementData = null) : base(SetReplacementData)
        {
            if (SetSearchBytes is null || SetSearchBytes.Length == 0)
                throw new ArgumentNullException(nameof(SetSearchBytes), ResourceStringManager.GetString("ExceptionEnterYourSearchInformation", CultureInfo.CurrentCulture));

            SearchBytes = SetSearchBytes;
            BufferSize = SetSearchBytes.Length;
        }

        public void Checking(byte[] BytesForCheck)
        {
            if (BytesForCheck is null)
                throw new ArgumentNullException(nameof(BytesForCheck));

            int[] indexes = BytesForCheck.StartingIndex(SearchBytes).ToArray();
            if (indexes.Length == 0)
                IndexOf = -1;
            else
                IndexOf = indexes[0];
        }

        public override byte[] GetDetectedSearchData() => SearchBytes;
    }
}
