using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ServeRick;

namespace WebBackend
{
    public class FormatHelper : Helper
    {
        readonly static float KB = 1 << 10;


        public static string LinkTo(LogFile file)
        {
            return LinkToLog(file.Id, file.ExperimentId);
        }

        public static string LogFileId(ActionEntry action)
        {
            if (action.TaskId < 0)
                return "user_" + action.UserId + ".json";
            else
                return string.Format("{0}-{1}.json", action.TaskId, action.UserId);
        }

        public static string LinkToLog(string id, string experimentId)
        {
            return "/log?experiment=" + experimentId + "&id=" + id;
        }

        public static string LinkToExperiment(string experimentId)
        {
            return "/logs?experiment=" + experimentId;
        }

        public static string Size(int bytes)
        {
            var result = string.Format("{0:0.##} KB", bytes / KB);

            return result;
        }

        public static string DateTime(DateTime time)
        {
            var result = string.Format("{0:00}.{1:00}.{2} - {3:00}:{4:00}", time.Day, time.Month, time.Year, time.Hour, time.Minute);
            return result;
        }

        public static string Time(TimeSpan time)
        {
            if (time.TotalMilliseconds == 0)
                return "N/A";

            if (time.Hours == 0)
            {
                return time.ToString(@"mm\:ss\s");
            }
            else
            {
                return time.ToString(@"hh\:mm\:ss\s");
            }
        }

        public static string ln(string data)
        {
            return data + "\n";
        }

        public static string Flash(Response response, string messageID)
        {
            var value = response.Flash(messageID);
            if (value == null)
                return null;

            return string.Format("<div class=\"{0}\">{1}</div>", messageID, value);
        }
    }
}
