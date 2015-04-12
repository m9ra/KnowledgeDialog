using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;

using ServeRick;
using ServeRick.Networking;

using KnowledgeDialog.Database;
using KnowledgeDialog.Knowledge;

namespace WebBackend
{
    class UserTracker
    {
        /// <summary>
        /// Lock for trackers index.
        /// </summary>
        private static readonly object _L_trackers = new object();

        /// <summary>
        /// Stored trackers.
        /// </summary>
        private static readonly Dictionary<string, UserTracker> _trackers = new Dictionary<string, UserTracker>();

        private static readonly CallStorage _feedbackStorage;

        private static readonly CallSerializer _feedbackCall;

        /// <summary>
        /// Mapping between storages and consolas.
        /// </summary>
        private readonly Dictionary<string, WebConsole> _consoleMapping = new Dictionary<string, WebConsole>();

        private readonly List<string> _tasks = new List<string>();

        private readonly CallStorage _storage;

        private readonly CallSerializer _infoCall;

        private readonly CallSerializer _completitionCall;

        private string _actualStorageFullpath;

        /// <summary>
        /// Time when user has been seen for first time.
        /// </summary>
        public readonly DateTime EntryTime = DateTime.Now;

        /// <summary>
        /// ID of session.
        /// </summary>
        public readonly string UserID;

        public bool HasTaskLimit;
        public KnowledgeDialog.Dialog.ResponseBase LastResponse { get; private set; }

        /// <summary>
        /// Tasks that has been completed by user.
        /// </summary>
        public IEnumerable<string> CompletedTasks { get { return _tasks; } }

        public WebConsole ActualConsole { get { return getConsole(); } }

        static UserTracker()
        {
            var feedbackPath = Path.Combine(Program.RootPath, "data/storages", "feedback" + ".json");
            _feedbackStorage = new CallStorage(feedbackPath);
            _feedbackCall = _feedbackStorage.RegisterCall("Feedback", (c) => { });
        }

        private UserTracker(string id)
        {
            UserID = id;
            var fullPath = Path.Combine(Program.RootPath, "data/storages/users", "user_" + id + ".json");
            _storage = new CallStorage(fullPath);

            var isInitialized = false;
            _infoCall = _storage.RegisterCall("Info", c => { isInitialized = true; });
            _completitionCall = _storage.RegisterCall("ReportTaskCompletition", c => ReportTaskCompletition(c.String("task"), c.String("format"), c.Nodes("substitutions", DialogWeb.Graph)));

            _storage.ReadStorage();

            if (!isInitialized)
                logInfo("tracker initialized");
        }

        /// <summary>
        /// Get tracker, that is associated with given client.
        /// </summary>
        /// <param name="client">Client which tracker is requested.</param>
        /// <returns>Requested tracker.</returns>
        public static UserTracker GetTracker(Client client)
        {
            //TODO maybe distinguish according to IP or browser entropy
            var id = client.SessionID;

            lock (_L_trackers)
            {
                UserTracker tracker;
                if (!_trackers.TryGetValue(id, out tracker))
                {
                    _trackers[id] = tracker = new UserTracker(id);
                }

                return tracker;
            }
        }

        /// <summary>
        /// Reset console associated with given storage.
        /// </summary>
        /// <param name="storageFullPath">Full path of associated storage.</param>
        internal void ResetConsole()
        {
            logUtterance("reset");
            if (_actualStorageFullpath == null)
                _actualStorageFullpath = "";
            WebConsole console;
            if (!_consoleMapping.TryGetValue(_actualStorageFullpath, out console))
                //nothing to do
                return;

            console.Close();
            _consoleMapping.Remove(_actualStorageFullpath);
        }

        /// <summary>
        /// Put given utterance into console associated with storage.
        /// </summary>
        /// <param name="utterance"></param>
        internal void Input(string utterance)
        {
            logUtterance(utterance);
            var console = getConsole();
            if (utterance == null)
            {
                LastResponse = null;
            }
            else
            {
                LastResponse = console.Input(utterance);
            }
        }

        internal void Feedback(string message)
        {
            lock (_L_trackers)
            {
                _feedbackCall.ReportParameter("time", DateTime.Now.ToString());
                _feedbackCall.ReportParameter("user_id", UserID);
                _feedbackCall.ReportParameter("message", message);
                _feedbackCall.ReportParameter("actual_storage", _actualStorageFullpath);
                _feedbackCall.SaveReport();
            }
        }

        internal void SetActualStorage(string storageFullPath)
        {
            _actualStorageFullpath = storageFullPath;
        }

        internal string GetDialogHTML()
        {
            var console = getConsole();
            if (console == null)
                return null;

            var html = console.CurrentHTML;
            return html;
        }

        internal void ReportTaskCompletition(string task, string format, IEnumerable<NodeReference> substitutions)
        {
            _completitionCall.ReportParameter("task", task);
            _completitionCall.ReportParameter("format", format);
            _completitionCall.ReportParameter("substitutions", substitutions);
            _completitionCall.SaveReport();
            _tasks.Add(task);
        }

        internal void ReportTaskStart(string task, string format, IEnumerable<NodeReference> substitutions)
        {
            _infoCall.ReportParameter("task", task);
            _infoCall.ReportParameter("format", format);
            _infoCall.ReportParameter("substitutions", substitutions);
            _infoCall.SaveReport();
        }

        internal void LogMessage(string message)
        {
            _infoCall.ReportParameter("time", message);
            _infoCall.SaveReport();
        }

        /// <summary>
        /// Get console according to desired storage. 
        /// <remarks>State of webconsole is not persistant.</remarks>
        /// </summary>
        /// <param name="storageFullpath">Full path of storage file.</param>
        /// <returns>Console with given storage</returns>
        private WebConsole getConsole()
        {
            var storageFullpath = _actualStorageFullpath;
            if (storageFullpath == null)
                storageFullpath = "";

            WebConsole console;
            if (!_consoleMapping.TryGetValue(storageFullpath, out console))
            {
                _consoleMapping[storageFullpath] = console = new WebConsole(storageFullpath);
                logInfo("new console " + storageFullpath);
            }

            return console;
        }

        private void logUtterance(string utterance)
        {
            _infoCall.ReportParameter("time", DateTime.Now.ToString());
            _infoCall.ReportParameter("storage", _actualStorageFullpath);
            _infoCall.ReportParameter("utterance", utterance);
            _infoCall.SaveReport();
        }

        private void logInfo(string info)
        {
            _infoCall.ReportParameter("time", DateTime.Now.ToString());
            _infoCall.ReportParameter("message", info);
            _infoCall.SaveReport();
        }
    }
}
