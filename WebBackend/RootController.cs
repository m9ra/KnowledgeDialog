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
            var tracker = fillAndGetUserTracker(dialogStorage);

            SetParam("dialog", tracker.GetDialogHTML());
            Layout("layout.haml");
            Render("index.haml");
        }

        public void experiment0()
        {
            var experimentName = "experiment0";

            experimetHandler(experimentName, true);
        }

        public void public_experiment()
        {
            var experimentName = "public_experiment";

            experimetHandler(experimentName, false);
        }

        private void experimetHandler(string experimentName, bool enableTaskLimit)
        {

            //require experiment data for each user
            var experimentData = Session<ExperimentData>();
            if (experimentData == null)
                experimentData = Session<ExperimentData>(new ExperimentData());

            var user = fillAndGetUserTracker(experimentName);
            user.HasTaskLimit = enableTaskLimit;

            //handle feedback
            var feedback = GET("feedback");
            if (feedback != null)
            {
                user.Feedback(feedback);

                //redirect because of avoiding multiple feedback sending
                Flash("message", "<h3>Your feedback has been saved. Thank you!</h3>");
                RedirectTo(experimentName);
                return;
            }


            //handle dialog initialization
            var id = GET(experimentName + "taskid");
            if (experimentData.CheckIdChange(id))
            {
                //id of task has changed - we have to remove old dialog console
                user.LogMessage("start " + id);
                user.ResetConsole();
            }

            if (user.ActualConsole != null && user.ActualConsole.Task != null && user.ActualConsole.Task.IsComplete)
                //user is returning to experiment page after task completition - we can reset it
                user.ResetConsole();


            //render the page
            Layout("layout.haml");
            if (user.ActualConsole == null || user.ActualConsole.Task == null)
            {
                Render("no_tasks.haml");
            }
            else
            {
                SetParam("dialog", user.GetDialogHTML());
                SetParam("task", user.ActualConsole.Task.Text);

                Render("experiment.haml");
            }
        }

        public void dialog_data()
        {
            var utterance = GET("utterance");
            var tracker = fillAndGetUserTracker(null, utterance);
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

        private UserTracker fillAndGetUserTracker(string dialogStorageName, string utterance = null)
        {
            var storageFullPath = dialogStorageName == null ? null : Path.Combine(RootPath, "data/storages", dialogStorageName) + ".dialog";

            var tracker = getTracker();
            if (storageFullPath != null)
                tracker.SetActualStorage(storageFullPath);

            var isReset = (utterance != null && utterance.Trim() == "reset");

            if (isReset)
                tracker.ResetConsole();
            else if (utterance != null)
                tracker.Input(utterance);

            return tracker;
        }

        private UserTracker getTracker()
        {
            return UserTracker.GetTracker(this.Client);
        }
    }
}
