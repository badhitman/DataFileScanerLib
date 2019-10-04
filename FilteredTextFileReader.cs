////////////////////////////////////////////////
// © https://github.com/badhitman 
////////////////////////////////////////////////
using TextFileScanerLib.scan;
using System;
using System.IO;

namespace TextFileScanerLib
{
    /// <summary>
    /// Чтение файла через фильтр. Настроив фильтрующий сканер в последствии при чтении из этого файла фильтруемые данные не будут прочитаны (будут пропущены)
    /// Фильтры могут быть трёх типов: строковой фильтр, фильтр регулярного выражения (regex) или фильтр данных (byte[])
    /// </summary>
    public class FilteredTextFileReader : IDisposable
    {
        readonly Stream TextFileStream;
        /// <summary>
        /// Фильтрующий сканер. Данные, распознанные сканером, пропадут в выходном потоке [ReadByte()] так, какбуд-то их небыло вовсе
        /// </summary>
        public DataScanner Scanner { get; } = new DataScanner();

        /// <summary>
        /// Принудительное указание отключить фильтр
        /// </summary>
        public bool DisableFilters { get; set; } = false;
        public bool CanRead => TextFileStream is null ? false : TextFileStream.CanRead;
        public long Length => TextFileStream is null ? -1 : TextFileStream.Length;

        public long Position
        {
            get => TextFileStream.Position;
            set=> TextFileStream.Position = value;
        }

        public string Name => TextFileStream is FileStream ? ((FileStream)TextFileStream).Name : "";

        public FilteredTextFileReader(string path, FileMode mode, FileAccess access)
        {
            TextFileStream = new FileStream(path, mode, access);
        }

        public int ReadByte()
        {
            int ret_val = TextFileStream.ReadByte();
            if (DisableFilters || Scanner.MatchUnitsCount == 0)
                return ret_val;

            if (ret_val < 0)
            {
                if (Scanner.BufferBytes.Count == 0)
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

        public void Close()
        {
            TextFileStream.Close();
            TextFileStream.Dispose();
        }

        protected virtual void Dispose(bool disposing)
        {
            TextFileStream.Close();
            TextFileStream.Dispose();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
