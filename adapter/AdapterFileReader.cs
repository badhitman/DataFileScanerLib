////////////////////////////////////////////////
// © https://github.com/badhitman - @fakegov 
////////////////////////////////////////////////

using System.Text;

namespace TextFileScannerLib;

/// <summary>
/// Класс работы с файлами. Нарезка, склейка ...
/// </summary>
public class AdapterFileReader
{
    /// <summary>
    /// Исходный файл
    /// </summary>
    public required FilteredTextFileReader FileFilteredReadStream { get; set; }

    /// <summary>
    /// Bytes to HEX
    /// </summary>
    public static string BytesToHEX(byte[] bytes) => BitConverter.ToString(bytes);

    /// <summary>
    /// String to HEX
    /// </summary>
    public static string StringToHEX(string OriginalString) => BytesToHEX(EncodingMode.GetBytes(OriginalString));

    /// <summary>
    /// Hex to Byte
    /// </summary>
    public static byte[] HexToByte(string ConvertibleString)
    {
        if (string.IsNullOrEmpty(ConvertibleString))
            throw new ArgumentNullException(nameof(ConvertibleString));

        return ConvertibleString.Split('-').Select(b => Convert.ToByte(b, 16)).ToArray();
    }

    #region read data in files

    /// <summary>
    /// Read data about position as string
    /// </summary>
    public string ReadDataAboutPositionAsString(long position, int SizeArea)
    {
        List<byte> bytes = new(ReadBaseBytes(position - SizeArea, position));
        bytes.AddRange(ReadBaseBytes(position, position + SizeArea));
        return EncodingMode.GetString(bytes.ToArray());
    }

    /// <summary>
    /// Возвращает массив байт слева и справа от указанной точки указанного размера в байтах
    /// </summary>
    /// <param name="position">Точка от которой читать данные</param>
    /// <param name="SizeArea">Желаемый размер данных в каждом из направлений от точки (в начало и в конец)</param>
    public Dictionary<ReadingDirection, byte[]> ReadDataAboutPosition(long position, int SizeArea) => new()
    {
        { ReadingDirection.Left, ReadBaseBytes(position - SizeArea, position) },
        { ReadingDirection.Right, ReadBaseBytes(position, position + SizeArea) }
    };

    /// <summary>
    /// Читает и возвращает массив байт из файла (без фильтров чтения). Если начальная точка больше или равна конечной точки, то возвращается пустой массив байт.
    /// </summary>
    /// <param name="StartPosition">Начальная точка чтения байт. Если меньше нуля, то читает с начала файла (с позиции 0). Если точка больше размера файла, то возвращается пустой массив байт.</param>
    /// <param name="EndPosition">Конечная точка чтения байт. Если точка больше размера файла, то читается до конца файла</param>
    /// <returns>Возвращает массив байт из файла с произвольной точки до произвольной точки</returns>
    public byte[] ReadBaseBytes(long StartPosition, long EndPosition)
    {
        // Запоминаем позицию курсора в файле, что бы потом вернуть его на место
        long current_position_of_stream = Position;
        //
        if (StartPosition < 0)
            StartPosition = 0;

        if (EndPosition > Length)
            EndPosition = Length;

        if (Length < 1 || StartPosition >= EndPosition)
            return [];

        byte[] returned_data = new byte[EndPosition - StartPosition];

        Position = StartPosition;

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
    /// <param name="PathFile">Путь к файлу для чтения/обработки</param>
    public void OpenFile(string PathFile)
    {
        CloseFile();
        //
        FileFilteredReadStream = new FilteredTextFileReader(PathFile, FileMode.Open, FileAccess.Read);
    }

    /// <summary>
    /// Закрыть оригинальный файл (если открыт)
    /// </summary>
    public void CloseFile()
    {
        FileFilteredReadStream?.Close();
    }

    #region Encoding

    /// <summary>
    /// Режим кодировки данных
    /// </summary>
    public static Encoding EncodingMode { get; protected set; } = Encoding.UTF8;

    /// <summary>
    /// Определить кодировку по имени
    /// </summary>
    /// <param name="EncodingName">Имя кодировки</param>
    /// <returns>Указатель кодировки, определённой по строке имени</returns>
    public static Encoding DetectEncoding(string EncodingName)
    {
        return (EncodingName?.ToLower(System.Globalization.CultureInfo.CurrentCulture)) switch
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
    public static void SetEncoding(string EncodingName) => SetEncoding(DetectEncoding(EncodingName));

    #endregion
}