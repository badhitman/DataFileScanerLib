////////////////////////////////////////////////
// © https://github.com/badhitman 
////////////////////////////////////////////////
using TextFileScanerLib.Matches;
using System;
using System.Collections.Generic;
using TextFileScanerLib.scan.matches;
using System.Linq;
using System.Globalization;
using System.Resources;

namespace TextFileScanerLib.scan
{
    public class DataScanner
    {
        public List<byte> BufferBytes { get; } = new List<byte>();
        public string BufferAsString { get; private set; }
        public ScanResult ScanResult { get; private set; }
        public int MaxDataLengthBytes { get; private set; }
        public int MinDataLengthBytes { get; private set; }
        protected ResourceManager ResourceStringManager { get; private set; }

        #region MatchUnits
        //////////////////////////////////////////////////////////////////////////////////////////
        //

        /// <summary>
        /// Список искомых данных
        /// </summary>
        public List<AbstractMatchUnitCore> MatchUnits { get; } = new List<AbstractMatchUnitCore>();
        public int MatchUnitsCount => MatchUnits.Count;
        //public AbstractMatchUnit[] GetMatchUnits => MatchUnits.ToArray();
        public void ClearMatchUnits()
        {
            MatchUnits.Clear();
            ContainsTextSearchUnit = false;
            MatchUnitIsAddeded = false;
        }

        public bool AddMatchUnit(AbstractMatchUnitCore ThisMatchUnit)
        {
            if (ThisMatchUnit is null)
                throw new ArgumentNullException(nameof(ThisMatchUnit));

            // TODO: Провести тестирование быстродейтсвия разных методов поиска
            //if (ThisMatchUnit is MatchUnitText && !((MatchUnitText)ThisMatchUnit).IgnoreCase)
            //{
            //    MatchUnitText matchUnitText = (MatchUnitText)ThisMatchUnit;
            //    if (!matchUnitText.IgnoreCase)
            //        ThisMatchUnit = new MatchUnitBytes(AdapterFileReader.EncodingMode.GetBytes(matchUnitText.SearchExpression));
            //}

            if (MatchUnits.Contains(ThisMatchUnit))
                return false;

            MatchUnits.Add(ThisMatchUnit);
            MatchUnitIsAddeded = true;
            if (ThisMatchUnit.GetType().IsSubclassOf(typeof(AbstractMatchUnitText)))
                ContainsTextSearchUnit = true;

            MinDataLengthBytes = MinDataLengthBytes == default ? ThisMatchUnit.BufferSize : Math.Min(ThisMatchUnit.BufferSize, MinDataLengthBytes);
            MaxDataLengthBytes = Math.Max(ThisMatchUnit.BufferSize, MaxDataLengthBytes);

            return true;
        }

        public void AddMatchUnitRange(AbstractMatchUnitCore[] abstractMatchUnitCore)
        {
            if (abstractMatchUnitCore is null)
                throw new ArgumentNullException(nameof(abstractMatchUnitCore));

            foreach (AbstractMatchUnitCore x in abstractMatchUnitCore)
                AddMatchUnit(x);
        }

        //
        //////////////////////////////////////////////////////////////////////////////////////////
        #endregion

        bool ContainsTextSearchUnit = false;
        bool MatchUnitIsAddeded = false;
        public void AddToBuffer(int NewByte)
        {
            if (NewByte < 0)
                return;
            if (!MatchUnitIsAddeded)
                throw new Exception(ResourceStringManager.GetString("ExceptionListOfSearchUnitsCannotBeEmpty", CultureInfo.CurrentCulture));

            BufferBytes.Add((byte)NewByte);
            BufferAsString = ContainsTextSearchUnit ? AdapterFileReader.EncodingMode.GetString(BufferBytes.ToArray()) : string.Empty;
            CheckData();

            if (ScanResult.SuccessMatch)
            {
                if (ScanResult.MatchUnit.ReplacementData != null)
                {
                    List<byte> clear_data_list_bytes = new List<byte>();
                    if (ScanResult.MatchUnit.GetType().IsSubclassOf(typeof(AbstractMatchUnitText)))
                    {
                        clear_data_list_bytes.AddRange(AdapterFileReader.EncodingMode.GetBytes(BufferAsString.Substring(0, ScanResult.MatchUnit.IndexOf)));

                        if (ScanResult.MatchUnit.ReplacementData.Count > 0)
                            clear_data_list_bytes.AddRange(ScanResult.MatchUnit.ReplacementData);

                        if (ScanResult.MatchUnit is MatchUnitRegexp)
                            clear_data_list_bytes.AddRange(AdapterFileReader.EncodingMode.GetBytes(BufferAsString.Substring(ScanResult.MatchUnit.IndexOf + ((MatchUnitRegexp)ScanResult.MatchUnit).DetectedMatch.Length)));
                        else
                            clear_data_list_bytes.AddRange(AdapterFileReader.EncodingMode.GetBytes(BufferAsString.Substring(ScanResult.MatchUnit.IndexOf + ((MatchUnitText)ScanResult.MatchUnit).SearchExpression.Length)));
                    }
                    else
                    {
                        clear_data_list_bytes.AddRange(ScanResult.MatchUnit.GetDetectedSearchData().Take(ScanResult.MatchUnit.IndexOf));

                        if (ScanResult.MatchUnit.ReplacementData.Count > 0)
                            clear_data_list_bytes.AddRange(ScanResult.MatchUnit.ReplacementData);

                        if (ScanResult.MatchUnit.IndexOf + ScanResult.MatchUnit.ReplacementData.Count < BufferBytes.Count)
                            clear_data_list_bytes.AddRange(BufferBytes.Skip(ScanResult.MatchUnit.IndexOf + ScanResult.MatchUnit.ReplacementData.Count));
                    }

                    BufferBytes.Clear();
                    BufferBytes.AddRange(clear_data_list_bytes);
                    BufferAsString = ContainsTextSearchUnit ? AdapterFileReader.EncodingMode.GetString(BufferBytes.ToArray()) : string.Empty;
                }
            }
        }

        public virtual void CheckData()
        {
            ScanResult = new ScanResult();

            if (MinDataLengthBytes > BufferBytes.Count)
                return;

            foreach (AbstractMatchUnitCore match_unit in MatchUnits)
            {
                if (match_unit is MatchUnitText)
                    ((MatchUnitText)match_unit).Checking(BufferAsString);
                else if (match_unit is MatchUnitRegexp)
                    ((MatchUnitRegexp)match_unit).Checking(BufferAsString);
                else if (match_unit is MatchUnitBytes)
                    ((MatchUnitBytes)match_unit).Checking(BufferBytes.ToArray());
                else
                    throw new Exception(ResourceStringManager.GetString("ExceptionSearchUnitTypeNotDefined", CultureInfo.CurrentCulture));

                if (match_unit.SuccessMatch)
                {
                    ScanResult.MatchUnit = match_unit;

                    break;
                }
            }
        }
    }
}
