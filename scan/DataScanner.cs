////////////////////////////////////////////////
// © https://github.com/badhitman 
////////////////////////////////////////////////
using TextFileScanerLib.Matches;
using System;
using System.Collections.Generic;
using TextFileScanerLib.scan.matches;
using System.Linq;

namespace TextFileScanerLib.scan
{
    public class DataScanner
    {
        public long FilteredDataCounter { get; set; }
        public List<byte> BufferBytes { get; } = new List<byte>();
        public string BufferAsString { get; private set; }
        public ScanResult ScanResult { get; private set; }
        public int MaxDataLengthBytes { get; private set; }
        public int MinDataLengthBytes { get; private set; }

        #region MatchUnits
        //////////////////////////////////////////////////////////////////////////////////////////
        //

        /// <summary>
        /// Список искомых данных
        /// </summary>
        private List<AbstractMatchUnitCore> MatchUnits { get; } = new List<AbstractMatchUnitCore>();
        public int MatchUnitsCount => MatchUnits.Count;
        public AbstractMatchUnitCore[] GetMatchUnits() => MatchUnits.ToArray();
        public void ClearMatchUnits()
        {
            MatchUnits.Clear();
            BufferBytes.Clear();
            MaxDataLengthBytes = 0;
            MinDataLengthBytes = 0;
            ScanResult = new ScanResult();
            BufferAsString = string.Empty;
            ContainsTextSearchUnit = false;
            MatchUnitIsAddeded = false;
        }

        public bool AddMatchUnit(AbstractMatchUnitCore ThisMatchUnit)
        {
            if (ThisMatchUnit is null)
                throw new ArgumentNullException(nameof(ThisMatchUnit));

            if (ThisMatchUnit is MatchUnitText && !((MatchUnitText)ThisMatchUnit).IgnoreCase)
                Console.WriteLine("Поисковый string-юнит с учётом регистра вероятно стоит заменить на bytes-юнит. Таким образом отпадает необходимость множественного преобразования строк в байты и обратно");

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
                throw new Exception("Список поисковых юнитов не может быть пустым");

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
                        if (ScanResult.MatchUnit.IndexOf > 0)
                            clear_data_list_bytes.AddRange(BufferBytes.Take(ScanResult.MatchUnit.IndexOf));

                        if (ScanResult.MatchUnit.ReplacementData.Count > 0)
                            clear_data_list_bytes.AddRange(ScanResult.MatchUnit.ReplacementData);

                        if (ScanResult.MatchUnit.IndexOf + ScanResult.MatchUnit.GetDetectedSearchData().Length < BufferBytes.Count)
                            clear_data_list_bytes.AddRange(BufferBytes.Skip(ScanResult.MatchUnit.IndexOf + ScanResult.MatchUnit.ReplacementData.Count));
                    }
                    int filteredDataCount = BufferBytes.Count - clear_data_list_bytes.Count;
                    
                    FilteredDataCounter += filteredDataCount;
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
                    throw new Exception("Тип поискового юнита не определён");

                if (match_unit.SuccessMatch)
                {
                    ScanResult.MatchUnit = match_unit;

                    break;
                }
            }
        }
    }
}
