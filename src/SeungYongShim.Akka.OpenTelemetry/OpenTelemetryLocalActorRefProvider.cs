using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Actor.Internal;
using Akka.Event;
using Akka.Routing;
using Akka.Serialization;

namespace SeungYongShim.Akka.OpenTelemetry
{
    public class OpenTelemetryLocalActorRefProvider : IActorRefProvider
    {
        private ActorSystemImpl _system;

        public OpenTelemetryLocalActorRefProvider(string systemName, Settings settings, EventStream eventStream)
            : this(systemName, settings, eventStream, null, null)
        {
            //Intentionally left blank
        }

        public OpenTelemetryLocalActorRefProvider(string systemName, Settings settings, EventStream eventStream, Deployer deployer, Func<ActorPath, IInternalActorRef> deadLettersFactory)
        {
            var actorRefProvider = new LocalActorRefProvider(systemName, settings, eventStream, deployer, deadLettersFactory);
            Log = actorRefProvider.Log;
            
            ActorRefProvider = actorRefProvider;
        }

        public IInternalActorRef ActorOf(ActorSystemImpl system, Props props, IInternalActorRef supervisor, ActorPath path, bool systemService, Deploy deploy, bool lookupDeploy, bool async)
        {
            if (props.Deploy.RouterConfig is NoRouter)
            {
                if (Settings.DebugRouterMisconfiguration)
                {
                    var d = Deployer.Lookup(path);
                    if (d != null && !(d.RouterConfig is NoRouter))
                        Log.Warning("Configuration says that [{0}] should be a router, but code disagrees. Remove the config or add a RouterConfig to its Props.",
                                    path);
                }

                var props2 = props;

                // mailbox and dispatcher defined in deploy should override props
                var propsDeploy = lookupDeploy ? Deployer.Lookup(path) : deploy;
                if (propsDeploy != null)
                {
                    if (propsDeploy.Mailbox != Deploy.NoMailboxGiven)
                        props2 = props2.WithMailbox(propsDeploy.Mailbox);
                    if (propsDeploy.Dispatcher != Deploy.NoDispatcherGiven)
                        props2 = props2.WithDispatcher(propsDeploy.Dispatcher);
                }

                if (!system.Dispatchers.HasDispatcher(props2.Dispatcher))
                {
                    throw new ConfigurationException($"Dispatcher [{props2.Dispatcher}] not configured for path {path}");
                }

                try
                {
                    // for consistency we check configuration of dispatcher and mailbox locally
                    var dispatcher = _system.Dispatchers.Lookup(props2.Dispatcher);
                    var mailboxType = _system.Mailboxes.GetMailboxType(props2, dispatcher.Configurator.Config);

                    if (async)
                        return
                            new RepointableActorRef(system, props2, dispatcher,
                                mailboxType, supervisor,
                                path).Initialize(async);
                    return new LocalActorRef(system, props2, dispatcher,
                        mailboxType, supervisor, path);
                }
                catch (Exception ex)
                {
                    throw new ConfigurationException(
                        $"Configuration problem while creating [{path}] with dispatcher [{props.Dispatcher}] and mailbox [{props.Mailbox}]", ex);
                }
            }
            else //routers!!!
            {
                throw new ConfigurationException("Not Support router, Current Version");
            }
        }

        public ILoggingAdapter Log { get; }
        public IActorRefProvider ActorRefProvider { get; }

        public IInternalActorRef RootGuardian => ActorRefProvider.RootGuardian;

        public LocalActorRef Guardian => ActorRefProvider.Guardian;

        public LocalActorRef SystemGuardian => ActorRefProvider.SystemGuardian;

        public IActorRef DeadLetters => ActorRefProvider.DeadLetters;

        public IActorRef IgnoreRef => ActorRefProvider.IgnoreRef;

        public ActorPath RootPath => ActorRefProvider.RootPath;

        public Settings Settings => ActorRefProvider.Settings;

        public Deployer Deployer => ActorRefProvider.Deployer;

        public IInternalActorRef TempContainer => ActorRefProvider.TempContainer;

        public Task TerminationTask => ActorRefProvider.TerminationTask;

        public Address DefaultAddress => ActorRefProvider.DefaultAddress;

        public Information SerializationInformation => ActorRefProvider.SerializationInformation;


            
        public Address GetExternalAddressFor(Address address) => ActorRefProvider.GetExternalAddressFor(address);
        public void Init(ActorSystemImpl system)
        {
            _system = system;
            ActorRefProvider.Init(system);
        }
        public void RegisterTempActor(IInternalActorRef actorRef, ActorPath path) => ActorRefProvider.RegisterTempActor(actorRef, path);
        public IActorRef ResolveActorRef(string path) => ActorRefProvider.ResolveActorRef(path);
        public IActorRef ResolveActorRef(ActorPath actorPath) => ActorRefProvider.ResolveActorRef(actorPath);
        public IActorRef RootGuardianAt(Address address) => ActorRefProvider.RootGuardianAt(address);
        public ActorPath TempPath() => ActorRefProvider.TempPath();
        public void UnregisterTempActor(ActorPath path) => ActorRefProvider.UnregisterTempActor(path);
    }
}
