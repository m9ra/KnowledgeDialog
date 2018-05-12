using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PerceptiveDialogBasedAgent.V4.Models
{
    class PolicyModel
    {
        internal PolicyModel()
        {
            //NATIVE ACTIONS

            //AddForExecution
            //Transform
            //SetParameter


           /* this
           
            .With("a", AnyInputPhrase)
            .With("an", AnyInputPhrase)
            .Do(
                Join()
                )

            .With(
                InputPattern()
                    .InstanceOf(Concept2.NativeAction)
                )
            .Do(
                AddForExecution()
                )
                */
            ;
        }

        internal PolicyModel When()
        {
            throw new NotImplementedException();
        }

        internal PolicyModel Do()
        {
            throw new NotImplementedException();
        }
    }
}
