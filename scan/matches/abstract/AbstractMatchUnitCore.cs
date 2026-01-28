////////////////////////////////////////////////
// © https://github.com/badhitman - @fakegov 
////////////////////////////////////////////////

using System.Collections.ObjectModel;

namespace TextFileScannerLib.Matches;

/// <summary>
/// Abstract match unit core
/// </summary>
public abstract class AbstractMatchUnitCore
{
    /// <summary>
    /// Результат проверки
    /// </summary>
    public bool SuccessMatch => IndexOf > -1;

    /// <summary>
    /// Начальная позиция обнаруженного совпадения
    /// </summary>
    public int IndexOf { get; set; } = -1;

    /// <summary>
    /// данные, которыми нужно заменить найденные совпадения
    /// </summary>
    public ReadOnlyCollection<byte>? ReplacementData { get; }

    /// <summary>
    /// Размер буфера
    /// </summary>
    public int BufferSize { get; set; }

    /// <summary>
    /// Get detected search data
    /// </summary>
    public abstract byte[]? GetDetectedSearchData();

    /// <summary>
    /// Abstract match unit core
    /// </summary>
    protected AbstractMatchUnitCore(byte[]? setReplacementData)
    {
        if (setReplacementData != null)
            ReplacementData = new ReadOnlyCollection<byte>(setReplacementData);
    }

    #region сравнение объектов
    /// <inheritdoc/>
    public override bool Equals(object? other)
    {
        if (other is null || other.GetType() != GetType())
            return false;
        else
            return true;

    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        return GetType().Name.GetHashCode();
    }

    /// <inheritdoc/>
    public static bool operator ==(AbstractMatchUnitCore a1, AbstractMatchUnitCore a2)
    {
        if (a1 is null && a2 is null)
            return true;
        else if (a1 is null || a2 is null)
            return false;
        //
        return a1.Equals(a2);
    }

    /// <inheritdoc/>
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