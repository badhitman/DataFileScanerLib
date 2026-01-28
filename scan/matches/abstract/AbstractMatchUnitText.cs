////////////////////////////////////////////////
// © https://github.com/badhitman - @fakegov 
////////////////////////////////////////////////

using System.Globalization;
using TextFileScannerLib.Matches;

namespace TextFileScannerLib.scan.matches;

/// <summary>
/// AbstractMatchUnitText
/// </summary>
public abstract class AbstractMatchUnitText(byte[]? setReplacementData = null) : AbstractMatchUnitCore(setReplacementData)
{
    /// <summary>
    /// Искомое выражение
    /// </summary>
    public required string SearchExpression { get; set; }

    /// <summary>
    /// Признак регистро-независимости
    /// </summary>
    public bool IgnoreCase { get; set; }

    /// <summary>
    /// Checking
    /// </summary>
    public virtual void Checking(string textForCheck)
    {
        IndexOf = -1;
    }

    /// <inheritdoc/>
    public override bool Equals(object? other)
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

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        return (base.GetHashCode().ToString(CultureInfo.InvariantCulture) + SearchExpression + IgnoreCase.ToString(CultureInfo.InvariantCulture)).GetHashCode();
    }
}