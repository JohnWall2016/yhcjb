using System.Collections;
using YHCJB.Util;
using Newtonsoft.Json;

namespace YHCJB.HNCJB
{
    public interface IService
    {
        string Id { get; }
    }
    
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

        public Service(IService serv) :
            this(serv.Id, serv)
        {
        }

        public static implicit operator string(Service serv) => serv?.ToJson() ?? "";
    }
    
    public class CustomService : IService
    {
        public string Id { get; }
        
        public CustomService(string serviceid)
        {
            Id = serviceid;
        }
    }

    public class PageService : CustomService
    {
        public int page, pagesize;
        public ArrayList filtering = new ArrayList();
        public ArrayList sorting = new ArrayList();
        public ArrayList totals = new ArrayList();

        public PageService(string serviceid, int page = 1, int size = 15)
            : base(serviceid)
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

    public class Result<TData> : Result where TData : class
    {
        public TData[] datas;
    }

    public class SysLogin : CustomService
    {
        public string username, passwd;

        public SysLogin() : base("syslogin") {}
    }

    public class DfrymdQuery : PageService
    {
        public string aaf013 = "", aaf030 = "", aac082 = "";

        // 参保状态: "1":有效, "2":无效
        [JsonProperty("aae100")]
        public string cbzt = ""; // 参保状态

        [JsonProperty("aac002")]
        public string pid = ""; // 身份证号码

        [JsonProperty("aac003")]
        public string name = "";

        // 代发状态: "1":正常发放, "2":暂停发放, "3":终止发放
        [JsonProperty("aae116")]
        public string dfzt = "";

        // 代发类型: "801":"独生子女", "802":"乡村教师", "803":"乡村医生", "807":"电影放映员"
        [JsonProperty("aac066")]
        public string dflx = "";

        public static string GetDflxCN(string dflx)
        {
            switch (dflx)
            {
                case "801":
                    return "独生子女";
                case "802":
                    return "乡村教师";
                case "803":
                    return "乡村医生";
                case "807":
                    return "电影放映员";
            }
            return "";
        }

        public DfrymdQuery(string dflx, string cbzt = "1", string dfzt = "1")
            : base("executeDfrymdQuery", 1, 500)
        {
            this.dflx = dflx;
            this.cbzt = cbzt;
            this.dfzt = dfzt;

            AddSorting(new Hashtable()
            {
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
        public int? dfbz;

        // 代发类型
        [JsonProperty("aac066s")]
        public string dflx;

        // 代发状态
        [JsonProperty("aae116")]
        public string dfzt;

        // 居保状态
        [JsonProperty("aac008s")]
        public string jbzt;

        public string JbztCN
        {
            get
            {
                switch (jbzt)
                {
                    case "1":
                        return "正常参保";
                    case "2":
                        return "暂停参保";
                    case "3":
                        return "未参保";
                    case "4":
                        return "终止参保";
                }
                return "";
            }
        }

        // 代发截至成功发放年月
        [JsonProperty("aae002jz")]
        public int? jzny;

        // 代发截至成功发放金额
        [JsonProperty("aae019jz")]
        public int? jzje;
    }

    /*public class DfrymdResult : Result
    {
        public Dfrymd[] datas;
    }*/

    public class CbshQuery : PageService
    {
        public string aaf013 = "", aaf030 = "", aae011 = "";
        public string aae036 = "", aae036s = "", aae014 = "";
        public string aae015 = "", aae015s = "", aac009 = "";
        public string aac002 = "", aac003 = "", sfccb = "";

        // 审核状态: "0":未审核, "1":审核通过, "2":审核不通过
        [JsonProperty("aae016")]
        public string shzt = "0";

        public CbshQuery(string shzt = "0")
            : base("cbshQuery", 1, 500)
        {
            this.shzt = shzt;
        }
    }

    public class Cbsh
    {
        [JsonProperty("aac004")]
        public string sex = ""; // "1":男, "2":女

        [JsonProperty("aac003")]
        public string name = "";

        [JsonProperty("aac006")]
        public int birthday;

        [JsonProperty("aac005")]
        public string nation = ""; // 民族

        [JsonProperty("aac009")]
        public string household = ""; // 户籍

        public int aac049; // 参保时间?

        [JsonProperty("aae005", NullValueHandling=NullValueHandling.Ignore)]
        public string phone = null; // 可选?

        [JsonProperty("aac066")]
        public string type = ""; // 参保身份

        public string aae476 = "";

        public int aaz165;
        
        public string aaf103 = ""; // 村社区

        [JsonProperty("aac001")]
        public int id;
        
        public string aae140 = ""; // 社保类型: "170":居保
        
        public string aaf102 = ""; // 组

        [JsonProperty("aac002")]
        public string pid = ""; // 身份证号码

        public string aaf101 = ""; // 区划编码
        
        public string aaa129 = ""; // 县市区

        public string aae016 = "";

        public int aaz158;
        
        public string aae036 = ""; // 录入时间

        public string aae011 = ""; // 录入人

        public int aaz177;

        public CbshSave ToCbshSave()
        {
            return new CbshSave
            {
                sex = sex,
                name = name,
                birthday = $"{birthday}",
                nation = nation,
                household = household,
                aac049 = $"{aac049}",
                phone = phone,
                type = type,
                aae476 = aae476,
                aaz165 = $"{aaz165}",
                aaf103 = aaf103,
                id = $"{id}",
                aae140 = aae140,
                aaf102 = aaf102,
                pid = pid,
                aaf101 = aaf101,
                aaa129 = aaa129,
                aae016 = aae016,
                aaz158 = aaz158, // int
                aae036 = aae036,
                aae011 = aae011,
                aaz177 = $"{aaz177}",
            };
        }
    }

    public class CbshSave : Cbsh
    {
        [JsonProperty("aac006")]
        public new string birthday;

        public new string aac049;

        public new string aaz165;

        [JsonProperty("aac001")]
        public new string id;

        public new string aaz177;
        
        public string aaa129cj = "";
    }

    /*public class CbshResult : Result
    {
        public Cbsh[] datas;
    }*/

    public class CbshGotoSave : CustomService
    {
        public ArrayList rows = new ArrayList();
        public string bz = "";

        public void AddRow(CbshSave save)
        {
            rows.Add(save);
        }

        public CbshGotoSave() : base("cbshGotoSave") {}
    }

    public class CbzzfhQuery : PageService
    {
        public string aaf013 = "", aaf030 = "", aae016 = "";
        public string aae011 = "", aae036 = "", aae036s = "";
        public string aae014 = "", aae015 = "", aae015s = "";

        [JsonProperty("aac002")]
        public string pid = ""; // 身份证号码

        [JsonProperty("aac003")]
        public string name = "";

        public CbzzfhQuery(string pid, string name = "")
            : this("cbzzfhPerInfoList")
        {
            this.pid = pid;
            this.name = name;
        }

        protected CbzzfhQuery(string serviceid) : base(serviceid)
        {
        }
    }

    public class Cbzzfh
    {
        [JsonProperty("aac002")]
        public string pid = ""; // 身份证号码

        [JsonProperty("aac003")]
        public string name = "";

        public string aaz002 = "", aac001 = "", aaa027 = "";
        public string aaz038 = "", aae160 = "";

        [JsonProperty("aae031")]
        public int zzny; // 终止年月

        [JsonProperty("aae015")]
        public string shsj = ""; // 审核时间
    }

    public class DyzzfhQuery : CbzzfhQuery
    {
        public string aic301;

        public DyzzfhQuery(string pid, string name = "")
            : base("dyzzfhPerInfoList")
        {
            this.pid = pid;
            this.name = name;
        }
    }

    public class Dyzzfh
    {
        [JsonProperty("aac002")]
        public string pid = ""; // 身份证号码

        [JsonProperty("aac003")]
        public string name = "";

        public string aac001 = "", aaa027 = "", aaz176 = "";

        [JsonProperty("aae031")]
        public int zzny; // 终止年月

        [JsonProperty("aae015")]
        public string shsj = ""; // 审核时间
    }

    public class GrinfoQuery : PageService
    {
        [JsonProperty("aaf013")]
        public string xzqh = ""; // 行政区划编码

        [JsonProperty("aaz070")]
        public string cjbm = ""; // 村级编码

        public string aaf101 = "", aac009 = "";

        [JsonProperty("aac008")]
        public string cbzt = ""; // 参保状态: "1"-正常参保 "2"-暂停参保 "4"-终止参保 "0"-未参保

        [JsonProperty("aac031")]
        public string jfzt = ""; //缴费状态: "1"-参保缴费 "2"-暂停缴费 "3"-终止缴费

        public string aac006str = "", aac006end = "";
        public string aac066 = "", aae030str = "";
        public string aae030end = "", aae476 = "";

        [JsonProperty("aac003")]
        public string name = "";

        [JsonProperty("aac002")]
        public string pid = ""; // 身份证号码
        
        public GrinfoQuery(string pid) : base("zhcxgrinfoQuery")
        {
            this.pid = pid;
        }
    }

    public class Grinfo
    {
        [JsonProperty("aac001")]
        public int grbh; // 个人编号

        [JsonProperty("aac002")]
        public string pid = ""; // 身份证号码

        [JsonProperty("aac003")]
        public string name = "";

        [JsonProperty("aac006")]
        public int birthday;

        [JsonProperty("aac008")]
        public string cbzt = ""; // 参保状态: "1"-正常参保 "2"-暂停参保 "4"-终止参保 "0"-未参保

        [JsonProperty("aac010")]
        public string hkszd = ""; // 户口所在地

        [JsonProperty("aac031")]
        public string jfzt = ""; // 缴费状态: "1"-参保缴费 "2"-暂停缴费 "3"-终止缴费

        [JsonProperty("aae005")]
        public string phone = "";

        [JsonProperty("aae006")]
        public string address = "";

        [JsonProperty("aae010")]
        public string bankcard = "";

        [JsonProperty("aaf101")]
        public string xzqh = ""; // 行政区划编码

        [JsonProperty("aaf102")]
        public string czmc = ""; // 村组名称

        [JsonProperty("aaf103")]
        public string csmc = ""; // 村社区名称
    }
}
