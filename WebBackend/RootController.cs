using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;

using ServeRick;

namespace WebBackend
{
    class RootController : ResponseController
    {
        public void index()
        {
            var dialogStorage = GET("storage");

            SetParam("dialog", getDialogHTML(dialogStorage));
            Layout("layout.haml");
            Render("index.haml");
        }

        public void experiment()
        {
            SetParam("dialog", getDialogHTML("experiment"));
            Layout("layout.haml");
            ContentFor("task", "data/tasks/task1.htm");
            Render("experiment.haml");
        }

        public void dialog_data()
        {
            var utterance = GET("utterance");
            //storage is determined by sess
            var dialog = getDialogHTML(Session<DialogLog>().StorageName, utterance);

            SetParam("dialog", dialog);
            Render("dialog.haml");
        }

        private string getDialogHTML(string dialogStorageName, string utterance = null)
        {
            var console = Session<WebConsole>();
            var isReset = (utterance != null && utterance.Trim() == "reset");


            if (isReset || console == null)
            {
                var storageFullPath = dialogStorageName==null ? null : Path.Combine(RootPath, "data/storages", dialogStorageName) + ".dialog";
                Session<DialogLog>(new DialogLog(dialogStorageName));

                //create new instance of console
                console = new WebConsole(storageFullPath);
                Session<WebConsole>(console);
            }

            if (!isReset && utterance != null)
                console.Input(utterance);

            return console.CurrentHTML;
        }
    }
}
