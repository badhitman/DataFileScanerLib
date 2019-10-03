////////////////////////////////////////////////
// © https://github.com/badhitman 
////////////////////////////////////////////////
using TextFileScanerLib.scan;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace TextFileScanerLib
{
    public class AdapterFileScanner : AdapterFileReader
    {
        public DataScanner Scanner { get; } = new DataScanner();

        private void AddToBuffer(int curr_byte)
        {
            if (curr_byte < 0)
                return;

            Scanner.AddToBuffer((byte)curr_byte);

            Scanner.CheckData();
        }

        public long FindPositionData(long StartPosition)
        {
            if (Scanner.MinDataLengthBytes == 0)
                throw new Exception(ResourceStringManager.GetString("ExceptionEnterYourSearchInformation", CultureInfo.CurrentCulture));

            long finded_position = -1;

            long original_position_of_stream = Position;

            long file_length = Length;

            long WorkingReadPosition = Position = StartPosition;
            Scanner.BufferBytes.Clear();

            while (WorkingReadPosition + Scanner.BufferBytes.Count <= file_length)
            {
                int this_byte = FileReadStream.ReadByte();
                AddToBuffer(this_byte);
                if (Scanner.ScanResult != null && Scanner.ScanResult.SuccessMatch)
                {
                    finded_position = Position- Scanner.ScanResult.MatchUnit.GetDetectedSearchData().Length;
                    break;
                }
                WorkingReadPosition++;
            }

            Position = original_position_of_stream;
            return finded_position;
        }

        public long[] FindDataAll(long StartPosition)
        {
            List<long> indexes = new List<long>();
            while (true)
            {
                long match_index_position = FindPositionData(StartPosition);
                if (match_index_position < 0)
                    break;
                else
                {
                    indexes.Add(match_index_position);
                    StartPosition += Scanner.ScanResult.MatchUnit.GetDetectedSearchData().Length;
                }
            }
            return indexes.ToArray();
        }
    }
}
