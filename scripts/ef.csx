#! "netcoreapp2.1"
#r "nuget: Microsoft.EntityFrameworkCore, 2.1.1"
#r "nuget: Microsoft.EntityFrameworkCore.Relational, 2.1.1"
#r "nuget: Pomelo.EntityFrameworkCore.MySql, 2.1.1"
#r "../src/YHCJB.Util/bin/Debug/netstandard2.0/YHCJB.Util.dll"
#r "../src/YHCJB.HNCJB/bin/Debug/netstandard2.0/YHCJB.HNCJB.dll"

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using YHCJB.Util;
using YHCJB.HNCJB;

[Table("2018年特殊参保人员分类明细表")]
public class TSCBRY
{
    [Key, Column("序号"), Required]
    public int Xh { get; set; }

    [Column("姓名")]
    public string Xm { get; set; }

    [Column("身份证号码")]
    public string Sfzhm { get; set; }

    [Column("出生日期")]
    public string Csrq { get; set; }

    [Column("居保状态")]
    public string Jbzt { get; set; }

    [Column("人员认定身份")]
    public string Rdsf { get; set; }

    [Column("对应系统中身份")]
    public string Xtsf { get; set; }

    [Column("对应系统中身份编码")]
    public string Sfbm { get; set; }

    [Column("人员所属单位")]
    public string Ssdw { get; set; }
}

[Table("2018年历史参保人员信息表")]
public class LSCBRY
{
    [Column("行政区划"), Required]
    public string Xzqh { get; set; }

    [Column("户籍性质")]
    public string Hjxz { get; set; }

    [Column("姓名"), Required]
    public string Xm { get; set; }

    [Key, Column("公民身份证号码"), Required]
    public string Sfzhm { get; set; }

    [Column("出生日期")]
    public string Csrq { get; set; }

    [Column("参保身份")]
    public string Cbsf { get; set; }

    [Column("参保状态")]
    public string Cbzt { get; set; }

    [Column("缴费状态")]
    public string Jfzt { get; set; }

    [Column("参保时间")]
    public string Cbsj { get; set; }
}


public class JZFPContext : DbContext
{
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseMySql("server=127.0.0.1;port=3306;database=JZFP;uid=root;pwd=root;charset=utf8;sslmode=None;");
    }

    public DbSet<TSCBRY> TSCBRYs { get; set; }
    public DbSet<LSCBRY> LSCBRYs { get; set; }
}

void initJZFPDatabase()
{
    using (var context = new JZFPContext())
    {
        Console.WriteLine("创建精准扶贫数据库");
        context.Database.EnsureDeleted();
        context.Database.EnsureCreated();
        Console.WriteLine("导入特殊参保人员数据");
        context.Database.LoadExcel<TSCBRY>(
            @"D:\残疾特困\201806清理数据\特殊参保人员分类明细含居保参保情况20180716.xls",
            beginRow: 2, endRow: 28636);
        Console.WriteLine("导入居保历史参保人员数据");
        context.Database.LoadExcel<LSCBRY>(@"D:\残疾特困\201806清理数据\居保历史参保人员名单20180720A.xls");
        context.Database.LoadExcel<LSCBRY>(@"D:\残疾特困\201806清理数据\居保历史参保人员名单20180720B.xls");
        Console.WriteLine("精准扶贫数据库创建完毕");
    }
}

void updateJbzt(string excel = @"D:\残疾特困\201806清理数据\特殊参保人员分类明细含居保参保情况20180716.xls")
{
    var wb = ExcelExtension.LoadExcel(excel);
    var sheet = wb.GetSheetAt(0);
    using (var context = new JZFPContext())
    {
        for (var i = 2; i <= sheet.LastRowNum; i++)
        {
            var sfzhm = sheet.Cell(i, 2).StringCellValue;

            var jbxx = from lsry in context.LSCBRYs
                       where lsry.Sfzhm == sfzhm
                       select new { lsry.Xm, lsry.Sfzhm, lsry.Cbsf, lsry.Cbzt, lsry.Jfzt };

            var jbzt = "";
            var jbxm = "";
            if (jbxx.Count() <= 0)
                jbzt = "未参加居保";
            else
            {
                var lsry = jbxx.First();
                jbxm = lsry.Xm;
                jbzt = Grinfo.GetJbztCN(lsry.Cbzt, lsry.Jfzt);
            }
            Console.WriteLine($"{i,5}: {sfzhm} {jbzt}");
            sheet.Cell(i, 4).SetValue(jbzt);
            sheet.Cell(i, 5).SetValue(jbxm);
        }
    }
    wb.Save(Utils.FileNameAppend(excel, ".new"));
    wb.Close();
}

//initJZFPDatabase();
//updateJbzt();
