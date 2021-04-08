using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenTelemetry.Trace;

namespace ConsoleApp
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            await Host.CreateDefaultBuilder()
                      .UseAkka("open-telemetry-system", @"
                      akka {
                          stdout-loglevel = DEBUG
                          loglevel = DEBUG
                          log-config-on-start = on
                          actor {
                              provider = ""SeungYongShim.Akka.OpenTelemetry.LocalActorRefProvider, SeungYongShim.Akka.OpenTelemetry""
                              debug {
                                    receive = on
                                    autoreceive = on
                                    lifecycle = on
                                    event-stream = on
                                    unhandled = on
                              }
                          }
                      }

                      ", (sp, sys) =>
                      {

                      })
                      .ConfigureServices(service =>
                      {
                          service.AddOpenTelemetryTracing((builder) => builder
                                 .AddSource("Actor")
                                 .SetSampler(new AlwaysOnSampler())
                                 .AddZipkinExporter());
                      })
                      .RunConsoleAsync();
        }
    }
}
