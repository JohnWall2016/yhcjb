using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Data;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;

namespace YHCJB.Util
{
    public static class EFCoreExtension
    {
        class FieldMapping
        {
            public string Name { get; set; }
            public Type Type { get; set; }
            public int? Column { get; set; }
    
            public override string ToString()
            {
                return $"Name = {Name}, Type = {Type}, Column = {Column}";
            }
        }

        static List<FieldMapping> GetFieldMappings<TFields>(string[] titleColumns)
        {
            var fieldMappings = new List<FieldMapping>();

            var type = typeof(TFields);
            var titleMatched = false;
            foreach (var pi in type.GetProperties())
            {
                var fieldMapping = new FieldMapping();

                var customAttrs = pi.GetCustomAttributes(typeof(ColumnAttribute), false);
                fieldMapping.Name = customAttrs.Length > 0 ? (customAttrs[0] as ColumnAttribute).Name : pi.Name;
                fieldMapping.Type = pi.PropertyType;

                for (var i = 0; i < titleColumns.Length; i++)
                {
                    if (titleColumns[i] == fieldMapping.Name)
                    {
                        fieldMapping.Column = i;
                        titleMatched = true;
                        break;
                    }
                }

                fieldMappings.Add(fieldMapping);
            }

            if (titleMatched)
                return fieldMappings;
            else
                return null;
        }

        public static void MySqlLoadInFile(this DatabaseFacade db, string fileName, string tableName)
        {
            fileName = new Uri(fileName).AbsolutePath;
            var loadCVS = $@"load data infile '{fileName}' into table `{tableName}` CHARACTER SET utf8 FIELDS TERMINATED BY ',' OPTIONALLY ENCLOSED BY '\'' LINES TERMINATED BY '\n';";
            //Console.WriteLine(loadCVS);

            var connection = db.GetDbConnection();
            if (connection.State == ConnectionState.Closed)
                connection.Open();
            using (var command = connection.CreateCommand())
            {
                command.CommandText = loadCVS;
                command.ExecuteNonQuery();
            }
        }

        public static void LoadExcel<T>(this DatabaseFacade db, string fileName, int sheetIndex = 0, int titleRow = 0, int beginRow = 1, int endRow = -1)
        {
            if (!db.IsMySql()) throw new NotImplementedException();

            var wb = ExcelExtension.LoadExcel(fileName);
            try
            {
                var sheet = wb.GetSheetAt(sheetIndex);
                var titles = sheet.GetRow(titleRow).Cells.Select(cell => cell.CellValue());

                var fieldMappings = GetFieldMappings<T>(titles.ToArray());
                if (fieldMappings == null) return;

                var tmpFileName = Path.GetTempFileName();
            
                if (File.Exists(tmpFileName))
                    File.Delete(tmpFileName);
            
                using (var tmpFile = new FileStream(tmpFileName, FileMode.CreateNew))
                {
                    if (endRow == -1) endRow = sheet.LastRowNum;
                    for (var idx = beginRow; idx <= endRow; idx++)
                    {
                        var line = new List<string>();
                        var row = sheet.Row(idx);
                        foreach (var mapping in fieldMappings)
                        {
                            var value = "";
                            if (mapping.Column.HasValue)
                                value = row.Cell(mapping.Column.Value).CellValue();
                            if (mapping.Type.Equals(typeof(string)))
                                value = $"'{value}'";
                            line.Add(value);
                        }
                        var cvs = Encoding.UTF8.GetBytes(string.Join(",", line) + "\n");
                        tmpFile.Write(cvs, 0, cvs.Length);
                    }
                }

                var tableType = typeof(T);
                var customAttrs = tableType.GetCustomAttributes(typeof(TableAttribute), false);
                var tableName = customAttrs.Length > 0 ? (customAttrs[0] as TableAttribute).Name : tableType.Name;
                
                db.MySqlLoadInFile(tmpFileName, tableName);
            }
            finally
            {
                wb.Close();
            }
        }
    }
}
