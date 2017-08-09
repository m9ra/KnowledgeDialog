using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;

using ServeRick;
using ServeRick.Networking;

using KnowledgeDialog.Dialog;
using KnowledgeDialog.Database;
using KnowledgeDialog.Knowledge;

using WebBackend.Experiment;

namespace WebBackend
{
    class UserData
    {
        /// <summary>
        /// Lock for users index.
        /// </summary>
        private static readonly object _L_users = new object();

        /// <summary>
        /// Stored users.
        /// </summary>
        private static readonly Dictionary<string, UserData> _data = new Dictionary<string, UserData>();

        /// <summary>
        /// Mapping from task id to corresponding console.
        /// </summary>
        private readonly Dictionary<Tuple<string, int>, SolutionLog> _taskToSolution = new Dictionary<Tuple<string, int>, SolutionLog>();

        private static readonly Dictionary<string, CallSerializer> _experimentToFeedbackCall = new Dictionary<string, CallSerializer>();

        /// <summary>
        /// All available experiments.
        /// </summary>
        internal readonly ExperimentCollection Experiments;

        /// <summary>
        /// ID of session.
        /// </summary>
        public readonly string UserID;

        private UserData(string id, ExperimentCollection experiments)
        {
            UserID = id;
            Experiments = experiments;
        }

        /// <summary>
        /// Get data, that is associated with given client.
        /// </summary>
        /// <param name="client">Client which data is requested.</param>
        /// <returns>Requested data.</returns>
        public static UserData GetUserData(Client client, ExperimentCollection experiments)
        {
            //TODO maybe distinguish according to IP or browser entropy
            var id = client.SessionID;

            lock (_L_users)
            {
                UserData data;
                if (!_data.TryGetValue(id, out data))
                {
                    _data[id] = data = new UserData(id, experiments);
                }

                return data;
            }
        }

        internal void Feedback(string experimentId, int taskId, string message)
        {
            lock (_L_users)
            {
                CallSerializer _feedbackCall;
                if (!_experimentToFeedbackCall.TryGetValue(experimentId, out _feedbackCall))
                {
                    var experiment = Experiments.Get(experimentId);
                    if (experiment != null)
                    {
                        var feedbackPath = experiment.GetFeedbackPath();
                        var feedbackStorage = new CallStorage(feedbackPath);
                        _feedbackCall = feedbackStorage.RegisterCall("Feedback", (c) => { });
                        _experimentToFeedbackCall[experimentId] = _feedbackCall;
                    }
                }


                _feedbackCall.ReportParameter("user_id", UserID);
                _feedbackCall.ReportParameter("message", message);
                _feedbackCall.ReportParameter("experiment_id", experimentId);
                _feedbackCall.ReportParameter("task_id", taskId.ToString());
                _feedbackCall.SaveReport();
            }
        }


        internal SolutionLog GetSolution(string experimentId, int taskId)
        {
            var key = Tuple.Create(experimentId, taskId);
            SolutionLog log;
            if (!_taskToSolution.TryGetValue(key, out log))
            {
                var experiment = Experiments.Get(experimentId);
                if (experiment == null)
                    return null;

                _taskToSolution[key] = log = new SolutionLog(this, experiment, taskId);
            }


            return log;
        }
    }
}
