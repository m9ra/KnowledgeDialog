using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebBackend.Dataset
{
    class PhraseLearningDatasetWriter
    {
        IEnumerable<TaskDialogAnnotation> _dialogs;

        internal PhraseLearningDatasetWriter(PhraseLearningDialogProvider dialogProvider)
        {
            dialogProvider.Refresh();
            var dialogs = new List<TaskDialogAnnotation>();

            for (int i = 0; i < dialogProvider.DialogCount; i++)
            {
                var dialog = dialogProvider.GetDialog(i);
                dialogs.Add(dialog);
            }

            _dialogs = dialogs.ToArray();
        }

        internal void WriteValidDialogs(string targetFile)
        {
            var dialogRepresentations = new List<Dictionary<string, object>>();

            Console.WriteLine($"EXPORTING {_dialogs.Count()} to {targetFile}");

            var invalidDialogCount = 0;
            foreach (var dialog in _dialogs)
            {
                var annotation = dialog.Annotation;
                if (annotation == "invalid")
                {
                    invalidDialogCount += 1;
                    continue;
                }

                int? forcedSuccessValue = null;
                switch (annotation)
                {
                    case "override-1":
                        forcedSuccessValue = -1;
                        break;
                    case "override+0":
                        forcedSuccessValue = 0;
                        break;
                    case "override+1":
                        forcedSuccessValue = 1;
                        break;
                    case "override+2":
                        forcedSuccessValue = 2;
                        break;
                    case "valid":
                        break;

                    default:
                        Console.WriteLine("Unrecognized annotation: " + annotation);
                        break;
                }

                var representation = getDialogRepresentation(dialog,annotation, forcedSuccessValue);
                dialogRepresentations.Add(representation);
            }

            Console.WriteLine($"\t invalid dialogs detected: {invalidDialogCount}");
            Console.WriteLine($"\t valid dialogs detected: {dialogRepresentations.Count}");

            var json = Newtonsoft.Json.JsonConvert.SerializeObject(dialogRepresentations);
            File.WriteAllText(targetFile, json);
        }

        private Dictionary<string, object> getDialogRepresentation(TaskDialogAnnotation dialog,string annotation, int? forcedSuccessCode)
        {
            var r = new Dictionary<string, object>();

            var d = dialog.Dialog;
            r["Id"] = d.Id;
            r["Task"] = d.Task;
            r["ManualAnnotation"] = annotation;
            r["SuccessCode"] = forcedSuccessCode ?? d.SuccessCode;
            r["ExperimentId"] = d.ExperimentId;
            r["DialogEvents"] = getEventsRepresentation(dialog.Dialog);
            return r;
        }

        private List<Dictionary<string, object>> getEventsRepresentation(TaskDialog dialog)
        {
            var result = new List<Dictionary<string, object>>();
            foreach (var entry in dialog.Entries)
            {
                if (
                    entry.IsRegularTurn ||
                    entry.IsCompletition ||
                    entry.IsInfo
                    )

                    result.Add(getEventRepresentation(entry));
            }

            return result;
        }

        private Dictionary<string, object> getEventRepresentation(ActionEntry entry)
        {
            var r = new Dictionary<string, object>();
            r["Type"] = entry.Type;
            r["Text"] = entry.Text;
            r["Time"] = entry.Time;

            return r;
        }
    }
}
