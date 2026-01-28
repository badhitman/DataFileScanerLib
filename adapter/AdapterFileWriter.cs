////////////////////////////////////////////////
// © https://github.com/badhitman - @fakegov 
////////////////////////////////////////////////

namespace TextFileScannerLib;

/// <summary>
/// Adapter file writer
/// </summary>
public class AdapterFileWriter : AdapterFileScanner
{
    #region Событие, возникающее по мере выполнения процесса извлечения данных из файла
    /// <summary>
    /// Progress value changed handle action
    /// </summary>
    public delegate void ProgressValueChangedHandler(int percentage);
    /// <summary>
    /// Событие, возникающее по мере выполнения процесса извлечения данных из файла
    /// </summary>
    public event ProgressValueChangedHandler? ProgressValueChange;
    #endregion

    ///////////////////////////////////////////////////////////////////////////////////
    /// <summary>
    /// Поток файла результата
    /// </summary>
    protected FileStream? FileWriteStream { get; set; }

    /// <summary>
    /// Копирует часть данных из файла в новый файл с произвольной точки до произвольной точки
    /// </summary>
    /// <param name="StartPosition">Точка, с которой нужно копировать данные в новый файл</param>
    /// <param name="EndPosition">Точка, до которой нужно копировать данные в новый файл</param>
    /// <param name="destinationFileName">Файл, в который нужно записать данные</param>
    public void CopyData(long StartPosition, long EndPosition, string destinationFileName)
    {
        if (FileFilteredReadStream is null)
            throw new Exception($"{nameof(FileFilteredReadStream)} не инициализирован");

        // Запоминаем позицию курсора в файле, что бы потом вернуть его на место
        long current_position_of_stream = Position;
        //
        if (StartPosition < 0)
            StartPosition = 0;

        if (EndPosition > Length)
            EndPosition = Length;

        if (Length < 1 || StartPosition >= EndPosition)
            return;

        if (!Directory.Exists(Path.GetDirectoryName(destinationFileName)))
            Directory.CreateDirectory(destinationFileName);

        FileWriteStream = new FileStream(destinationFileName, FileMode.Create);

        int markerFlush = 0;
        long ActualPoint = 0;
        Position = StartPosition;
        while (Position < EndPosition)
        {
            FileWriteStream.WriteByte((byte)FileFilteredReadStream.ReadByte());

            markerFlush++;
            if (markerFlush >= 20)
            {
                FileWriteStream.Flush();
                markerFlush = 0;
                ProgressValueChange?.Invoke(((int)(ActualPoint / (Length / 100))));
            }
        }
        ProgressValueChange?.Invoke(((int)(ActualPoint / (Length / 100))));

        FileWriteStream.Close();
        Position = current_position_of_stream;
    }

    /// <summary>
    /// Создать файл из нескольких "склеив" их последовательно один за одним
    /// </summary>
    /// <param name="files">Файлы, которые требуется "склеить"</param>
    /// <param name="fileNameSave">Путь/Имя нового файла, который получиться путём объединения других файлов</param>
    public static void JoinFiles(string[] files, string fileNameSave)
    {
        ArgumentNullException.ThrowIfNull(files);

        FileStream stream_w = new(fileNameSave, FileMode.Create);
        BinaryWriter binary_w = new(stream_w);
        int BufferSize = 1024 * 64;
        //
        FileStream stream_r;
        BinaryReader binary_r;
        //
        foreach (string s in files)
        {
            stream_r = new FileStream(s, FileMode.Open, FileAccess.Read);
            using (binary_r = new BinaryReader(stream_r))
            {
                long startPointRead = 0, endPointRead = stream_r.Length;
                int markerFlush = 20, SizePartData = 0;
                while (startPointRead < endPointRead)
                {
                    if (endPointRead - startPointRead > BufferSize)
                    {
                        SizePartData = BufferSize;
                    }
                    else
                    {
                        SizePartData = (int)(endPointRead - startPointRead);
                    }
                    byte[] bytesForWrite = new byte[SizePartData];
                    binary_r.Read(bytesForWrite, 0, SizePartData);
                    binary_w.Write(bytesForWrite);

                    markerFlush++;
                    if (markerFlush >= 200)
                    {
                        binary_w.Flush();
                        stream_w.Flush();
                        markerFlush = 0;
                    }
                    startPointRead += SizePartData;
                }
                binary_w.Flush();
                stream_w.Flush();
                stream_r.Close();
            }
        }
        binary_w.Close();
        stream_w.Close();
    }
}