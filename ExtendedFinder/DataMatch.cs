////////////////////////////////////////////////
// © https://github.com/badhitman 
////////////////////////////////////////////////
using System;
using System.Linq;

namespace DataFileScanerLib.ExtendedFinder
{
    /// <summary>
    /// Поисковый юнит
    /// </summary>
    public class DataMatch
    {
        /// <summary>
        /// Типы/статусы совпадений
        /// </summary>
        public enum MatchesResult
        {
            /// <summary>
            /// Достигнуто полное совпадение выражения
            /// </summary>
            IsFullMatch,

            /// <summary>
            /// Совпал очередной байт, но полного совпадения не достигнуто
            /// </summary>
            IsMatchThisByte,

            /// <summary>
            /// Очередной байт не совпал => FindIndexPosition будет сброшен в начало
            /// </summary>
            IsNotMatchThisByte
        }

        public string FindString { get; }
        public bool IgnoreCase { get; }

        public byte[][] FindData { get; }
        public int FindDataLength { get; }

        public int FindIndexPosition { get; set; } = 0;

        public DataMatch(string set_find_string, bool set_ignore_case)
        {
            if (string.IsNullOrEmpty(set_find_string))
                throw new ArgumentNullException(nameof(set_find_string));

            FindString = set_find_string;
            IgnoreCase = set_ignore_case;

            FindData = FileScanner.StringToSearchBytes(set_find_string, set_ignore_case);
            FindDataLength = FindData.Length;
        }

        public MatchesResult MatchNextByte(byte next_byte)
        {
            if (FindIndexPosition + 1 > FindDataLength)
                throw new ArgumentOutOfRangeException("Позиция проверки в искомых данных вышла за пределы доступного размера: FindData.Length = " + FindDataLength);

            if (!FindData[FindIndexPosition].Contains(next_byte))
            {
                if (FindIndexPosition > 0)
                {
                    FindIndexPosition = 0;
                    return MatchNextByte(next_byte);
                }

                return MatchesResult.IsNotMatchThisByte;
            }

            if (FindIndexPosition + 1 == FindDataLength)
            {
                FindIndexPosition = 0;
                return MatchesResult.IsFullMatch;
            }
            else
                FindIndexPosition++;

            return MatchesResult.IsMatchThisByte;
        }

        /// <summary>
        /// Сравнение объектов юнитов по поисковой фразе без учёта регистра. сравниваемые значения приводятся к единому регистру
        /// </summary>
        public override bool Equals(object other)
        {
            if (other is null || other.GetType() != this.GetType())
                return false;

            DataMatch norm_other = (DataMatch)other;

            return this.FindString.ToLower().Equals(norm_other.FindString.ToLower());
        }

        /// <summary>
        /// Get hash code
        /// </summary>
        /// <returns>Returns the hash code for this string.</returns>
        public override int GetHashCode()
        {
            if (string.IsNullOrEmpty(FindString))
                return 0;

            return (this.GetType().Name + this.FindString.ToString()).GetHashCode();
        }

        public static bool operator ==(DataMatch a1, DataMatch a2)
        {
            if (a1 is null && a2 is null)
                return true;
            else if (a1 is null || a2 is null)
                return false;
            //
            return a1.Equals(a2);
        }

        public static bool operator !=(DataMatch a1, DataMatch a2)
        {
            if (a1 is null && a2 is null)
                return true;
            else if (a1 is null || a2 is null)
                return false;
            //
            return !a1.Equals(a2);
        }
    }
}
