////////////////////////////////////////////////
// © https://github.com/badhitman - @fakegov 
////////////////////////////////////////////////

using TextFileScannerLib.scan;

namespace TextFileScannerLib;

/// <summary>
/// Adapter file scanner
/// </summary>
public class AdapterFileScanner : AdapterFileReader
{
    /// <summary>
    /// Tail bytes
    /// </summary>
    public List<byte> TailBytes { get; private set; } = [];

    /// <summary>
    /// Scanner
    /// </summary>
    public DataScanner Scanner { get; } = new DataScanner();

    void AddToBuffer(int curr_byte)
    {
        if (curr_byte < 0)
            return;
        TailBytes.Add((byte)curr_byte);
        Scanner.AddToBuffer((byte)curr_byte);

        Scanner.CheckData();
    }

    /// <summary>
    /// Find position data
    /// </summary>
    public long FindPositionData(long startPosition)
    {
        if(FileFilteredReadStream is null)
            throw new Exception($"{nameof(FileFilteredReadStream)} не инициализирован");

        if (Scanner.MinDataLengthBytes == 0)
            throw new Exception("Укажите данные поиска");

        TailBytes.Clear();
        Scanner.BufferBytes.Clear();
        FileFilteredReadStream.Scanner.BufferBytes.Clear();

        long found_position = -1;

        long original_position_of_stream = Position;

        long file_length = Length;

        long WorkingReadPosition = Position = startPosition;

        while (WorkingReadPosition <= file_length || this.FileFilteredReadStream.Scanner.BufferBytes.Count > 0)
        {
            int this_byte = FileFilteredReadStream.ReadByte();
            AddToBuffer(this_byte);
            if (Scanner.ScanResult != null && Scanner.ScanResult.SuccessMatch)
            {
                Position = Position - Scanner.ScanResult.MatchUnit!.GetDetectedSearchData()!.Length - this.FileFilteredReadStream.Scanner.BufferBytes.Count;
                found_position = Position;
                break;
            }
            WorkingReadPosition++;
        }

        Position = original_position_of_stream;
        return found_position;
    }

    /// <summary>
    /// Find data all
    /// </summary>
    public long[] FindDataAll(long startPosition)
    {
        List<long> indexes = [];
        while (true)
        {
            long match_index_position = FindPositionData(startPosition);
            if (match_index_position < 0)
                break;
            else
            {
                indexes.Add(match_index_position);
                startPosition += Scanner.ScanResult!.MatchUnit!.GetDetectedSearchData()!.Length;
            }
        }
        return [.. indexes];
    }
}