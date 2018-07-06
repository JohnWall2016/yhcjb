using System;
using System.IO;
using NPOI.HSSF.UserModel;
using NPOI.XSSF.UserModel;
using NPOI.SS.UserModel;

namespace YHCJB.Util
{
    public static class ExcelExtension
    {
        public enum ExcelType
        {
            XLS, XLSX, AUTO
        }

        public static IWorkbook LoadExcel(string fileName, ExcelType type = ExcelType.AUTO)
        {
            Stream stream = new MemoryStream();
            using (var file = new FileStream(fileName, FileMode.Open, FileAccess.Read))
            {
                file.CopyTo(stream);
                stream.Seek(0, SeekOrigin.Begin);
            }
            if (type == ExcelType.AUTO)
            {
                var ext = Path.GetExtension(fileName).ToLower();
                type = ext.Equals(".xls") ? ExcelType.XLS :
                    ext.Equals(".xlsx") ? ExcelType.XLSX :
                    throw new ArgumentException("Unknown excel type");
            }
            switch (type)
            {
                case ExcelType.XLS:
                    return new HSSFWorkbook(stream);
                case ExcelType.XLSX:
                    return new XSSFWorkbook(stream);
            }
            throw new ArgumentException("Unknown excel type");
        }

        public static void Save(this IWorkbook wb, string fileName)
        {
            using (var stream = new FileStream(fileName, FileMode.CreateNew))
                wb.Write(stream);
        }

        public static IRow GetOrCopyRowFrom(this ISheet sheet, int dstRowIdx, int srcRowIdx)
        {
            if (dstRowIdx <= srcRowIdx)
                return sheet.GetRow(srcRowIdx);
            else
            {
                if (sheet.LastRowNum >= dstRowIdx)
                    sheet.ShiftRows(dstRowIdx, sheet.LastRowNum, 1, true, false);
                var dstRow = sheet.CreateRow(dstRowIdx);
                var srcRow = sheet.GetRow(srcRowIdx);
                dstRow.Height = srcRow.Height;
                for (var idx = (int)srcRow.FirstCellNum; idx < srcRow.PhysicalNumberOfCells; idx++)
                {
                    var dstCell = dstRow.CreateCell(idx);
                    var srcCell = srcRow.GetCell(idx);
                    dstCell.SetCellType(srcCell.CellType);
                    dstCell.CellStyle = srcCell.CellStyle;
                    dstCell.SetValue("");
                }
                return dstRow;
            }
        }

        public static void CopyRowsFrom(this ISheet sheet, int start, int count, int srcRowIdx)
        {
            sheet.ShiftRows(start, sheet.LastRowNum, count, true, false);
            var srcRow = sheet.GetRow(srcRowIdx);
            for (var i = 0; i < count; i++)
            {
                var dstRow = sheet.CreateRow(start + i);
                dstRow.Height = srcRow.Height;
                for (var idx = (int)srcRow.FirstCellNum; idx < srcRow.PhysicalNumberOfCells; idx++)
                {
                    var dstCell = dstRow.CreateCell(idx);
                    var srcCell = srcRow.GetCell(idx);
                    dstCell.SetCellType(srcCell.CellType);
                    dstCell.CellStyle = srcCell.CellStyle;
                    dstCell.SetValue("");
                }
            }
        }

        public static void DuplicateRows(this ISheet sheet, int rowIdx, int count)
        {
            sheet.ShiftRows(rowIdx + 1, sheet.LastRowNum, count - 1, true, false);
            var srcRow = sheet.GetRow(rowIdx);
            for (var i = 1; i < count; i++)
            {
                var dstRow = sheet.CreateRow(rowIdx + i);
                dstRow.Height = srcRow.Height;
                for (var idx = (int)srcRow.FirstCellNum; idx < srcRow.PhysicalNumberOfCells; idx++)
                {
                    var dstCell = dstRow.CreateCell(idx);
                    var srcCell = srcRow.GetCell(idx);
                    dstCell.SetCellType(srcCell.CellType);
                    dstCell.CellStyle = srcCell.CellStyle;
                    dstCell.SetValue("");
                }
            }
        }

        public static IRow Row(this ISheet sheet, int row) => sheet.GetRow(row);
        
        public static ICell Cell(this ISheet sheet, int row, int col) => sheet.GetRow(row).GetCell(col);

        public static ICell Cell(this IRow row, int col) => row.GetCell(col);

        public static void SetValue(this ICell cell, string value) => cell.SetCellValue(value);

        public static void SetValue(this ICell cell, double value) => cell.SetCellValue(value);

        public static string CellValue(this ICell cell)
        {
            try
            {
                return cell.StringCellValue;
            }
            catch (InvalidOperationException)
            {
                return cell.NumericCellValue.ToString();
            }
        }
    }
}

