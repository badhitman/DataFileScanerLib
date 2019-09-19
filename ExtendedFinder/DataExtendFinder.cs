////////////////////////////////////////////////
// © https://github.com/badhitman 
////////////////////////////////////////////////
using System;
using System.Collections.Generic;
using System.Linq;
using static DataFileScanerLib.ExtendedFinder.DataMatch;

namespace DataFileScanerLib.ExtendedFinder
{
    /// <summary>
    /// Поисковый менеджер
    /// </summary>
    public class DataExtendFinder
    {
        public List<DataMatch> DataMatches = new List<DataMatch>();

        private int p_MinDataLength = int.MaxValue;
        /// <summary>
        /// Минимальный размер данных для возможности положительного результата. Данные размером меньше чем это значение выходят за допустимые границы и не требуют проверки
        /// </summary>
        public int MinDataLength
        {
            get
            {
                if (p_MinDataLength == int.MaxValue)
                    return 0;
                else
                    return p_MinDataLength;
            }
        }
        //
        public int MaxSizeFindedData => DataMatches.Max(x => x.FindDataLength);

        public DataMatch AddFindData(string find_string, bool ignore_case)
        {
            ////////////////////////////////////////
            // нельзя искать пустое значение
            if (string.IsNullOrEmpty(find_string))
                return null;

            DataMatch dataMatch = new DataMatch(find_string, ignore_case);

            ////////////////////////////////////////
            // нельзя одинаковые строки добавлять
            if (DataMatches.Contains(dataMatch))
                return null;


            DataMatches.Add(dataMatch);

            return dataMatch;
        }

        public ResultScanning ScanningNextByte(byte next_read_byte)
        {
            ResultScanning result = new ResultScanning();

            if (DataMatches.Count == 0)
            {
                result.Error = "Сканирование невозможно. Данные для поиска отсусвуют.";
                return result;
            }

            p_MinDataLength = 0;
            int p_min_data_length = MaxSizeFindedData;
            foreach (DataMatch m in DataMatches)
            {
                if (m.MatchNextByte(next_read_byte) == MatchesResult.IsFullMatch)
                {
                    DataMatches.ForEach(x => x.FindIndexPosition = 0); // сброс всех указателей в очереди сравнения
                    result.MatchData = m;
                    break;
                }
                p_min_data_length = Math.Min(p_min_data_length, m.FindDataLength - m.FindIndexPosition);
            }
            p_MinDataLength = p_min_data_length;
            return result;
        }
    }
}
