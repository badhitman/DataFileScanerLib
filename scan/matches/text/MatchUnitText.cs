////////////////////////////////////////////////
// © https://github.com/badhitman 
////////////////////////////////////////////////
using System;
using System.Globalization;
using TextFileScanerLib.scan.matches;

namespace TextFileScanerLib.Matches
{
    public class MatchUnitText : AbstractMatchUnitText
    {
        public MatchUnitText(string SearchString, bool SetIgnoreCase, byte[] SetReplacementData = null) : base(SetReplacementData)
        {
            if (string.IsNullOrEmpty(SearchString) || SearchString.Length == 0)
                throw new ArgumentOutOfRangeException(nameof(SearchString), ResourceStringManager.GetString("ExceptionMissingSearchData", CultureInfo.CurrentCulture));

            SearchExpression = SearchString;
            IgnoreCase = SetIgnoreCase;
            BufferSize = AdapterFileReader.EncodingMode.GetByteCount(SearchString);
        }

        public override void Checking(string StringForCheck)
        {
            if (StringForCheck is null)
                throw new ArgumentNullException(nameof(StringForCheck));

            base.Checking(StringForCheck);

            if (IgnoreCase)
                IndexOf = StringForCheck.ToLower(CultureInfo.CurrentCulture).IndexOf(SearchExpression.ToLower(System.Globalization.CultureInfo.CurrentCulture), StringComparison.CurrentCulture);
            else
                IndexOf = StringForCheck.IndexOf(SearchExpression, StringComparison.CurrentCulture);

        }

        public override byte[] GetDetectedSearchData() => AdapterFileReader.EncodingMode.GetBytes(SearchExpression);
    }
}
