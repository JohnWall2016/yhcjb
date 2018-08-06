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

[Table("2018年居保缴费明细数据")]
public class JBJFMX
{
    [Column("行政区划名称"), Required]
    public string Xzqh { get; set; }

    [Column("户籍")]
    public string Hj { get; set; }

    [Column("姓名"), Required]
    public string Xm { get; set; }

    [Key, Column("身份证"), Required]
    public string Sfz { get; set; }

    [Column("性别"), Required]
    public string Xb { get; set; }

    [Column("参保身份")]
    public string Cbsf { get; set; }

    [Column("缴费年度")]
    public string Jfnd { get; set; }

    [Column("个缴")]
    public decimal Gj { get; set; }

    [Column("省补")]
    public decimal Shenb { get; set; }

    [Column("市补")]
    public decimal Shib { get; set; }

    [Column("县补")]
    public decimal Xianb { get; set; }

    [Column("代缴")]
    public decimal Dj { get; set; }
}

public class JZFPContext : DbContext
{
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseMySql("server=127.0.0.1;port=3306;database=JZFP;uid=root;pwd=root;charset=utf8;sslmode=None;");
    }

    public DbSet<TSCBRY> TSCBRYs { get; set; }
    public DbSet<LSCBRY> LSCBRYs { get; set; }
    public DbSet<JBJFMX> JBJFMXs { get; set; }
}
