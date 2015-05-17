using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebBackend
{
    class ExperimentData
    {
        private int _currentId;

        private string _experimentId;

        internal static readonly string ExperimentId = "experiment0";

        public TaskInstance Task { get; private set; }

        public bool RefreshTask(int currentId, string experimentId, UserTracker user, bool hasTaskLimit)
        {
            var hasNewID = (currentId != _currentId) || (experimentId != _experimentId);
            _currentId = currentId;
            _experimentId = experimentId;

            if (hasNewID)
            {
                Task = TaskFactory.GetTask(_currentId, user, hasTaskLimit);
                if (Task == null)
                    return false;

                Task.ReportStart();
            }

            return hasNewID;
        }

        internal string GetCurrentSuccessCode()
        {
            return GetSuccessCode(_experimentId, _currentId);
        }

        internal static string CalculateMD5Hash(string input)
        {
            // step 1, calculate MD5 hash from input
            var md5 = System.Security.Cryptography.MD5.Create();
            byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
            byte[] hash = md5.ComputeHash(inputBytes);

            // step 2, convert byte array to hex string
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < hash.Length; i++)
            {
                sb.Append(hash[i].ToString("X2"));
            }
            return sb.ToString();
        }

        internal static string GetSuccessCode(string experimentId, int taskId)
        {
            var md5 = CalculateMD5Hash(experimentId + taskId);

            var sum = md5.Sum(c => (int)c);
            var code = sum % 100000;

            return code.ToString();
        }
    }
}
