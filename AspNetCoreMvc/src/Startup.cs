﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using C4D.AspNetCore.Tutorial.Models;
using C4D.AspNetCore.Tutorial.Services;
using StackExchange.Redis;
using Microsoft.AspNetCore.DataProtection;

namespace C4D.AspNetCore.Tutorial
{
	public class Startup
	{
		public Startup(IHostingEnvironment env)
		{
			var builder = new ConfigurationBuilder()
				.SetBasePath(env.ContentRootPath)
				.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
				.AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true);

			if (env.IsDevelopment())
			{
				// For more details on using the user secret store see https://go.microsoft.com/fwlink/?LinkID=532709
				builder.AddUserSecrets<Startup>();
			}

			builder.AddEnvironmentVariables();
			Configuration = builder.Build();
		}

		public IConfigurationRoot Configuration { get; }

		// This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices(IServiceCollection services)
		{
			//This is a hack to overcome the issue with suporting connection based on host name for :nix systems
			var redisHost = Configuration.GetValue<string>("Redis:Host");
			var redisPort = Configuration.GetValue<int>("Redis:Port");
			var redisIp = System.Net.Dns.GetHostEntryAsync(redisHost).Result.AddressList.Last();
			var redis = ConnectionMultiplexer.Connect($"{redisIp}:{redisPort}");

			services.AddDataProtection().PersistKeysToRedis(redis, "DataProtection-Keys");

			services.AddIdentityWithMongoStores(Configuration.GetConnectionString("DefaultConnection"));

			services.AddMvc();

			// Add application services.
			services.AddTransient<IEmailSender, AuthMessageSender>();
			services.AddTransient<ISmsSender, AuthMessageSender>();
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
		{
			loggerFactory.AddConsole(Configuration.GetSection("Logging"));
			loggerFactory.AddDebug();

			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
				app.UseBrowserLink();
			}
			else
			{
				app.UseExceptionHandler("/Home/Error");
			}

			app.UseStaticFiles();

			// Add external authentication middleware below. To configure them please see https://go.microsoft.com/fwlink/?LinkID=532715
			app.UseIdentity();

			app.UseMvc(routes =>
			{
				routes.MapRoute(
					name: "default",
					template: "{controller=Home}/{action=Index}/{id?}");
			});
		}
	}
}
