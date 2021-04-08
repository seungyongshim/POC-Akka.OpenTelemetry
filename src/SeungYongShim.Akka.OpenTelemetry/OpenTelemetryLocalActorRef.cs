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
    public class OpenTelemetryLocalActorRef : LocalActorRef
    {
        public OpenTelemetryLocalActorRef(ActorSystemImpl system, Props props, MessageDispatcher dispatcher, MailboxType mailboxType, IInternalActorRef supervisor, ActorPath path) : base(system, props, dispatcher, mailboxType, supervisor, path)
        {
        }

        protected override ActorCell NewActorCell(ActorSystemImpl system, IInternalActorRef self, Props props, MessageDispatcher dispatcher, IInternalActorRef supervisor)
        {
            return new OpenTelemetryActorCell(system, self, props, dispatcher, supervisor);
        }

    }
}
