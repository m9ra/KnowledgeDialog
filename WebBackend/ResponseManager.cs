using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ServeRick;

namespace WebBackend
{
    class ResponseManager : ResponseManagerBase
    {
        internal ResponseManager(WebApplication app, string rootPath)
            : base(app, rootPath,
            typeof(RootController)
            )
        {
            ErrorPage(404, "404.haml");
            PublicExtensions("png", "gif", "jpg", "css", "js", "scss", "md", "swf", "ico", "txt");
        }
    }
}
