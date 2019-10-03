////////////////////////////////////////////////
// © https://github.com/badhitman 
////////////////////////////////////////////////
using System;
using System.Globalization;
using TextFileScanerLib.Matches;

namespace TextFileScanerLib.scan.matches
{
    public abstract class AbstractMatchUnitText : AbstractMatchUnitCore
    {
        /// <summary>
        /// Искомое выражение
        /// </summary>
        public string SearchExpression { get; protected set; }

        /// <summary>
        /// Признак регистро-независимости
        /// </summary>
        public bool IgnoreCase { get; set; }

        protected AbstractMatchUnitText(byte[] SetReplacementData = null) : base(SetReplacementData)
        {

        }

        public virtual void Checking(string TextForCheck)
        {
            IndexOf = -1;
        }

        public override bool Equals(object other)
        {
            if (!base.Equals(other))
                return false;

            //return base.Equals(other);
            AbstractMatchUnitText norm_other = (AbstractMatchUnitText)other;

            if (IgnoreCase && norm_other.IgnoreCase)
                return string.Equals(SearchExpression, norm_other.SearchExpression, StringComparison.CurrentCultureIgnoreCase);
            else
                return string.Equals(SearchExpression, norm_other.SearchExpression, StringComparison.CurrentCulture);

        }

        public override int GetHashCode()
        {
            return (base.GetHashCode().ToString(CultureInfo.InvariantCulture) + SearchExpression + IgnoreCase.ToString(CultureInfo.InvariantCulture)).GetHashCode();
        }
    }
}
