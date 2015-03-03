using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ServeRick;

namespace WebBackend
{
    class RootController : ResponseController
    {
        public void index()
        {
            SetParam("dialog", getDialogHTML());
            Layout("layout.haml");
            Render("index.haml");
        }

        public void dialog_data()
        {
            var utterance = GET("utterance");
            var dialog = getDialogHTML(utterance);

            SetParam("dialog", dialog);
            Render("dialog.haml");
        }

        private string getDialogHTML(string utterance = null)
        {
            var console = Session<WebConsole>();
            var isReset = (utterance != null && utterance.Trim() == "reset");


            if (isReset || console == null)
            {
                //create new instance of console
                console = new WebConsole();
                Session<WebConsole>(console);
            }

            if (!isReset && utterance != null)
                console.Input(utterance);

            return console.CurrentHTML;
        }
    }
}
