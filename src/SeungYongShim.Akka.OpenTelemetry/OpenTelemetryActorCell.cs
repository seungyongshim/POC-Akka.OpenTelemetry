using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Actor.Internal;
using Akka.Dispatch;

namespace SeungYongShim.Akka.OpenTelemetry
{
    public class OpenTelemetryActorCell : ActorCell
    {
        public OpenTelemetryActorCell(ActorSystemImpl system, IInternalActorRef self, Props props, MessageDispatcher dispatcher, IInternalActorRef parent) : base(system, self, props, dispatcher, parent)
        {
            
        }

    }
}
