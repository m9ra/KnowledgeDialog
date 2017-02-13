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
            var file = new LogFile(logfilePath);

            var actions = file.LoadActions();

            SetParam("actions", actions);
            SetParam("logfile", file);
            Layout("layout.haml");
            Render("action_log.haml");
        }

        public void annotate()
        {
            var logFileId = GET("id");
            var experimentId = GET("experiment");

            if (logFileId == null || experimentId == null)
            {
                RedirectTo("logs");
                return;
            }

            var logfilePath = Path.Combine(Program.ExperimentsRootPath, experimentId, ExperimentBase.RelativeUserPath, logFileId);
            var file = new LogFile(logfilePath);
            var annotation = new AnnotatedLogFile(file);

            var actions = file.LoadActions();
            actions = actions.Where(a => a.Type == "T_response" || a.Type == "T_utterance").ToArray();

            var correctAnswers = annotation.GetQuestionAnswers();
            if (POST("save_and_next") != null)
            {
                foreach (var action in actions)
                {
                    var question_annotation = POST("correct_answer_" + action.ActionIndex);
                    correctAnswers[action.ActionIndex] = question_annotation;
                }

                annotation.SetQuestionAnswers(correctAnswers);
                annotation.Save();
            }
            var annotatedActions = annotation.Annotate(actions);

            SetParam("annotated_actions", annotatedActions);
            SetParam("logfile", file);
            Layout("layout.haml");
            Render("annotate.haml");
        }

        public void annotate2()
        {

            //refreshing
            if (GET("action") == "refresh")
                Program.QuestionDialogProvider.Refresh();

            //find id without annotation
            if (GET("action") == "id_without_annotation")
            {
                for (var i = 0; i < Program.QuestionDialogProvider.DialogCount; ++i)
                {
                    var testedDialog = Program.QuestionDialogProvider.GetDialog(i);
                    if (testedDialog.Annotation == null)
                    {
                        RedirectTo("/annotate2?id=" + i);
                        return;
                    }
                }
            }

            //annotation handling
            int annotatedId;
            int.TryParse(POST("annotated_id"), out annotatedId);
            var annotation = POST("annotation");

            if (annotation != null)
            {
                var annotatedDialog = Program.QuestionDialogProvider.GetDialog(annotatedId);
                if (annotatedDialog != null)
                    annotatedDialog.Annotate(annotation);
            }

            //display indexed dialog
            int dialogIndex;
            int.TryParse(GET("id"), out dialogIndex);
            var dialog = Program.QuestionDialogProvider.GetDialog(dialogIndex);


            SetParam("next_id_link", "/annotate2?id=" + (dialogIndex + 1));
            SetParam("previous_id_link", "/annotate2?id=" + (dialogIndex - 1));
            SetParam("refresh_link", "/annotate2?id=" + dialogIndex + "&action=refresh");

            SetParam("first_without_annotation_link", "/annotate2?action=id_without_annotation");
            SetParam("dialog", dialog);
            SetParam("total_dialog_count", Program.QuestionDialogProvider.DialogCount);
            SetParam("dialog_index", dialogIndex.ToString());
            Layout("layout.haml");
            Render("annotate2.haml");
        }

        public void index()
        {
            var dialogStorage = GET("storage");
            var solution = getSolution(dialogStorage, 0);

            SetParam("dialog", solution.GetDialogHTML());
            Layout("layout.haml");
            Render("index.haml");
        }

        public void validate()
        {
            Response.SetAccessControlAllowOrigin_AllDomains();
            int taskId;
            int.TryParse(GET("taskid"), out taskId);
            var code = GET("code");
            var experimentName = GET("experimentId");

            var experiment = Program.Experiments.Get(experimentName);
            var task = experiment == null ? null : experiment.GetTask(taskId);

            var result = task != null && task.ValidationCode.ToString() == code ? "ok" : "no";
            SetParam("message", result);
            Render("validate.haml");
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

        public void question_collection()
        {
            var experimentName = "question_collection";

            experimentHandler(experimentName);
        }

        public void question_collection2()
        {
            var experimentName = "question_collection2";

            experimentHandler(experimentName);
        }

        public void question_collection_r_1()
        {
            var experimentName = "question_collection_r_1";

            experimentHandler(experimentName);
        }

        public void question_collection_r_2()
        {
            var experimentName = "question_collection_r_2";

            experimentHandler(experimentName);
        }

        public void question_collection_r_3()
        {
            var experimentName = "question_collection_r_3";

            experimentHandler(experimentName);
        }

        public void question_collection_r_4()
        {
            var experimentName = "question_collection_r_4";

            experimentHandler(experimentName);
        }

        public void question_collection_r_5()
        {
            var experimentName = "question_collection_r_5";

            experimentHandler(experimentName);
        }

        public void question_collection_r_6()
        {
            var experimentName = "question_collection_r_6";

            experimentHandler(experimentName);
        }

        public void question_collection_r_7()
        {
            var experimentName = "question_collection_r_7";

            experimentHandler(experimentName);
        }

        public void question_collection_r_8()
        {
            var experimentName = "question_collection_r_8";

            experimentHandler(experimentName);
        }

        public void question_collection_r_9()
        {
            var experimentName = "question_collection_r_9";

            experimentHandler(experimentName);
        }

        public void question_collection_r_10()
        {
            var experimentName = "question_collection_r_10";

            experimentHandler(experimentName);
        }

        public void qdd_extension_r_1()
        {
            var experimentName = "qdd_extension_r_1";
            experimentHandler(experimentName);
        }

        public void qdd_extension_r_2()
        {
            var experimentName = "qdd_extension_r_2";
            experimentHandler(experimentName);
        }

        public void qdd_extension_r_3()
        {
            var experimentName = "qdd_extension_r_3";
            experimentHandler(experimentName);
        }

        public void qdd_extension_r_4()
        {
            var experimentName = "qdd_extension_r_4";
            experimentHandler(experimentName);
        }

        public void qdd_extension_r_5()
        {
            var experimentName = "qdd_extension_r_5";
            experimentHandler(experimentName);
        }

        public void answer_extraction()
        {
            var experimentName = "answer_extraction";
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

        public void data_collection3()
        {
            var experimentName = "data_collection3";

            experimentHandler(experimentName);
        }

        public void data_collection4()
        {
            var experimentName = "data_collection4";

            experimentHandler(experimentName);
        }

        public void data_collection5()
        {
            var experimentName = "data_collection5";

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

                Render(solution.ExperimentHAML);
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
