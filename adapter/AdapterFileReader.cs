////////////////////////////////////////////////
// © https://github.com/badhitman - @fakegov 
////////////////////////////////////////////////

using System.Text;

namespace TextFileScannerLib;

/// <summary>
/// Класс работы с файлами. Нарезка, склейка ...
/// </summary>
public class AdapterFileReader : IDisposable
{
    /// <summary>
    /// Исходный файл
    /// </summary>
    public FilteredTextFileReader? FileFilteredReadStream { get; set; }

    /// <summary>
    /// Bytes to HEX
    /// </summary>
    public static string BytesToHEX(byte[] bytes) => BitConverter.ToString(bytes);

    /// <summary>
    /// String to HEX
    /// </summary>
    public static string StringToHEX(string originalString) => BytesToHEX(EncodingMode.GetBytes(originalString));

    /// <summary>
    /// Hex to Byte
    /// </summary>
    public static byte[] HexToByte(string convertibleString)
    {
        if (string.IsNullOrEmpty(convertibleString))
            throw new ArgumentNullException(nameof(convertibleString));

        return [.. convertibleString.Split('-').Select(b => Convert.ToByte(b, 16))];
    }

    #region read data in files
    /// <summary>
    /// Read data about position as string
    /// </summary>
    public string ReadDataAboutPositionAsString(long position, int sizeArea)
    {
        List<byte> bytes = [.. ReadBaseBytes(position - sizeArea, position)];
        bytes.AddRange(ReadBaseBytes(position, position + sizeArea));
        return EncodingMode.GetString([.. bytes]);
    }

    /// <summary>
    /// Возвращает массив байт слева и справа от указанной точки указанного размера в байтах
    /// </summary>
    /// <param name="position">Точка от которой читать данные</param>
    /// <param name="sizeArea">Желаемый размер данных в каждом из направлений от точки (в начало и в конец)</param>
    public Dictionary<ReadingDirection, byte[]> ReadDataAboutPosition(long position, int sizeArea) => new()
    {
        { ReadingDirection.Left, ReadBaseBytes(position - sizeArea, position) },
        { ReadingDirection.Right, ReadBaseBytes(position, position + sizeArea) }
    };

    /// <summary>
    /// Читает и возвращает массив байт из файла (без фильтров чтения). Если начальная точка больше или равна конечной точки, то возвращается пустой массив байт.
    /// </summary>
    /// <param name="startPosition">Начальная точка чтения байт. Если меньше нуля, то читает с начала файла (с позиции 0). Если точка больше размера файла, то возвращается пустой массив байт.</param>
    /// <param name="endPosition">Конечная точка чтения байт. Если точка больше размера файла, то читается до конца файла</param>
    /// <returns>Возвращает массив байт из файла с произвольной точки до произвольной точки</returns>
    public byte[] ReadBaseBytes(long startPosition, long endPosition)
    {
        if (FileFilteredReadStream is null)
            return [];

        // Запоминаем позицию курсора в файле, что бы потом вернуть его на место
        long current_position_of_stream = Position;
        //
        if (startPosition < 0)
            startPosition = 0;

        if (endPosition > Length)
            endPosition = Length;

        if (Length < 1 || startPosition >= endPosition)
            return [];

        byte[] returned_data = new byte[endPosition - startPosition];

        Position = startPosition;

        FileFilteredReadStream.DisableFilters = false;
        for (int i = 0; i < returned_data.Length; i++)
            returned_data[i] = (byte)FileFilteredReadStream.ReadByte();
        FileFilteredReadStream.DisableFilters = true;

        Position = current_position_of_stream;
        return returned_data;
    }
    #endregion

    /// <summary>
    /// Текущая позиция в исходном файле
    /// </summary>
    public long Position
    {
        get
        {
            return (FileFilteredReadStream is null || !FileFilteredReadStream.CanRead) ? -1 : FileFilteredReadStream.Position;
        }
        set
        {
            if (FileFilteredReadStream is null || !FileFilteredReadStream.CanRead)
                return;

            if (value < 0)
                FileFilteredReadStream.Position = 0;
            else if (value > Length)
                FileFilteredReadStream.Position = Length;
            else
                FileFilteredReadStream.Position = value;
        }
    }

    /// <summary>
    /// Размер исходного файла
    /// </summary>
    public long Length => (FileFilteredReadStream is null || !FileFilteredReadStream.CanRead) ? -1 : FileFilteredReadStream.Length;

    /// <summary>
    /// Открыть для чтения файл
    /// </summary>
    /// <param name="pathFile">Путь к файлу для чтения/обработки</param>
    public void OpenFile(string pathFile)
    {
        CloseFile();
        //
        FileFilteredReadStream = new FilteredTextFileReader(pathFile, FileMode.Open, FileAccess.Read);
    }

    /// <summary>
    /// Закрыть оригинальный файл (если открыт)
    /// </summary>
    public void CloseFile()
    {
        FileFilteredReadStream?.Close();
        FileFilteredReadStream?.Dispose();
        FileFilteredReadStream = null;
    }

    /// <summary>
    /// Dispose
    /// </summary>
    public void Dispose()
    {
        CloseFile();
        GC.SuppressFinalize(this);
    }

    #region Encoding
    /// <summary>
    /// Режим кодировки данных
    /// </summary>
    public static Encoding EncodingMode { get; protected set; } = Encoding.UTF8;

    /// <summary>
    /// Определить кодировку по имени
    /// </summary>
    /// <param name="encodingName">Имя кодировки</param>
    /// <returns>Указатель кодировки, определённой по строке имени</returns>
    public static Encoding DetectEncoding(string encodingName)
    {
        return encodingName.ToLower() switch
        {
            "utf8" => Encoding.UTF8,
            "ascii" => Encoding.ASCII,
            "unicode" => Encoding.Unicode,
            "bigendianunicode" => Encoding.BigEndianUnicode,
            "utf32" => Encoding.UTF32,
            _ => Encoding.Default,
        };
    }

    /// <summary>
    /// Установить кодировку работы с файлом. При попытке установить NULL -> установится по умолчанию: Encoding.UTF8
    /// </summary>
    public static void SetEncoding(Encoding? encoding = null)
    {
        if (encoding is null)
            EncodingMode = Encoding.UTF8;
        else
            EncodingMode = encoding;
    }

    /// <summary>
    /// Set encoding
    /// </summary>
    public static void SetEncoding(string encodingName) => SetEncoding(DetectEncoding(encodingName));
    #endregion
}