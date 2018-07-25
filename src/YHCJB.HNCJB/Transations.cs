using Newtonsoft.Json;
using System.Collections;

namespace YHCJB.HNCJB
{
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
        
        public string aaf103 = ""; // 村社区名称

        [JsonProperty("aac001")]
        public int id;
        
        public string aae140 = ""; // 社保类型: "170":居保
        
        public string aaf102 = ""; // 到组名称

        [JsonProperty("aac002")]
        public string pid = ""; // 身份证号码

        public string aaf101 = ""; // 到组编码
        
        public string aaa129 = ""; // 县市区名称

        public string aae016 = "";

        public int aaz158;

        //"aae013":null,
        //"aae014":null, 审核人审核前为空
        
        public string aae036 = ""; // 录入时间

        //"aae015":null, 审核时间审核前为空

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
        public string pid; // 身份证号码

        [JsonProperty("aac003")]
        public string name;

        public string aaz002, aac001, aaa027;
        public string aaz038, aae160;

        [JsonProperty("aae031")]
        public int zzny; // 终止年月

        [JsonProperty("aae015")]
        public string shsj; // 审核时间
    }

    public class DyzzfhQuery : CbzzfhQuery
    {
        public string aic301 = "";

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
        public string pid; // 身份证号码

        [JsonProperty("aac003")]
        public string name;

        public string aac001, aaa027, aaz176;

        [JsonProperty("aae031")]
        public int zzny; // 终止年月

        [JsonProperty("aae015")]
        public string shsj; // 审核时间
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
        public string pid; // 身份证号码

        [JsonProperty("aac003")]
        public string name;

        [JsonProperty("aac006")]
        public int birthday;

        [JsonProperty("aac008")]
        public string cbzt; // 参保状态: "1"-正常参保 "2"-暂停参保 "4"-终止参保 "0"-未参保

        [JsonProperty("aac010")]
        public string hkszd; // 户口所在地

        [JsonProperty("aac031")]
        public string jfzt; // 缴费状态: "1"-参保缴费 "2"-暂停缴费 "3"-终止缴费

        [JsonProperty("aae005")]
        public string phone;

        [JsonProperty("aae006")]
        public string address;

        [JsonProperty("aae010")]
        public string bankcard;

        [JsonProperty("aaf101")]
        public string xzqh; // 行政区划编码

        [JsonProperty("aaf102")]
        public string czmc; // 村组名称

        [JsonProperty("aaf103")]
        public string csmc; // 村社区名称

        public string JbztCN => GetJbztCN(cbzt, jfzt);

        public static string GetJbztCN(string cbzt, string jfzt)
        {
            switch (jfzt)
            {
                case "3": //终止缴费
                    switch (cbzt)
                    {
                        case "1":
                            return "正常待遇人员";
                        case "2":
                            return "暂停待遇人员";
                        case "4":
                            return "终止参保人员";
                        default:
                            return "其他终止缴费人员";
                    }
                case "1": //参保缴费
                    switch (cbzt)
                    {
                        case "1":
                            return "正常缴费人员";
                        default:
                            return "其他参保缴费人员";
                    }
                case "2": //暂停缴费
                    switch (cbzt)
                    {
                        case "2":
                            return "暂停缴费人员";
                        default:
                            return "其他暂停缴费人员";
                    }
                default:
                    return "其他未知类型人员";
            }
        }
    }

    // 省内参保信息查询
    public class SncbxxQuery : CustomService
    {
        [JsonProperty("aac002")]
        public string pid = ""; // 身份证号码

        public SncbxxQuery(string pid) : base("executeSncbxxConQ")
        {
            this.pid = pid;
        }
    }

    public class Sncbxx
    {
        [JsonProperty("aac004")]
        public string sex; // "1":男, "2":女

        [JsonProperty("aac003")]
        public string name;

        [JsonProperty("aac006")]
        public int birthday;

        [JsonProperty("aac005")]
        public string nation; // 民族

        public string aae410, aac008;

        [JsonProperty("aac009")]
        public string household; // 户籍 "10":城市, "20":农村

        [JsonProperty("aae006")]
        public string address;

        [JsonProperty("aae005")]
        public string phone;

        [JsonProperty("aac066")]
        public string type; // 参保身份 "011":正常参保

        public string rylx; // 人员类型

        public string aae140; // 社保类型: "170":居保

        [JsonProperty("aaf102")]
        public string czmc; // 村组名称

        [JsonProperty("aac001")]
        public int id;

        [JsonProperty("aac002")]
        public string pid; // 身份证号码

        public string aaa129; // 社保经办机构

        [JsonProperty("aae016")]
        public string shzt; // 审核状态: "0":未审核, "1":审核通过, "2":审核不通过

        public string aae014, aae011; // 经办人或最后经办人

        public string aae036; // 经办时间

        public string aae171;

        [JsonProperty("aac031")]
        public string jfzt; // 缴费状态: "1"-参保缴费 "2"-暂停缴费 "3"-终止缴费

        [JsonProperty("aac010")]
        public string hkszd; // 户口所在地
    }

    /************ 缴费参数设置 **********/

    /// <summary>
    ///   征缴规则
    /// </summary>
    public class Zjgz
    {
        [JsonProperty("aaa044")]
        public string name;

        [JsonProperty("aac066")]
        public string sflx; // 身份类型

        [JsonProperty("aac009")]
        public string hklx; // 户口类型

        [JsonProperty("aae174")]
        public string jfdc; // 缴费档次

        [JsonProperty("aae041")]
        public string ksrq; // 开始日期

        [JsonProperty("aae042")]
        public string jsrq; // 结束日期
    }

    /// <summary>
    ///   保存征缴规则
    /// </summary>
    public class SaveZjgz : CustomService
    {
        public Zjgz form_main = new Zjgz();

        public SaveZjgz(string name, string sflx,
                        string hklx, string jfdc,
                        string ksrq, string jsrq)
        : base("executeSaveZjgz")
        {
            form_main = new Zjgz
            {
                name = name,
                sflx = sflx,
                hklx = hklx,
                jfdc = jfdc,
                ksrq = ksrq,
                jsrq = jsrq
            };
        }
    }

    /// <summary>
    ///   征缴规则查询
    /// </summary>
    public class ZjgzQuery : PageService
    {
        [JsonProperty("aac009")]
        public string hklx; // 户口类型

        [JsonProperty("aac066")]
        public string sflx; // 身份类型

        [JsonProperty("aae174")]
        public string jfdc; // 缴费档次

        [JsonProperty("aae003")]
        public string jfnd = "2018"; // 缴费年度

        public ZjgzQuery() : base("executeZjgzQuery") {}
    }

    /// <summary>
    ///   征缴规则明细
    /// </summary>
    public class Zjgzmx : Zjgz
    {
        [JsonProperty("aae041")]
        public new int ksrq; // 开始日期

        [JsonProperty("aae042")]
        public new int jsrq; // 结束日期

        public int aaz289;

        public string Id => $"{aaz289:00000000}";

        static string ToYearMonth(int date)
        {
            int year = date / 10000;
            int month = (date % 10000) / 100;
            return $"{year:0000}-{month:00}";
        }

        public string Ksrq => ToYearMonth(ksrq);

        public string Jsrq => ToYearMonth(jsrq);
    }

    /// <summary>
    ///   征缴规则参数
    /// </summary>
    public class Zjgzcs
    {
        [JsonProperty("aae341")]
        public string tcxm; // 统筹项目: "1"-个缴, "3"-省补贴
                            //           "4"-市补贴, "5"-县补贴
                            //           "11"-政府代缴

        [JsonProperty("aaa041")]
        public string jfbz; // 缴费标准

        [JsonProperty("aae380")]
        public string btlx; // 补贴类型: "1"-补贴, ""-非补贴

        [JsonProperty("aae041")]
        public string ksny; // 开始年月

        [JsonProperty("aae042")]
        public string zzny; // 终止年月

        public delegate Zjgzcs CreateZjgzcs(string tcxm,
                                            string jfbz,
                                            string btlx);

        public static CreateZjgzcs Create(string ksny, string zzny)
        {
            return delegate(string tcxm,
                            string jfbz,
                            string btlx)
            {
                return new Zjgzcs
                {
                    tcxm = tcxm,
                    jfbz = jfbz,
                    btlx = btlx,
                    ksny = ksny,
                    zzny = zzny
                };
            };
        }
    }

    /// <summary>
    ///   保存征缴规则参数
    /// </summary>
    public class SaveZjgzcs : RowsService<Zjgzcs>
    {
        [JsonProperty("aaz289")]
        public string id = "";

        public SaveZjgzcs() : base("executeSaveZjgzcs") {}
    }

    /// <summary>
    ///   省内参保情况查询-个人缴费信息
    /// </summary>
    public class SncbqkcxjfxxQ : PageService
    {
        [JsonProperty("aac002")]
        public string pid;

        public SncbqkcxjfxxQ() : base("executeSncbqkcxjfxxQ") {}
    }

    /// <summary>
    ///   省内参保情况查询-个人缴费信息明细
    /// </summary>
    public class Sncbqkcxjfxxmx
    {
        [JsonProperty("aae003")]
        public int jfnd; //缴费年度

        [JsonProperty("aae013")]
        public string memo; //备注

        [JsonProperty("aae022")]
        public double jfje; //缴费金额

        [JsonProperty("aaa115")]
        public string jflx; //缴费类型 或 "合计"

        [JsonProperty("aab033")]
        public string jffs; //缴费方式

        [JsonProperty("aae341")]
        public string jfxm; //缴费项目

        [JsonProperty("aaa027")]
        public string sbjg; //社保机构

        [JsonProperty("aae006")]
        public string hbrq; //划拨日期

        [JsonProperty("aaf101")]
        public string xzqh; //行政区划代码
    }

    public class DyzzPerInfo
    {
        [JsonProperty("aac001")]
        public string id;

        [JsonProperty("aac002")]
        public string pid = ""; // 身份证号码

        [JsonProperty("aac003")]
        public string name;

        [JsonProperty("aac004")]
        public string sex; // "1":男, "2":女

        [JsonProperty("aac006")]
        public string birthday;

        [JsonProperty("aac008")]
        public string cbzt = ""; // 参保状态: "1"-正常参保 "2"-暂停参保 "4"-终止参保 "0"-未参保

        [JsonProperty("aac009")]
        public string household = ""; // 户籍

        [JsonProperty("aac010")]
        public string hkszd; // 户口所在地

        [JsonProperty("aae006")]
        public string address;

        [JsonProperty("aae141")]
        public string ztny; // 暂停年月

        public string aaz159;

        public string zztime;
    }

    public class DyzzPerInfoQuery : CustomService
    {
        [JsonProperty("aac002")]
        public string pid = ""; // 身份证号码

        public DyzzPerInfoQuery(string pid) : base("dyzzPerInfo")
        {
            this.pid = pid;
        }
    }

    public class DyzzBankInfo
    {
        public int aaz003;

        [JsonProperty("aae133")]
        public string lqrxm; // 领取人姓名

        [JsonProperty("aae136")]
        public string lqrsfzh; // 领取人身份证号

        [JsonProperty("aae010")]
        public string yhzh; // 银行帐号

        [JsonProperty("aae009")]
        public string yhhm; // 银行户名

        [JsonProperty("aaz065")]
        public string khh; // 开户行

        public string bie020, bie021, bie022, bie023;
    }

    public class DyzzBankInfoQuery : CustomService
    {
        [JsonProperty("aac001")]
        public string id; // 个人系统ID

        public DyzzBankInfoQuery(string id) : base("dyzzBankInfo")
        {
            this.id = id;
        }
    }

    public class DyzzAccrualProMx
    {
        [JsonProperty("aae417")]
        public string zzqylj; // 终止前养老金

        [JsonProperty("aae418")]
        public string ktzhje; // 可退账户金额

        [JsonProperty("aae419")]
        public string grjfbf; // 个人缴费部分

        [JsonProperty("aae420")]
        public string ylqys; // 已领取月数

        [JsonProperty("aae421")]
        public string bfzje; // 补发总金额

        [JsonProperty("aae422")]
        public string dbfkje; // 多拨付款总额

        [JsonProperty("aae423")]
        public string szj; // 丧葬金

        [JsonProperty("aae424")]
        public string dkje; // 抵扣金额

        [JsonProperty("aae425")]
        public string tkzje; // 退款总金额

        [JsonProperty("aae426")]
        public string qfzje; // 欠费总金额 总是为0

        [JsonProperty("aae427")]
        public string yjhje; // 应稽核金额
    }

    public class DyzzAccrualPro : CustomService
    {
        [JsonProperty("aae160")]
        public string zzyy; // 终止原因: 1407

        [JsonProperty("aac001")]
        public string id;

        [JsonProperty("aic301")]
        public string zzrq; // 终止日期: 2018-07

        [JsonProperty("sf")]
        public string sfyszf; // 是否有丧葬费: 1-是 2-否

        public DyzzAccrualPro() : base("dyzzAccrualPro")
        {
        }
    }

    public class DyzzPerSave : CustomService
    {
        [JsonProperty("aae465")]
        public string sfbrlq = "1"; // ? 1-本人领取

        [JsonProperty("aac001")]
        public string id;

        [JsonProperty("aic301")]
        public string zzrq; // 终止日期: 2018-07

        [JsonProperty("sf")]
        public string sfyszf; // 是否有丧葬费: 1-是 2-否

        [JsonProperty("aae160")]
        public string zzyy; // 终止原因: 1407

        [JsonProperty("aae013")]
        public string bz; // 备注

        public class DyzzData
        {
            [JsonProperty("aae009")]
            public string yhhm; // 银行户名

            [JsonProperty("aae133")]
            public string lqrxm; // 领取人姓名

            [JsonProperty("aae136")]
            public string lqrsfzh; // 领取人身份证号

            [JsonProperty("aae010")]
            public string yhzh; // 银行帐号

            [JsonProperty("aaz065_t")]
            public string khh; // 开户行 "ZG"..

            [JsonProperty("aae019")]
            public string dyje; // 待遇金额 "0"

            public string aaz003;
        }

        public DyzzData dyzz_data;

        public class DyzzTre
        {
            [JsonProperty("aae417")]
            public string zzqylj; // 终止前养老金

            [JsonProperty("aae418")]
            public string ktzhje; // 可退账户金额

            [JsonProperty("aae419")]
            public string grjfbf; // 个人缴费部分

            [JsonProperty("aae420")]
            public string ylqys; // 已领取月数

            [JsonProperty("aae421")]
            public string bfzje; // 补发总金额

            [JsonProperty("aae422")]
            public string dbfkje; // 多拨付款总额

            [JsonProperty("aae426")]
            public string qfzje; // 欠费总金额 总是为0
        }

        public DyzzTre dyzz_tre;

        public class DyzzPay
        {
            [JsonProperty("aae423")]
            public string szj; // 丧葬金

            [JsonProperty("aae424")]
            public string dkje; // 抵扣金额

            [JsonProperty("aae425")]
            public string tkzje; // 退款总金额

            [JsonProperty("aae427")]
            public string yjhje; // 应稽核金额
        }

        public DyzzPay dyzz_pay;

        public DyzzPerSave(DyzzBankInfo bi, DyzzAccrualPro ap,
                           DyzzAccrualProMx apmx, string bz = "") : base("dyzzPerSave")
        {
            this.id = ap.id ?? "";
            this.zzrq = ap.zzrq ?? "";
            this.sfyszf = ap.sfyszf ?? "";
            this.zzyy = ap.zzyy ?? "";
            this.bz = bz ?? "";

            this.dyzz_data = new DyzzData
            {
                yhhm = bi.yhhm ?? "",
                lqrxm = bi.lqrxm ?? "",
                lqrsfzh = bi.lqrsfzh ?? "",
                yhzh = bi.yhzh ?? "",
                khh = bi.khh ?? "",
                dyje = apmx.tkzje ?? "",
                aaz003 = $"{bi.aaz003}",
            };

            var ylj = apmx.zzqylj ?? "";
            if (ylj.EndsWith(".0"))
                ylj = ylj.Substring(0, ylj.Length - 2);
            
            this.dyzz_tre = new DyzzTre
            {
                zzqylj = ylj,
                ktzhje = apmx.ktzhje ?? "",
                grjfbf = apmx.grjfbf ?? "",
                ylqys = apmx.ylqys ?? "",
                bfzje = apmx.bfzje ?? "",
                dbfkje = apmx.dbfkje ?? "",
                qfzje = apmx.qfzje ?? "",
            };

            this.dyzz_pay = new DyzzPay
            {
                szj = apmx.szj ?? "",
                dkje = apmx.dkje ?? "",
                tkzje = apmx.tkzje ?? "",
                yjhje = apmx.yjhje ?? "",
            };
        }
    }

    public class InfoByIdcard
    {
        [JsonProperty("aac001")]
        public int grbh; // 个人编号

        [JsonProperty("aac002")]
        public string pid; // 身份证号码

        [JsonProperty("aac003")]
        public string name;

        [JsonProperty("aac004")]
        public string sex = ""; // "1":男, "2":女

        [JsonProperty("aac005")]
        public string nation = ""; // 民族

        [JsonProperty("aac006")]
        public int birthday;

        [JsonProperty("aac009")]
        public string household = ""; // 户籍

        [JsonProperty("aac010")]
        public string hkszd; // 户口所在地

        [JsonProperty("aae005")]
        public string phone;

        [JsonProperty("aac066")]
        public string cbsf = ""; // 参保身份

        public string aae476 = "";

        [JsonProperty("aae010")]
        public string bankcard;

        public string aee011;

        [JsonProperty("aac031")]
        public string jfzt; // 缴费状态: "1"-参保缴费 "2"-暂停缴费 "3"-终止缴费

        public int aaz159;

        [JsonProperty("aac008")]
        public string cbzt = ""; // 参保状态: "1"-正常参保 "2"-暂停参保 "4"-终止参保 "0"-未参保

        public int aac049; // 参保年月

        [JsonProperty("aaf030")]
        public string csbm; // 村社区编码
        
        [JsonProperty("aaf103")]
        public string csmc; // 村社区名称

        [JsonProperty("aaz070")]
        public string fzbm = ""; // 村社区分组编码

        [JsonProperty("aaf101")]
        public string dzbm; // 到组编码

        [JsonProperty("aaf102")]
        public string dzmc; // 到组名称

        [JsonProperty("aaf031")]
        public string xzjmc; // 乡镇街名称

        [JsonProperty("aaf013")]
        public string xzjbm = ""; // 乡镇街编码
    }

    public class InfoByIdcardQuery : CustomService
    {
        [JsonProperty("aac002")]
        public string pid = ""; // 身份证号码

        public InfoByIdcardQuery(string pid) : base("queryInfoByIdcardService")
        {
            this.pid = pid;
        }
    }

    /// <summary>
    ///   查询（参保）未审核信息.
    /// </summary>
    /// <returns>
    ///   返回 Result 类型, type:"data", vcode: "".
    /// </returns>
    public class NotAuditInfoQuery : CustomService
    {
        [JsonProperty("aac002")]
        public string pid = ""; // 身份证号码

        public NotAuditInfoQuery(string pid) : base("queryNotAuditInfoService")
        {
            this.pid = pid;
        }
    }

    public class InfoChangeItem
    {
        [JsonProperty("aae122")]
        public string xgzd = ""; //修改字段

        [JsonProperty("aae123")]
        public string zdold = ""; //字段原值

        [JsonProperty("aae124")]
        public string zdnow = ""; //字段新值

        [JsonProperty("aae157")]
        public string zdbm = ""; //字段编码

        [JsonProperty("aae155")]
        public string zdmc = ""; //字段名称

        public static InfoChangeItem ChangeCbsf(string zdold, string zdnow)
        {
            return new InfoChangeItem
            {
                xgzd = "AAC066",
                zdold = zdold,
                zdnow = zdnow,
                zdbm = "AC20",
                zdmc = "参保身份",
            };
        }
    }

    public class AddInfoChange : CustomService
    {
        public AddInfoChange(int grbh, string pid, string name, int aaz159, string bz = "") 
            : base("addInformationChangeService")
        {
            this.grbh = grbh;
            this.pid = pid;
            this.bz = bz;
            this.pidnow = pid;
            this.pidold = pid;
            this.aaz159 = aaz159;
            this.namenow = name;
            this.nameold = name;
        }

        public ArrayList array = new ArrayList();

        public void AddArray(InfoChangeItem item)
        {
            array.Add(item);
        }

        [JsonProperty("aac001")]
        public int grbh; // 个人编号

        [JsonProperty("aac002")]
        public string pid; // 身份证号码

        [JsonProperty("aae013")]
        public string bz; // 备注
        
        [JsonProperty("aac002Now")]
        public string pidnow; // 身份证号码

        [JsonProperty("aac002Old")]
        public string pidold; // 身份证号码

        public int aaz159;

        [JsonProperty("aac003Now")]
        public string namenow;

        [JsonProperty("aac003Old")]
        public string nameold;
    }

    public class InfoChangeForAudit
    {
        [JsonProperty("aac004")]
        public string sex; // "1":男, "2":女

        [JsonProperty("aac003")]
        public string name;

        [JsonProperty("aaf013")]
        public string qxbm; // 区县编码

        [JsonProperty("aac006")]
        public int birthday;

        [JsonProperty("aac005")]
        public string nation; // 民族

        public string aac008;

        [JsonProperty("aac009")]
        public string household; // 户籍 "10":城市, "20":农村

        public int aac049; // 参保年月

        [JsonProperty("aaf031")]
        public string xzjmc; // 乡镇街名称

        [JsonProperty("aae016")]
        public string shzt; // 审核状态: "0":未审核, "1":审核通过, "2":审核不通过

        [JsonProperty("aac066")]
        public string cbsf; // 参保身份 "011":正常参保

        //"aae013":null,

        public string aae036; // 经办时间

        //"aae015":null, 审核时间审核前为空

        public string aae011 = ""; // 录入人

        [JsonProperty("aaz070")]
        public int fzbm; // 村社区分组编码

        [JsonProperty("aaf103")]
        public string csmc; // 村社区名称

        [JsonProperty("aaf102")]
        public string dzmc; // 到组名称

        public int aaz163;

        [JsonProperty("aac001")]
        public int grbh;

        [JsonProperty("aac002")]
        public string pid; // 身份证号码

        public AuditInfoChange ToAuditInfoChange()
        {
            return new AuditInfoChange
            {
                birthday = $"{birthday}",
                fzbm = $"{fzbm}",
                aaz163 = $"{aaz163}",
                grbh = $"{grbh}",
            };
        }
    }

    public class InfoChangeForAuditQuery : PageService
    {
        public string aaf013 = "", aaf030 = "", aae016 = "0"/*未审核*/;
        
        [JsonProperty("aac002")]
        public string pid = ""; // 身份证号码

        [JsonProperty("aac003")]
        public string name = "";

        public string aae011 = "", aae036 = "", aae036s = "";
        public string aae014 = "", aae015 = "", aae015s = "";
        public string aac009 = "";
        
        public InfoChangeForAuditQuery(string pid, string name = "")
            : this("queryInfoChangeForAuditService")
        {
            this.pid = pid;
            this.name = name;
        }

        protected InfoChangeForAuditQuery(string serviceid) : base(serviceid)
        {
        }
    }

    public class AuditInfoChange : InfoChangeForAudit
    {
        [JsonProperty("aac006")]
        public new string birthday;
        
        [JsonProperty("aaz070")]
        public new string fzbm; // 村社区分组编码

        public new string aaz163;

        [JsonProperty("aac001")]
        public new string grbh;

        public string aaz206 = "";
    }

    public class AuditInfoChangePass : RowsService<AuditInfoChange>
    {
        public AuditInfoChangePass() : base("auditInformationChangePassService") {}
    }
}
