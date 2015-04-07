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
            var tracker = fillAndGetTracker(dialogStorage);

            SetParam("dialog", tracker.GetDialogHTML());
            Layout("layout.haml");
            Render("index.haml");
        }

        public void experiment()
        {
            var experimentData = Session<ExperimentData>();
            if (experimentData == null)
                experimentData = Session<ExperimentData>(new ExperimentData());

            var tracker = fillAndGetTracker("experiment");
            var id = GET("taskid");
            if (experimentData.CheckIdChange(id))
            {
                tracker.LogMessage("start " + id);
                tracker.Remove();
            }

            if (tracker.ActualConsole != null && tracker.ActualConsole.Task != null && tracker.ActualConsole.Task.IsComplete)
                //user is returning to experiment page after task completition - we can reset it
                tracker.Reset();


            Layout("layout.haml");
            if (tracker.ActualConsole == null || tracker.ActualConsole.Task == null)
            {
                Render("no_tasks.haml");
            }
            else
            {
                SetParam("dialog", tracker.GetDialogHTML());
                SetParam("task", tracker.ActualConsole.Task.Text);


                Render("experiment.haml");
            }
        }

        public void dialog_data()
        {
            var utterance = GET("utterance");
            var tracker = fillAndGetTracker(null, utterance);
            if (tracker.ActualConsole == null)
                return;

            var task = tracker.ActualConsole.Task;
            var html = tracker.GetDialogHTML();
            if (task != null)
            {
                if (task.IsComplete)
                {
                    if (!task.CompletitionReported)
                        task.ReportCompletition();

                    var code = getSuccessCode();
                    html += "<h4>Task has been succesfully completed</h4><br>Your Crowdflower code: <b>" + code + "</b>";
                }
            }

            SetParam("dialog", html);
            Render("dialog.haml");
        }

        private string getSuccessCode()
        {
            var data = Session<ExperimentData>();
            if (data == null)
                return "no code";

            return data.GetCurrentSuccessCode();
        }

        private UserTracker fillAndGetTracker(string dialogStorageName, string utterance = null)
        {
            var storageFullPath = dialogStorageName == null ? null : Path.Combine(RootPath, "data/storages", dialogStorageName) + ".dialog";

            var tracker = getTracker();
            if (storageFullPath != null)
                tracker.SetActualStorage(storageFullPath);

            var isReset = (utterance != null && utterance.Trim() == "reset");

            if (isReset)
                tracker.Reset();
            else
                tracker.Input(utterance);

            return tracker;
        }

        private UserTracker getTracker()
        {
            return UserTracker.GetTracker(this.Client);
        }
    }
}
