using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Graph;
using Microsoft.Identity.Web;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ConsoleMSGraph
{
  class Program
  {
    public static async Task Main(string[] args)
    {
      Console.WriteLine("Hello World!");

      var config = new ConfigurationBuilder()
        .AddJsonFile($"appsettings.json", optional: true, reloadOnChange: true)
        .Build();

      var builder = new HostBuilder().ConfigureServices((hostContext, services) =>
      {
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
              .AddMicrosoftIdentityWebApi(config.GetSection("AzureAd"))
                  .EnableTokenAcquisitionToCallDownstreamApi()
                      .AddMicrosoftGraph(config.GetSection("DownstreamApi"))
                      .AddInMemoryTokenCaches();
      }).UseConsoleLifetime();

      var services = builder.Build().Services.CreateScope().ServiceProvider;
      Console.WriteLine("Hydrator configuration loaded. Begin fetch and cache...");

      var msapp = await GetServiceFromAzureAsync(services);
    }

    public static async Task<Microsoft.Graph.Application> GetServiceFromAzureAsync(IServiceProvider injectedProvider)
    {

      var serviceId =  2555417833004;
      var filterString = $"tags/any(c:c eq 'ThreescaleServiceId:{serviceId}')";

      using IServiceScope serviceScope = injectedProvider.CreateScope();
      IServiceProvider provider = serviceScope.ServiceProvider;

      var graphClient = provider.GetRequiredService<GraphServiceClient>();

      var results = await graphClient.Applications.Request()
          .Filter(filterString)
          .GetAsync();

      return results.First();
    }
  }
}
