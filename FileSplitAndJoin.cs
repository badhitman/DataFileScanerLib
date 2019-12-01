////////////////////////////////////////////////
// © https://github.com/badhitman 
////////////////////////////////////////////////
using System.IO;
using System.Globalization;

namespace TextFileScanerLib
{
    public class FileSplitAndJoin : AdapterFileWriter
    {
        /// <summary>
        /// Из исходного файла создаёт новый(е) (не изменяя исходный) "нарезая" на указанные размеры файл(ы).
        /// </summary>
        public void SplitFile(string DestinationFolder, long size, int DimensionGroup = 1)
        {
            if (size < 1 || Length < size)
                return;

            int part_file = 0;
            long StartPosition = 0;
            long EndPosition = FileFilteredReadStream.Length;
            string tmpl_new_file_names = Path.GetFileName(FileFilteredReadStream.Name);
            while (StartPosition + size * DimensionGroup < EndPosition)
            {
                part_file++;
                CopyData(StartPosition, StartPosition + size * DimensionGroup, Path.Combine(DestinationFolder, tmpl_new_file_names + ".part_" + part_file.ToString(CultureInfo.CurrentCulture)));
                StartPosition += size * DimensionGroup;
            }

            if (StartPosition < EndPosition)
                CopyData(StartPosition, EndPosition, Path.Combine(DestinationFolder, tmpl_new_file_names + ".part_" + (part_file + 1).ToString(CultureInfo.CurrentCulture)));
        }

        public void SplitFile(string DestinationFolder, int DimensionGroup = 1)
        {
            long[] entry_points = FindDataAll(0);
            if (entry_points.Length == 0 || (entry_points.Length == 1 && entry_points[0] == 0))
                return;

            if (DimensionGroup <= 0)
                DimensionGroup = 1;

            string tmpl_new_file_names = Path.GetFileName(FileFilteredReadStream.Name) + ".split.part_";

            if (entry_points[0] > 0)
                CopyData(0, entry_points[0], tmpl_new_file_names + "0");

            int operative_dimension_group = 0;
            int part_file = 1;

            long start_position_copy_data = -1;
            foreach (long point in entry_points)
            {
                if (operative_dimension_group <= 1)
                {
                    start_position_copy_data = start_position_copy_data < 0 ? point : start_position_copy_data;
                }

                if (operative_dimension_group == DimensionGroup)
                {
                    CopyData(start_position_copy_data, point, Path.Combine(DestinationFolder, tmpl_new_file_names + part_file.ToString(CultureInfo.CurrentCulture)));
                    start_position_copy_data = point;

                    operative_dimension_group = 0;
                    part_file++;
                }

                operative_dimension_group++;
            }

            CopyData(start_position_copy_data, Length, Path.Combine(DestinationFolder, tmpl_new_file_names + part_file.ToString(CultureInfo.CurrentCulture)));
        }
    }
}
