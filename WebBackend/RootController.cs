using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;

using ServeRick;

using WebBackend.Experiment;

namespace WebBackend
{
    class RootController : ResponseController
    {
        public void logs()
        {
            //look for all stored experiments
            var experimentIds = new List<string>(Directory.EnumerateDirectories(Program.ExperimentsRootPath).Select(Path.GetFileNameWithoutExtension));

            //select actual experiment
            var currentExperimentId = GET("experiment");
            if (!experimentIds.Contains(currentExperimentId))
                currentExperimentId = experimentIds.Last();

            var experimentRootPath = Path.Combine(Program.ExperimentsRootPath, currentExperimentId);
            var statistics = new Statistics(experimentRootPath);

            SetParam("experiment_statistics", statistics);
            SetParam("experiment_ids", experimentIds);
            SetParam("current_experiment_id", currentExperimentId);

            Layout("layout.haml");
            Render("logs.haml");
        }

        public void log()
        {
            var logFileId = GET("id");
            var experimentId = GET("experiment");

            if (logFileId == null || experimentId == null)
            {
                RedirectTo("logs");
                return;
            }

            switch (logFileId)
            {
                case "feedback":
                    logFileId = "../feedback.json";
                    break;
                case "advices":
                    logFileId = "../" + experimentId + ".dialog";
                    break;
            }

            var logfilePath = Path.Combine(Program.ExperimentsRootPath, experimentId, ExperimentBase.RelativeUserPath, logFileId);
            var file = new LogFile(logfilePath, experimentId);

            var actions = file.LoadActions();

            SetParam("actions", actions);
            SetParam("logfile", file);
            Layout("layout.haml");
            Render("action_log.haml");
        }

        public void index()
        {
            var dialogStorage = GET("storage");
            var solution = getSolution(dialogStorage, 0);

            SetParam("dialog", solution.GetDialogHTML());
            Layout("layout.haml");
            Render("index.haml");
        }

        public void experiment0()
        {
            var experimentName = "experiment0";

            experimentHandler(experimentName);
        }

        public void experiment1()
        {
            var experimentName = "experiment1";

            experimentHandler(experimentName);
        }

        public void public_experiment()
        {
            var experimentName = "public_experiment";

            experimentHandler(experimentName);
        }

        public void data_collection()
        {
            var experimentName = "data_collection";

            experimentHandler(experimentName);
        }

        public void data_collection2()
        {
            var experimentName = "data_collection2";

            experimentHandler(experimentName);
        }

        /// <summary>
        /// Render experiment console. Also handle feedback and tracing of given experiment.
        /// </summary>
        /// <param name="experimentName">Name of experiment.</param>
        private void experimentHandler(string experimentName)
        {
            int taskId;
            int.TryParse(GET("taskid"), out taskId);

            //handle feedback
            var feedbackMessage = GET("feedback");
            if (feedbackMessage != null)
            {
                var user = getUserData();
                user.Feedback(experimentName, taskId, feedbackMessage);

                //redirect because of avoiding multiple feedback sending
                Flash("message", "<h3>Your feedback has been saved. Thank you!</h3>");
                RedirectTo(experimentName);
                return;
            }


            var solution = getSolution(experimentName, taskId);
            solution.LogMessage("visiting " + experimentName + " " + taskId);

            //render the page
            Layout("layout.haml");
            if (solution.HasTask)
            {
                SetParam("dialog", solution.GetDialogHTML());
                SetParam("task", solution.GetTaskText());
                SetParam("experiment_id", experimentName);
                SetParam("task_id", taskId);

                Render("experiment.haml");
            }
            else
            {
                Render("no_tasks.haml");
            }
        }

        public void dialog_data()
        {
            int taskId;
            int.TryParse(GET("taskid"), out taskId);
            var experimentId = GET("experiment_id");
            var utterance = GET("utterance");

            //find user
            var solution = getSolution(experimentId, taskId);

            //process solution input
            if (solution == null)
                return;


            solution.Input(utterance);

            //build html output
            var html = solution.GetDialogHTML();
            if (solution.ValidationCode != null)
            {
                html += "<h4>Task has been succesfully completed</h4><br>Your Crowdflower code: <b>" + solution.ValidationCode + "</b>";
            }

            SetParam("dialog", html);
            Render("dialog.haml");
        }

        #region Utilities

        private UserData getUserData()
        {
            return UserData.GetUserData(this.Client, Program.Experiments);
        }

        private SolutionLog getSolution(string experimentId, int taskId)
        {
            if (experimentId == null)
                experimentId = "index";

            var userData = getUserData();
            return userData.GetSolution(experimentId, taskId);
        }

        #endregion
    }
}
