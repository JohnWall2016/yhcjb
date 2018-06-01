using System.Collections;
using YHCJB.Util;

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

        public Service(string serviceid)
            : this(serviceid, new Hashtable())
        {
        }

        public Service(IService serv)
            : this(serv.Id, serv)
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
}
