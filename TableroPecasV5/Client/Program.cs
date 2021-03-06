using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Blazorise;
using Blazorise.Bootstrap;
using Blazorise.Icons.FontAwesome;

namespace TableroPecasV5.Client
{
	public class Program
	{
		public static async Task Main(string[] args)
		{

			var builder = WebAssemblyHostBuilder.CreateDefault(args);

			builder.Services
				.AddBlazorise(options =>
				{
					options.ChangeTextOnKeyPress = true;
				})
				.AddBootstrapProviders()
				.AddFontAwesomeIcons();

			builder.Services.AddSingleton(new HttpClient
			{
				BaseAddress = new Uri(builder.HostEnvironment.BaseAddress)
			});

			builder.Services.AddScoped(sp => new HttpClient
			{
				BaseAddress = new Uri(builder.HostEnvironment.BaseAddress),
				Timeout = new TimeSpan(0, 15, 0)
			});

			builder.RootComponents.Add<App>("#app");

			var host = builder.Build();

			host.Services
				.UseBootstrapProviders()
				.UseFontAwesomeIcons();

			await host.RunAsync();


		}

		 }
}
