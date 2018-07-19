#! "netcoreapp2.1"
#r "nuget: Microsoft.EntityFrameworkCore, 2.1.1"
#r "nuget: Microsoft.EntityFrameworkCore.Relational, 2.1.1"
#r "nuget: Pomelo.EntityFrameworkCore.MySql, 2.1.1"
#r "../src/YHCJB.Util/bin/Debug/netstandard2.0/YHCJB.Util.dll"

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using YHCJB.Util;

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

using (var context = new JZFPContext())
{
    context.Database.EnsureDeleted();
    context.Database.EnsureCreated();
    context.Database.LoadExcel<TSCBRY>(
        @"D:\残疾特困\201806清理数据\特殊参保人员分类明细含居保参保情况20180716.xls",
        beginRow: 2, endRow: 28636);
}

