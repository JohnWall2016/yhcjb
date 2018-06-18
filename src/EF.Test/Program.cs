using System;
using System.Linq;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using MySql.Data.EntityFrameworkCore.Extensions;

namespace EF.Test
{
    [Table("2018年低保人员名单")]
    public class DBRY
    {
        [Key, Column("序号"), Required]
        public int NO { get; set; }

        [Column("姓名")]
        public string Name { get; set; }

        [Column("身份证")]
        public string ID { get; set; }

        [Column("低保类型")]
        public string Type { get; set; }

        [Column("低保类别")]
        public string Level { get; set; }

        [Column("备注")]
        public string Memo { get; set; }
    }

    public class TestContext : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseMySQL("server=127.0.0.1;port=3306;database=TEST;uid=root;pwd=root;charset=utf8;sslmode=None;");
        }

        public DbSet<DBRY> DBRY { get; set; }
    }

    class Program
    {
        static void Main(string[] args)
        {
            using (var db = new TestContext())
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
        }
    }
}
