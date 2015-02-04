namespace ConsoleApplication1
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Net;
    using System.IO;
    using Mono.Web;
    using System.IO.Compression;

    public class BingWebRequest
    {
        public CookieContainer cc = null;
        public string _auth = null;
        private List<String> searchTerms = null;

        public BingWebRequest(string userName, string password)
        {
            // Words from http://wordlist.sourceforge.net/
            searchTerms = new List<string>(File.ReadAllLines("english-words.70"));

            InitiateSession(userName, password);
        }

        public void SendBingRequest()
        {
            var url = "http://www.bing.com/search?q={term}&form=MOZSBR&pc=MOZI";
            var rand = new Random();

            var term = searchTerms[rand.Next(searchTerms.Count)];
            var tempUrl = url.Replace("{term}", term);


            HttpWebRequest req = buildRequest(tempUrl);
            HttpWebResponse res = (HttpWebResponse)req.GetResponse();
            string html = processReponse(res);
        }

        private void InitiateSession(string userName, string password)
        {
            if (cc == null)
            {
                cc = new CookieContainer();
                //HttpWebRequest req = buildRequest("https://www.facebook.com/login.php?next=https%3A%2F%2Fwww.facebook.com%2Flogin%2Freauth.php%3Fnext%3Dhttps%253A%252F%252Fwww.facebook.com%252Fdialog%252Fpermissions.request%253Fapp_id%253D111239619098%2526client_id%253D111239619098%2526display%253Dpage%2526redirect_uri%253Dhttps%25253A%25252F%25252Fssl.bing.com%25252Ffd%25252Fauth%25252Fsignin%25253Faction%25253Dfacebook_oauth%252526provider%25253Dfacebook%2526response_type%253Dcode%2526auth_type%253Dhttps%2526fbconnect%253D1%2526_path%253Dpermissions.request%2526from_login%253D1%2526ret%253Dlogin%26app_id%3D111239619098%26signed_next%3D1%26oauth_reauthenticate%3D1%26display%3Dpage&display=page");
                
                // Hit live login page
                HttpWebRequest req = buildRequest("https://login.live.com/login.srf?wa=wsignin1.0&rpsnv=11&ct=1367866611&rver=6.0.5286.0&wp=MBI&wreply=http:%2F%2Fwww.bing.com%2FPassport.aspx%3Frequrl%3Dhttp%253a%252f%252fwww.bing.com%252f&lc=1033&id=264960");
                HttpWebResponse res = (HttpWebResponse)req.GetResponse();
                String html = processReponse(res);

                for (int j = 0; j < res.Cookies.Count; j++)
                {
                    cc.Add(res.Cookies[j]);
                }

                res.Close();

                // Post paramters
                var postParms = new Dictionary<string, string>();
                postParms.Add("login", userName);
                postParms.Add("passwd",password);

                //postParms.Add("PPFT", "CqEm!tdMFHLJunTSLHaPgZwu4CSOXoOEXgzeFxeVUY6QavPv4SjwkLpJnHI3s2BQLvS9DFAlEH6MjtXWyCAy0jeuHRWMmnlJTOmlfAEI9y1V6Yg2XAtDjgGUOCrfj3mQOyfpRYOrWXILpUvljuCzK3PDn5jVblx64EKMgg0wFRKAILA9WQ8W8z3KWmmlqYqKIw$$");
                //postParms.Add("PPSX", "Pas");
                postParms.Add("sso", "0");
                postParms.Add("type", "11");
                postParms.Add("LoginOptions", "3");
                postParms.Add("NewUser", "1");

                // submit post
                HttpWebRequest req1 = buildRequest("https://login.live.com/ppsecure/post.srf");
                sendPostData(req1, buildPostData(postParms));
                HttpWebResponse res1 = (HttpWebResponse)req1.GetResponse();
                String html1 = processReponse(res1);

                _auth = req1.Headers["Authorization"];

                for (int j = 0; j < res.Cookies.Count; j++)
                {
                    cc.Add(res.Cookies[j]);
                }

                res1.Close();
            }
        }

        private void sendPostData(HttpWebRequest req, string postData)
        {
            ASCIIEncoding encoding = new ASCIIEncoding();
            byte[] data = encoding.GetBytes(postData);
            req.Method = "POST";
            req.ContentType = "application/x-www-form-urlencoded";
            req.ContentLength = data.Length;
            req.AllowAutoRedirect = false;
            Stream s = req.GetRequestStream();
            s.Write(data, 0, data.Length);
            s.Close();
        }

        private string buildPostData(Dictionary<string, string> postData)
        {
            StringBuilder sb = new StringBuilder();

            foreach (var data in postData)
            {
                sb.Append("&").Append(data.Key).Append("=").Append(HttpUtility.UrlEncode(data.Value));
            }

            return sb.ToString();
        }

        private string processReponse(HttpWebResponse res)
        {
            string response = null;
            Stream stream = null;
            using (stream = res.GetResponseStream())
            {
                if (res.ContentEncoding.ToLower().Contains("gzip"))
                    stream = new GZipStream(stream, CompressionMode.Decompress);
                else if (res.ContentEncoding.ToLower().Contains("deflate"))
                    stream = new DeflateStream(stream, CompressionMode.Decompress);

                var streamReader = new StreamReader(stream, Encoding.UTF8);
                response = streamReader.ReadToEnd();
            }

            return response;
        }

        private HttpWebRequest buildRequest(string uri)
        {
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(uri);
            req.Accept = "*/*";
            req.Headers.Add("Accept-Encoding: gzip, deflate");
            req.Headers.Add("Accept-Language: en-US,en;q=0.5");
            req.UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64; rv:20.0) Gecko/20100101 Firefox/20.0";

            req.KeepAlive = false;
            req.CookieContainer = cc;

            if (_auth != null)
            {
                req.Headers.Add("Authorization", _auth);
            }

            return req;
        }
    }
}
