////////////////////////////////////////////////
// © https://github.com/badhitman - @fakegov 
////////////////////////////////////////////////

using TextFileScannerLib.scan;

namespace TextFileScannerLib;

/// <summary>
/// Чтение файла через фильтр. Настроив фильтрующий сканер в последствии при чтении из этого файла фильтруемые данные не будут прочитаны (будут пропущены)
/// Фильтры могут быть трёх типов: строковой фильтр, фильтр регулярного выражения (regex) или фильтр данных (byte[])
/// </summary>
public class FilteredTextFileReader(string path, FileMode mode, FileAccess access) : IDisposable
{
    readonly Stream TextFileStream = new FileStream(path, mode, access);

    /// <summary>
    /// Фильтрующий сканер. Данные, распознанные сканером, пропадут в выходном потоке [ReadByte()] так, как буд-то их не было вовсе
    /// </summary>
    public DataScanner Scanner { get; } = new DataScanner();

    /// <summary>
    /// Принудительное указание отключить фильтр
    /// </summary>
    public bool DisableFilters { get; set; } = false;

    /// <summary>
    /// TextFileStream.CanRead
    /// </summary>
    public bool CanRead => TextFileStream is not null && TextFileStream.CanRead;

    /// <summary>
    /// TextFileStream.Length 
    /// </summary>
    public long Length => TextFileStream is null ? -1 : TextFileStream.Length;

    /// <summary>
    /// TextFileStream.Position
    /// </summary>
    public long Position
    {
        get => TextFileStream.Position;
        set => TextFileStream.Position = value;
    }

    /// <summary>
    /// FileStream.Name
    /// </summary>
    public string Name => TextFileStream is FileStream stream ? stream.Name : "";

    /// <summary>
    /// Read byte
    /// </summary>
    public int ReadByte()
    {
        int ret_val = TextFileStream.ReadByte();
        if (DisableFilters || Scanner.MatchUnitsCount == 0)
            return ret_val;

        if (ret_val < 0)
        {
            if (Scanner.BufferBytes.Count == 0)//this.FileFilteredReadStream.Scanner.BufferBytes
                return ret_val;
            else
            {
                ret_val = Scanner.BufferBytes[0];
                Scanner.BufferBytes.RemoveAt(0);

                return ret_val;
            }
        }

        Scanner.AddToBuffer(ret_val);
        while (Scanner.BufferBytes.Count < Scanner.MaxDataLengthBytes)
        {
            ret_val = TextFileStream.ReadByte();
            if (ret_val < 0)
                break;

            Scanner.AddToBuffer(ret_val);
        }

        ret_val = Scanner.BufferBytes[0];
        Scanner.BufferBytes.RemoveAt(0);

        return ret_val;
    }

    /// <summary>
    /// Close
    /// </summary>
    public void Close()
    {
        TextFileStream.Close();
        TextFileStream.Dispose();
    }

    /// <summary>
    /// Dispose
    /// </summary>
    public void Dispose()
    {
        TextFileStream.Close();
        TextFileStream.Dispose();

        GC.SuppressFinalize(this);
    }
}