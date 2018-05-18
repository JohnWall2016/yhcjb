using System;
using NPOI.HSSF.UserModel;

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
    }
}

