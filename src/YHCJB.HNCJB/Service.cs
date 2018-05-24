using System.Collections;
using YHCJB.Util;
using Newtonsoft.Json;

namespace YHCJB.HNCJB
{
    public class Service : Json<Service>
    {
        public string serviceid { get; } = "";
        public string target => "";
        public string sessionid => null;
        public string loginname { get; set; } = null;
        public string password { get; set; } = null;
        public object @params { get; }

        public ArrayList datas => new ArrayList() { @params };

        public Service(string serviceid, object obj)
        {
            this.serviceid = serviceid;
            this.@params = obj;
        }

        public Service(string serviceid) :
            this(serviceid, new Hashtable())
        {
        }

        public static implicit operator string(Service serv) => serv?.ToJson() ?? "";
    }

    public class SysLogin
    {
        public string username, passwd;
    }

    public class PageInfo
    {
        public int page, pagesize;
        public ArrayList filtering = new ArrayList();
        public ArrayList sorting = new ArrayList();
        public ArrayList totals = new ArrayList();

        public PageInfo(int page = 1, int size = 15)
        {
            this.page = page;
            this.pagesize = size;
        }

        public void AddSorting(Hashtable sorting)
        {
            this.sorting.Add(sorting);
        }

        public void AddTotals(Hashtable totals)
        {
            this.totals.Add(totals);
        }
    }

    public class Result
    {
        public int rowcount, page, pagesize;
        public string serviceid, type, vcode, message, messagedetail;
    }

    public class DfrymdQuery : PageInfo
    {
        public string aaf013 = "", aaf030 = "", aac082 = "";

        // 参保状态: "1":有效, "2":无效
        [JsonProperty("aae100")]
        public string cbzt = ""; // 参保状态

        [JsonProperty("aac002")]
        public string pid = "";

        [JsonProperty("aac003")]
        public string name = "";

        // 代发状态: "1":正常发放, "2":暂停发放, "3":终止发放
        [JsonProperty("aae116")]
        public string dfzt = "";

        // 代发类型: "801":"独生子女", "802":"乡村教师", "803":"乡村医生", "807":"电影放映员"
        [JsonProperty("aac066")]
        public string dflx = "";

        public DfrymdQuery(string dflx, string cbzt = "1", string dfzt = "1") : base(1, 500)
        {
            this.dflx = dflx;
            this.cbzt = cbzt;
            this.dfzt = dfzt;

            AddSorting(new Hashtable() {
                    ["dataKey"] = "aaf103",
                    ["sortDirection"] = "ascending"
                });
        }
    }

    public class Dfrymd
    {
        [JsonProperty("aac001")]
        public int id;
        
        [JsonProperty("aaf103")]
        public string region;
            
        [JsonProperty("aac003")]
        public string name;
        
        [JsonProperty("aac002")]
        public string pid;

        // 代发开始年月
        [JsonProperty("aic160")]
        public int ksny;

        // 代发标准
        [JsonProperty("aae019")]
        public int dfbz;

        // 代发类型
        [JsonProperty("aac066s")]
        public string dflx;

        // 代发状态
        [JsonProperty("aae116")]
        public string dfzt;

        // 居保状态
        [JsonProperty("aac008s")]
        public string jbzt;

        // 代发截至成功发放年月
        [JsonProperty("aae002jz")]
        public int jzny;

        // 代发截至成功发放金额
        [JsonProperty("aae019jz")]
        public int jzje;
    }

    public class DfrymdResult : Result
    {
        public Dfrymd[] datas;
    }
}
