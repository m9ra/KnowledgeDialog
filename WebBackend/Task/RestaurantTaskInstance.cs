using KnowledgeDialog.Dialog;
using KnowledgeDialog.Knowledge;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebBackend.Task
{
    class RestaurantTaskInstance : InformativeTaskInstance
    {
        internal RestaurantTaskInstance(int id, string taskFormat, string key, int validationCodeKey, string experimentHAML = "experiment.haml")
            : base(id, taskFormat, new NodeReference[0], new NodeReference[0], key, validationCodeKey, 1, experimentHAML)
        {
        }
    }
}