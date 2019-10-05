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
        public List<byte> TailBytes { get; private set; } = new List<byte>();

        public DataScanner Scanner { get; } = new DataScanner();

        private void AddToBuffer(int curr_byte)
        {
            if (curr_byte < 0)
                return;
            TailBytes.Add((byte)curr_byte);
            Scanner.AddToBuffer((byte)curr_byte);

            Scanner.CheckData();
        }

        public long FindPositionData(long StartPosition)
        {
            if (Scanner.MinDataLengthBytes == 0)
                throw new Exception(ResourceStringManager.GetString("ExceptionEnterYourSearchInformation", CultureInfo.CurrentCulture));

            TailBytes.Clear();
            long finded_position = -1;

            long original_position_of_stream = Position;

            long file_length = Length;

            long WorkingReadPosition = Position = StartPosition;
            Scanner.BufferBytes.Clear();

            while (WorkingReadPosition <= file_length)
            {
                int this_byte = FileFilteredReadStream.ReadByte();
                AddToBuffer(this_byte);
                if (Scanner.ScanResult != null && Scanner.ScanResult.SuccessMatch)
                {
                    Position = Position - Scanner.ScanResult.MatchUnit.GetDetectedSearchData().Length - this.FileFilteredReadStream.Scanner.BufferBytes.Count;
                    finded_position = Position;
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
