using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ServeRick;

namespace WebBackend
{
    class DialogWeb : WebApplication
    {
        private readonly string _root;

        public DialogWeb(string wwwPath)
        {
            _root = wwwPath;
        }


        protected override ResponseManagerBase createResponseManager()
        {
            var manager = new ResponseManager(this, _root);
            manager.AddDirectoryContent("");
            manager.AddDirectoryContent("images");
            manager.AddDirectoryContent("js");
            manager.AddDirectoryContent("css");
            manager.AddDirectoryContent("data/tasks");  

            return manager;
        }

        protected override Type[] getHelpers()
        {
            return new Type[]{
                
            };
        }


        protected override InputManagerBase createInputManager()
        {
            return new InputManager();
        }

        protected override IEnumerable<ServeRick.Database.DataTable> createTables()
        {
            return new ServeRick.Database.DataTable[0];
        }
    }
}
