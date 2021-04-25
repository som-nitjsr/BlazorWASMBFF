using Microsoft.AspNetCore.Hosting;

[assembly: HostingStartup(typeof(BlazorWASMBFF.Server.Areas.Identity.IdentityHostingStartup))]
namespace BlazorWASMBFF.Server.Areas.Identity
{
    public class IdentityHostingStartup : IHostingStartup
    {
        public void Configure(IWebHostBuilder builder)
        {
            builder.ConfigureServices((context, services) => {
            });
        }
    }
}