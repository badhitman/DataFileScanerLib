////////////////////////////////////////////////
// © https://github.com/badhitman - @fakegov 
////////////////////////////////////////////////

using TextFileScannerLib.Matches;
using TextFileScannerLib.scan.matches;

namespace TextFileScannerLib.scan;

/// <summary>
/// Data scanner
/// </summary>
public class DataScanner
{
    /// <summary>
    /// Filtered data counter
    /// </summary>
    public long FilteredDataCounter { get; set; }

    /// <summary>
    /// Buffer bytes
    /// </summary>
    public List<byte> BufferBytes { get; } = [];

    /// <summary>
    /// Buffer as string
    /// </summary>
    public string? BufferAsString { get; private set; }

    /// <summary>
    /// Scan result
    /// </summary>
    public ScanResult? ScanResult { get; private set; }

    /// <summary>
    /// Max data length bytes
    /// </summary>
    public int MaxDataLengthBytes { get; private set; }

    /// <summary>
    /// 
    /// </summary>
    public int MinDataLengthBytes { get; private set; }

    #region MatchUnits
    //////////////////////////////////////////////////////////////////////////////////////////
    //

    /// <summary>
    /// Список искомых данных
    /// </summary>
    private List<AbstractMatchUnitCore> MatchUnits { get; } = new List<AbstractMatchUnitCore>();

    /// <summary>
    /// MatchUnits.Count
    /// </summary>
    public int MatchUnitsCount => MatchUnits.Count;

    /// <summary>
    /// MatchUnits Array
    /// </summary>
    public AbstractMatchUnitCore[] GetMatchUnits() => [.. MatchUnits];

    /// <summary>
    /// Clear match units
    /// </summary>
    public void ClearMatchUnits()
    {
        MatchUnits.Clear();
        BufferBytes.Clear();
        MaxDataLengthBytes = 0;
        MinDataLengthBytes = 0;
        ScanResult = new ScanResult();
        BufferAsString = string.Empty;
        ContainsTextSearchUnit = false;
        MatchUnitIsAdded = false;
    }

    /// <summary>
    /// Add match unit
    /// </summary>
    public bool AddMatchUnit(AbstractMatchUnitCore ThisMatchUnit)
    {
        ArgumentNullException.ThrowIfNull(ThisMatchUnit);

        if (ThisMatchUnit is MatchUnitText text && !text.IgnoreCase)
            Console.WriteLine("Поисковый string-юнит с учётом регистра вероятно стоит заменить на bytes-юнит. Таким образом отпадает необходимость множественного преобразования строк в байты и обратно");

        if (MatchUnits.Contains(ThisMatchUnit))
            return false;

        MatchUnits.Add(ThisMatchUnit);
        MatchUnitIsAdded = true;
        if (ThisMatchUnit.GetType().IsSubclassOf(typeof(AbstractMatchUnitText)))
            ContainsTextSearchUnit = true;

        MinDataLengthBytes = MinDataLengthBytes == default ? ThisMatchUnit.BufferSize : Math.Min(ThisMatchUnit.BufferSize, MinDataLengthBytes);
        MaxDataLengthBytes = Math.Max(ThisMatchUnit.BufferSize, MaxDataLengthBytes);

        return true;
    }

    /// <summary>
    /// Add match unit range
    /// </summary>
    public void AddMatchUnitRange(AbstractMatchUnitCore[] abstractMatchUnitCore)
    {
        ArgumentNullException.ThrowIfNull(abstractMatchUnitCore);

        foreach (AbstractMatchUnitCore x in abstractMatchUnitCore)
            AddMatchUnit(x);
    }

    //
    //////////////////////////////////////////////////////////////////////////////////////////
    #endregion

    bool ContainsTextSearchUnit = false;
    bool MatchUnitIsAdded = false;

    /// <summary>
    /// Add to buffer
    /// </summary>
    public void AddToBuffer(int newByte)
    {
        if (newByte < 0)
            return;
        if (!MatchUnitIsAdded)
            throw new Exception("Список поисковых юнитов не может быть пустым");

        BufferBytes.Add((byte)newByte);
        BufferAsString = ContainsTextSearchUnit ? AdapterFileReader.EncodingMode.GetString(BufferBytes.ToArray()) : string.Empty;
        CheckData();

        if (ScanResult?.SuccessMatch == true)
        {
            if (ScanResult.MatchUnit?.ReplacementData != null)
            {
                List<byte> clear_data_list_bytes = [];
                if (ScanResult.MatchUnit.GetType().IsSubclassOf(typeof(AbstractMatchUnitText)))
                {
                    clear_data_list_bytes.AddRange(AdapterFileReader.EncodingMode.GetBytes(BufferAsString[..ScanResult.MatchUnit.IndexOf]));

                    if (ScanResult.MatchUnit.ReplacementData.Count > 0)
                        clear_data_list_bytes.AddRange(ScanResult.MatchUnit.ReplacementData);

                    if (ScanResult.MatchUnit is MatchUnitRegexp regexp)
                        clear_data_list_bytes.AddRange(AdapterFileReader.EncodingMode.GetBytes(BufferAsString[(ScanResult.MatchUnit.IndexOf + regexp.DetectedMatch!.Length)..]));
                    else
                        clear_data_list_bytes.AddRange(AdapterFileReader.EncodingMode.GetBytes(BufferAsString[(ScanResult.MatchUnit.IndexOf + ((MatchUnitText)ScanResult.MatchUnit).SearchExpression!.Length)..]));
                }
                else
                {
                    if (ScanResult.MatchUnit.IndexOf > 0)
                        clear_data_list_bytes.AddRange(BufferBytes.Take(ScanResult.MatchUnit.IndexOf));

                    if (ScanResult.MatchUnit.ReplacementData.Count > 0)
                        clear_data_list_bytes.AddRange(ScanResult.MatchUnit.ReplacementData);

                    if (ScanResult.MatchUnit.IndexOf + ScanResult.MatchUnit.GetDetectedSearchData()!.Length < BufferBytes.Count)
                        clear_data_list_bytes.AddRange(BufferBytes.Skip(ScanResult.MatchUnit.IndexOf + ScanResult.MatchUnit.ReplacementData.Count));
                }
                int filteredDataCount = BufferBytes.Count - clear_data_list_bytes.Count;

                FilteredDataCounter += filteredDataCount;
                BufferBytes.Clear();
                BufferBytes.AddRange(clear_data_list_bytes);
                BufferAsString = ContainsTextSearchUnit ? AdapterFileReader.EncodingMode.GetString(BufferBytes.ToArray()) : string.Empty;
            }
        }
    }

    /// <summary>
    /// Check data
    /// </summary>
    public virtual void CheckData()
    {
        ScanResult = new ScanResult();

        if (MinDataLengthBytes > BufferBytes.Count || BufferAsString is null)
            return;

        foreach (AbstractMatchUnitCore match_unit in MatchUnits)
        {
            if (match_unit is MatchUnitText text)
                text.Checking(BufferAsString);
            else if (match_unit is MatchUnitRegexp regexp)
                regexp.Checking(BufferAsString);
            else if (match_unit is MatchUnitBytes bytes)
                bytes.Checking([.. BufferBytes]);
            else
                throw new Exception("Тип поискового юнита не определён");

            if (match_unit.SuccessMatch)
            {
                ScanResult.MatchUnit = match_unit;

                break;
            }
        }
    }
}