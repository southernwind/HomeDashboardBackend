
using System;

using Back.Hubs;
using Back.Models.Aquarium;
using Back.Models.Common;
using Back.Models.ElectricPower;
using Back.Models.Financial;
using Back.Models.HealthCheck;
using Back.Models.Kitchen;
using Back.Models.Network;
using Back.Models.Palmie;
using Back.States;
using Back.States.Monitors.Aquarium;

using DataBase;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Back {
	public class Startup {
		private readonly string _crossOriginPolicyName = "anyOrigin";
		public IConfiguration Configuration {
			get;
		}


		public Startup(IWebHostEnvironment env) {
			var builder = new ConfigurationBuilder()
				.SetBasePath(env.ContentRootPath)
				.AddJsonFile("appsettings.json", false, true)
				.AddJsonFile($"appsettings.{env.EnvironmentName}.json", true, true)
				.AddEnvironmentVariables();
			this.Configuration = builder.Build();

			Console.WriteLine("設定値");
			foreach (var section in this.Configuration.GetChildren()) {
				Console.WriteLine($"{section.Path}:{section.Value}");
			}
		}

		// This method gets called by the runtime. Use this method to add services to the container.
		// For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
		public void ConfigureServices(IServiceCollection services) {
			// クロスドメインの許可
			services.AddCors(options => {
				options.AddPolicy(this._crossOriginPolicyName, builder => {
					builder.AllowCredentials();
					builder.WithOrigins(this.Configuration.GetSection("FrontOrigin").Value!);
					builder.AllowAnyHeader();
					builder.AllowAnyMethod();
				});
			});

			services.AddMvc();
			services.AddSignalR();
			services.AddDbContext<HomeServerDbContext>(optionsBuilder => {
				var connectionString = this.Configuration.GetConnectionString("Database");
				optionsBuilder.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
			});
			services.AddLogging(builder => {
				builder.AddConfiguration(this.Configuration.GetSection("Logging"))
					.AddConsole()
					.AddDebug();
			});
			services.AddHttpClient();

			services.AddSingleton<Updater>();
			services.AddSingleton<Store>();
			services.AddSingleton<Monitor>();
			services.AddTransient<AquariumMonitor>();
			services.AddTransient<FinancialModel>();
			services.AddTransient<ElectricPowerModel>();
			services.AddTransient<NetworkModel>();
			services.AddTransient<KitchenModel>();
			services.AddTransient<AquariumModel>();
			services.AddTransient<PalmieModel>();
			services.AddTransient<HealthCheckModel>();
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IWebHostEnvironment env) {
			if (env.IsDevelopment()) {
				app.UseDeveloperExceptionPage();
			}

			app.UseRouting();

			app.UseCors(this._crossOriginPolicyName);

			app.UseEndpoints(routes => {
				routes.MapHub<DashboardHub>("api/hubs/dashboard-hub");
			});

			app.UseEndpoints(routes => {
				routes.MapControllers();
			});

		}
	}
}
