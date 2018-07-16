using System;
using System.Collections.Generic;

namespace YHCJB.DB
{
    public class BaseField
    {
        public string Name { get; set; }

        public bool Nullable { get; set; }

        public bool IsPrimaryKey { get; set; }

        public virtual string ValueSQL(string value) => value;

        protected virtual string ToFieldSQL() => throw new NotImplementedException();

        public string FieldSQL()
        {
            var extSQL = new List<string>();
            if (!Nullable) extSQL.Add("not null");
            if (IsPrimaryKey) extSQL.Add("primary key");

            var baseSQL = $"`{Name}` {ToFieldSQL}";
            if (extSQL.Count > 0)
                return baseSQL + " " + string.Join(" ", extSQL);
            return baseSQL;
        }
    }

    public class BoolField : BaseField
    {
        protected override string ToFieldSQL => "bit";
    }
}
