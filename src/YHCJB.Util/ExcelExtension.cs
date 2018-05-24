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
                }
                return dstRow;
            }
        }

        public static ICell Cell(this HSSFSheet sheet, int row, int col)
        {
            return sheet.GetRow(row).GetCell(col);
        }

        public static ICell Cell(this HSSFRow row, int col)
        {
            return row.GetCell(col);
        }

        public static void SetValue(this ICell cell, string value)
        {
            cell.SetValue(value);
        }

        public static void SetValue(this ICell cell, double value)
        {
            cell.SetValue(value);
        }
    }
}

