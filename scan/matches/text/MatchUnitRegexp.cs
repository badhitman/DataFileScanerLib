////////////////////////////////////////////////
// © https://github.com/badhitman 
////////////////////////////////////////////////
using System;
using System.Globalization;
using System.Text.RegularExpressions;
using TextFileScanerLib.scan.matches;

namespace TextFileScanerLib.Matches
{
    public class MatchUnitRegexp : AbstractMatchUnitText
    {
        public Regex SearchRegex { get; }

        public Match DetectedMatch { get; private set; }

        private byte[] DetectedSearchExpressionData { get; set; }

        public MatchUnitRegexp(Regex regex, int SetBufferSize, byte[] SetReplacementData = null) : base(SetReplacementData)
        {
            if (regex is null)
                throw new ArgumentNullException(nameof(regex));

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

        public override void Checking(string data_for_check)
        {
            DetectedSearchExpressionData = Array.Empty<byte>();
            base.Checking(data_for_check);
            DetectedMatch = SearchRegex.Match(data_for_check);
            if (DetectedMatch.Success)
            {
                IndexOf = DetectedMatch.Index;
                DetectedSearchExpressionData = AdapterFileReader.EncodingMode.GetBytes(DetectedMatch.Value);
            }
        }

        public override byte[] GetDetectedSearchData() => DetectedSearchExpressionData;
    }
}
