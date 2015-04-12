using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebBackend
{
    class ExperimentData
    {
        private string _currentId;
        public TaskInstance Task { get; private set; }

        public bool RefreshTask(string currentId, UserTracker user, bool hasTaskLimit)
        {
            var hasNewID = currentId != _currentId;
            _currentId = currentId;

            if (hasNewID)
            {
                Task = TaskFactory.GetTask(currentId.Sum(c => (int)c), user, hasTaskLimit);
                Task.ReportStart();
            }

            return hasNewID;
        }

        public string CalculateMD5Hash(string input)
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


        internal string GetCurrentSuccessCode()
        {
            var md5 = CalculateMD5Hash("experiment1" + _currentId);

            var sum = md5.Sum(c => (int)c);
            var code = sum % 100000;

            return code.ToString();
        }
    }
}
