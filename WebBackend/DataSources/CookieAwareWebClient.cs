using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using System.Net;
using System.Collections.Specialized;

namespace WebBackend.DataSources
{
    public class CookieAwareWebClient : WebClient
    {
        public CookieContainer CookieContainer { get; private set; }

        private readonly int _timeout;

        public CookieAwareWebClient(int timeout = 30 * 1000)
        {
            _timeout = timeout;
            CookieContainer = new CookieContainer();
        }

        internal string SendValues(string url, NameValueCollection values)
        {
            var bytes = UploadValues(url, values);

            return Encoding.ASCII.GetString(bytes);
        }

        protected override WebRequest GetWebRequest(Uri address)
        {
            var request = (HttpWebRequest)base.GetWebRequest(address);
            request.CookieContainer = CookieContainer;
            request.Timeout = _timeout;
            return request;
        }
    }
}
