////////////////////////////////////////////////
// © https://github.com/badhitman 
////////////////////////////////////////////////
using System.Collections.ObjectModel;

namespace TextFileScanerLib.Matches
{
    public abstract class AbstractMatchUnitCore
    {
        /// <summary>
        /// Результат проверки
        /// </summary>
        public bool SuccessMatch => IndexOf > -1;

        /// <summary>
        /// Начальная позиция обнаруженого совпадения
        /// </summary>
        public int IndexOf { get; set; } = -1;

        public ReadOnlyCollection<byte> ReplacementData { get; } = null;

        public int BufferSize { get; set; }

        public abstract byte[] GetDetectedSearchData();

        protected AbstractMatchUnitCore(byte[] SetReplacementData)
        {
            if (SetReplacementData != null)
                ReplacementData = new ReadOnlyCollection<byte>(SetReplacementData);
        }

        #region сравнение объектов

        /// <summary>
        /// Сравнение объектов юнитов по поисковой фразе. без учёта регистра
        /// </summary>
        public override bool Equals(object other)
        {
            if (other is null || other.GetType() != this.GetType())
                return false;
            else
                return true;

        }

        /// <summary>
        /// Get hash code
        /// </summary>
        /// <returns>Returns the hash code for this string.</returns>
        public override int GetHashCode()
        {
            return GetType().Name.GetHashCode();
        }

        public static bool operator ==(AbstractMatchUnitCore a1, AbstractMatchUnitCore a2)
        {
            if (a1 is null && a2 is null)
                return true;
            else if (a1 is null || a2 is null)
                return false;
            //
            return a1.Equals(a2);
        }

        public static bool operator !=(AbstractMatchUnitCore a1, AbstractMatchUnitCore a2)
        {
            if (a1 is null && a2 is null)
                return true;
            else if (a1 is null || a2 is null)
                return false;
            //
            return !a1.Equals(a2);
        }

        #endregion
    }
}
