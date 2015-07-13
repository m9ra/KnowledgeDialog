﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using WebBackend.Task;

using KnowledgeDialog.Dialog;
using KnowledgeDialog.Database;
using KnowledgeDialog.Knowledge;

namespace WebBackend.Experiment
{
    class SolutionLog
    {
        private WebConsole _console;

        private readonly ExperimentBase _experiment;

        private readonly int _taskId;

        private readonly TaskInstance _task;

        private bool _completitionReported = false;

        private readonly CallStorage _logStorage;

        private readonly CallSerializer _infoCall;

        private readonly CallSerializer _completitionCall;

        internal string ValidationCode { get { return _task != null && _task.IsComplete ? _task.ValidationCode.ToString() : null; } }

        internal bool HasTask { get { return _task != null; } }

        internal SolutionLog(UserData userData, ExperimentBase experiment, int taskId)
        {
            _experiment = experiment;
            _taskId = taskId;
            _task = experiment.GetTask(taskId);

            var isInitialized = false;
            _logStorage = new CallStorage(experiment.GetLogPath(userData.UserID, taskId));
            _infoCall = _logStorage.RegisterCall("Info", c => { isInitialized = true; });
            _completitionCall = _logStorage.RegisterCall("ReportTaskCompletition", c => { });

            _logStorage.ReadStorage();
            if (!isInitialized)
                logInfo("solution initialized");

            reportTaskStart(_task);

            //console has to be created after log storage is prepared
            _console = createConsole();
        }

        internal void Input(string utterance)
        {
            var isReset = utterance != null && utterance.Trim().ToLowerInvariant() == "reset";
            logUtterance(utterance);
            if (isReset)
            {
                _console = createConsole();
            }
            else
            {
                _console.Input(utterance);
                logResponse(_console.LastResponse);

                if (HasTask)
                    _task.Register(_console.LastResponse);

                if (HasTask && _task.IsComplete)
                {
                    //task has been completed
                    if (!_completitionReported)
                        reportCompletition();
                }
            }
        }


        internal string GetDialogHTML()
        {
            return _console.CurrentHTML;
        }

        internal string GetTaskText()
        {
            return _task.Text;
        }


        internal void LogMessage(string message)
        {
            _infoCall.ReportParameter("message", message);
            _infoCall.SaveReport();
        }

        #region Logging

        private void reportTaskStart(TaskInstance task)
        {
            if (task == null)
                //there is no task available
                return;

            _infoCall.ReportParameter("task", task.Key);
            _infoCall.ReportParameter("format", task.TaskFormat);
            _infoCall.ReportParameter("substitutions", task.Substitutions);
            _infoCall.SaveReport();
        }


        private void reportCompletition()
        {
            if (!HasTask)
                //nothing to report
                return;

            _completitionCall.ReportParameter("task", _task.Key);
            _completitionCall.ReportParameter("format", _task.TaskFormat);
            _completitionCall.ReportParameter("substitutions", _task.Substitutions);
            _completitionCall.SaveReport();
        }

        private void logUtterance(string utterance)
        {
            _infoCall.ReportParameter("utterance", utterance);
            _infoCall.SaveReport();
        }

        private void logResponse(ResponseBase response)
        {
            var responseText = response == null ? null : response.ToString();
            _infoCall.ReportParameter("response", responseText);
            _infoCall.SaveReport();
        }

        private void logInfo(string info)
        {
            _infoCall.ReportParameter("message", info);
            _infoCall.SaveReport();
        }

        #endregion

        #region Private utilities

        private WebConsole createConsole()
        {
            logInfo("new console ");
            var databasePath = _experiment.GetDatabasePath("dialog");
            return new WebConsole(databasePath);
        }

        #endregion
    }
}