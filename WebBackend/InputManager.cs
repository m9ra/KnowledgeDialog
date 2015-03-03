using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ServeRick;
using ServeRick.Networking;
using ServeRick.Modules.Input;

namespace WebBackend
{
    class InputManager : InputManagerBase
    {
        protected override InputController createController(HttpRequest request)
        {
            return new UrlEncodedInput(65535);
        }
    }
}
