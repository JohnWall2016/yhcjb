using System;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;

namespace YHCJB.Util
{
    public static class ExcelExtension
    {
        public static HSSFRow GetOrCopyRowFrom(this HSSFSheet sheet, int dstRowIdx, int srcRowIdx)
        {
            if (dstRowIdx <= srcRowIdx)
                return (HSSFRow)sheet.GetRow(srcRowIdx);
            else
            {
                if (sheet.LastRowNum >= dstRowIdx)
                    sheet.ShiftRows(dstRowIdx, sheet.LastRowNum, 1, true, false);
                var dstRow = (HSSFRow)sheet.CreateRow(dstRowIdx);
                var srcRow = (HSSFRow)sheet.GetRow(srcRowIdx);
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

        public static void CopyRowsFrom(this HSSFSheet sheet, int start, int count, int srcRowIdx)
        {
            sheet.ShiftRows(start, sheet.LastRowNum, count, true, false);
            var srcRow = (HSSFRow)sheet.GetRow(srcRowIdx);
            for (var i = 0; i < count; i++)
            {
                var dstRow = (HSSFRow)sheet.CreateRow(start + i);
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

        public static void DuplicateRows(this HSSFSheet sheet, int rowIdx, int count)
        {
            sheet.ShiftRows(rowIdx + 1, sheet.LastRowNum, count - 1, true, false);
            var srcRow = (HSSFRow)sheet.GetRow(rowIdx);
            for (var i = 1; i < count; i++)
            {
                var dstRow = (HSSFRow)sheet.CreateRow(rowIdx + i);
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
    }
}

