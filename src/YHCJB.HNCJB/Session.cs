using System;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using System.Configuration;

namespace YHCJB.HNCJB
{
    public partial class Session : IDisposable
    {
        TcpClient _client;
        NetworkStream _stream;
        string _sessionId = "";
        string _cxCookie = "";
        Encoding _enc = Encoding.UTF8;
        
        public Session(string ip, int port)
        {
            _client = new TcpClient(ip, port);
            try
            {
                _stream = _client.GetStream();
            }
            catch (Exception ex)
            {
                _client.Close();
                throw ex;
            }

            IP = ip;
            Port = port;
        }

        public string IP { get; }
        public int Port { get; }
        public string Userid { get; set; }
        public string Password { get; set; }

        public Encoding Enc => _enc;

        public string Url => $"{IP}:{Port}";

        public void Dispose()
        {
            if (_stream != null)
            {
                _stream.Close();
                _stream = null;
            }
            if (_client != null)
            {
                _client.Close();
                _client = null;
            }
        }

        void Write(string content)
        {
            if (_stream == null)
                throw new ApplicationException("service is not connected");
            var data = Enc.GetBytes(content);
            _stream.Write(data, 0, data.Length);
        }

        (byte[], int) Read(int len)
        {
            if (_stream == null)
                throw new ApplicationException("service is not connected");
            var data = new Byte[len];
            var size = _stream.Read(data, 0, len);
            return (data, size);
        }

        string ReadLine()
        {
            if (_stream == null)
                throw new ApplicationException("service is not connected");
            using (var mem = new MemoryStream(512))
            {
                int c = 0, n = 0;
                while (true)
                {
                    c = _stream.ReadByte();
                    if (c == -1)
                        return Enc.GetString(mem.GetBuffer(), 0, (int)mem.Length);
                    if (c == 0xD)
                    {
                        n = _stream.ReadByte();
                        if (n == 0xA)
                            return Enc.GetString(mem.GetBuffer(), 0, (int)mem.Length);
                        else if (n == -1)
                        {
                            mem.WriteByte((byte)c);
                            return Enc.GetString(mem.GetBuffer(), 0, (int)mem.Length);
                        }
                        else
                        {
                            mem.WriteByte((byte)c);
                            mem.WriteByte((byte)n);
                        }
                    } else
                        mem.WriteByte((byte)c);
                }
            }
        }

        string ReadHeader()
        {
            var result = new StringBuilder(512);
            while (true)
            {
                var line = ReadLine();
                if (line == null || line == "") break;
                result.Append(line + "\n");
            }
            return result.ToString();
        }

        string ReadBody(string header = "")
        {
            using(var data = new MemoryStream(512))
            {
                if (header == "")
                    header = ReadHeader();
                if (Regex.IsMatch(header, "Transfer-Encoding: chunked"))
                {
                    while (true)
                    {
                        var len = Convert.ToInt32(ReadLine(), 16);
                        if (len <= 0)
                        {
                            ReadLine();
                            break;
                        }
                        while (true)
                        {
                            (var rec, var rlen) = Read(len);
                            data.Write(rec, 0, rlen);
                            len -= rlen;
                            if (len <= 0)
                            {
                                break;
                            }
                        }
                        ReadLine();
                    }
                }
                else
                {
                    var match = Regex.Match(header, @"Content-Length: (\d+)");
                    if (match.Length > 0)
                    {
                        var len = Convert.ToInt32(match.Groups[1].Value, 10);
                        while (len > 0)
                        {
                            (var rec, var rlen) = Read(len);
                            data.Write(rec, 0, rlen);
                            len -= rlen;
                        }
                    }
                    else
                        throw new ApplicationException("Unsupported transfer mode");
                }
                return Enc.GetString(data.GetBuffer(), 0, (int)data.Length);
            }
        }

        string MakeSendContent(string content)
        {
            var result =
                "POST /hncjb/reports/crud HTTP/1.1\n" +
                $"Host: {Url}\n" +
                "Connection: keep-alive\n" +
                $"Content-Length: {content.Length}\n"+
                "Accept: application/json, text/javascript, */*; q=0.01\n"+
                $"Origin: http://{Url}\n"+
                "X-Requested-With: XMLHttpRequest\n"+
                "User-Agent: Mozilla/5.0 (Windows NT 5.1) AppleWebKit/537.36 " +
                "(KHTML, like Gecko) Chrome/39.0.2171.95 Safari/537.36\n" +
                "Content-Type: multipart/form-data;charset=UTF-8\n"+
                $"Referer: http://{Url}/hncjb/pages/html/index.html\n"+
                "Accept-Encoding: gzip, deflate\n"+
                "Accept-Language: zh-CN,zh;q=0.8\n";
            if (_sessionId != "")
                result += $"Cookie: jsessionid_ylzcbp={_sessionId}" +
                    $"; cxcookie={_cxCookie}\n";
            result += $"\n{content}";
            //Console.WriteLine($"MakeSendContent: {result}");
            return result;
        }

        void Send(string serviceContent)
        {
            Write(MakeSendContent(serviceContent));
        }

        public void Send(string serviceId, object param)
        {
            Send(new Service(serviceId, param) { loginname = Userid, password = Password });
        }

        public string Get()
        {
            return ReadBody();
        }

        public string Login()
        {
            Send(new Service("loadCurrentUser"));
            var header = ReadHeader();
            var match = Regex.Match(header, "Set-Cookie: jsessionid_ylzcbp=(.+?);");
            if (match.Length > 0)
                _sessionId = match.Groups[1].Value;
            match = Regex.Match(header, "Set-Cookie: cxcookie=(.+?);");
            if (match.Length > 0)
                _cxCookie = match.Groups[1].Value;
            ReadBody(header);

            Send("syslogin", new SysLogin { username = Userid, passwd = Password });
            return Get();
        }

        public string Logout()
        {
            Send(new Service("syslogout"));
            return Get();
        }
    }
}
