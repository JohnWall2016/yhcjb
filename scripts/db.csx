#! "netcoreapp2.0"
#r "nuget: linq2db, 2.0.0"
#r "nuget: linq2db.MySql, 2.0.0"
#r "nuget: System.Security.Permissions, 4.5.0"

using System;
using System.Collections.Generic;
using System.Linq;
using LinqToDB;
using LinqToDB.Common;
using LinqToDB.Mapping;
using LinqToDB.Data;
using LinqToDB.Configuration;

[Table(Name = "2018年低保人员名单")]
public class DBRY
{
    [PrimaryKey, Column(Name = "序号"), NotNull]
    public int NO { get; set; }

    [Column(Name = "姓名")]
    public string Name { get; set; }

    [Column(Name = "身份证")]
    public string ID { get; set; }

    [Column(Name = "低保类型")]
    public string Type { get; set; }

    [Column(Name = "低保类别")]
    public string Level { get; set; }

    [Column(Name = "备注")]
    public string Memo { get; set; }
}

public class DbTest : DataConnection
{
    public DbTest() : base("TEST") { }

    public ITable<DBRY> DBRY => GetTable<DBRY>();
}

public class ConnectionStringSettings : IConnectionStringSettings
{
    public string ConnectionString { get; set; }
    public string Name { get; set; }
    public string ProviderName { get; set; }
    public bool IsGlobal => false;
}

IEnumerable<IConnectionStringSettings> ConnectionStrings()
{
    yield return
        new ConnectionStringSettings
    {
        Name = "TEST",
        ProviderName = "MySql.Data.MySqlClient",
        ConnectionString = "server=127.0.0.1;port=3306;database=TEST;uid=root;pwd=root;charset=utf8;"
    };
}
            
DataConnection.SetConnectionStrings(ConnectionStrings());

using (var db = new DbTest())
{
    var query = from p in db.DBRY
        where p.NO < 25
        && p.ID.Substring(6, 8).CompareTo("19550101") <= 0
        orderby p.Name descending
        select p;
    foreach (var p in query)
    {
        Console.WriteLine($"{p.Name} {p.ID}");
    }
}


