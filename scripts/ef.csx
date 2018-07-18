#! "netcoreapp2.1"
#r "nuget: Microsoft.EntityFrameworkCore, 2.1.1"
#r "nuget: Microsoft.EntityFrameworkCore.Relational, 2.1.1"
#r "nuget: Pomelo.EntityFrameworkCore.MySql, 2.1.1"

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;

[Table("2018年特殊参保人员分类明细表")]
public class TSCBRY
{
    [Key, Column("序号"), Required]
    public int NO { get; set; }

    [Column("姓名")]
    public string Name { get; set; }

    [Column("身份证号码")]
    public string IDNumber { get; set; }

    [Column("出生日期")]
    public string BirthDay { get; set; }

    [Column("居保状态")]
    public string GBState { get; set; }

    [Column("人员认定身份")]
    public string IdentifiedType { get; set; }

    [Column("对应系统中身份")]
    public string GBType  { get; set; }

    [Column("对应系统中身份编码")]
    public string GBTypeCode { get; set; }

    [Column("人员所属单位")]
    public string Region { get; set; }
}

public class JZFPContext : DbContext
{
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseMySql("server=127.0.0.1;port=3306;database=JZFP;uid=root;pwd=root;charset=utf8;sslmode=None;");
    }

    public DbSet<TSCBRY> TSCBRYs { get; set; }
}

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

void LoadExcel<T>(DatabaseFacade db, string fileName, int titleRow = 0, int beginRow = 1, int endRow = -1)
{
    var titles = new string[] { "姓名", "身份证号码", "对应系统中身份" };

    List<FieldMapping> GetFieldMappings<TFields>(string[] titleColumns)
    {
        var fieldMappings = new List<FieldMapping>();
    
        var type = typeof(TFields);
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
                    break;
                }
            }

            fieldMappings.Add(fieldMapping);
        }

        return fieldMappings;
    }

    foreach (var m in GetFieldMappings<T>(titles))
    {
        Console.WriteLine(m);
    }
    
}

using (var context = new JZFPContext())
{
    //context.Database.EnsureCreated();
    
    LoadExcel<TSCBRY>(context.Database, "");
}


