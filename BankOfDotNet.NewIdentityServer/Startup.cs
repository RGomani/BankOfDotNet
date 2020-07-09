using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using BankOfDotNet.NewIdentityServer.Data.Migrations.IdentityServer.ConfigurationDb;
using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.EntityFramework.Mappers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace BankOfDotNet.NewIdentityServer
{   /*
	public class Startup
	{
		// This method gets called by the runtime. Use this method to add services to the container.
		// For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
		public void ConfigureServices(IServiceCollection services)
		{
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}

			app.UseRouting();

			app.UseEndpoints(endpoints =>
			{
				endpoints.MapGet("/", async context =>
				{
					await context.Response.WriteAsync("Hello World!");
				});
			});
		}
	}  
    */

	public class Startup
	{
		// This method gets called by the runtime. Use this method to add services to the container.
		// For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
		public void ConfigureServices(IServiceCollection services)
		{
			// Added when we add the IS4 UI 
			services.AddMvc(option => option.EnableEndpointRouting = false);


			//AddConfogForReadAppSetings
			var appConfig = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appSettings.Json", false).Build();
			string connectionString = appConfig.GetSection("ConnectionString").Value;
			var migrationsAssembly = typeof(Startup).GetTypeInfo().Assembly.GetName().Name;

			// Plug-in our identity server middleware
			services.AddIdentityServer()
				// We are not going to use natural certificate to sign-in tokens and everything
				// The idea is to use developer mode sign-in credentials
				.AddDeveloperSigningCredential()
				// We have Users (Human users) need to use Clients (PC, Phone) and those Clients have access
				// to resources.  Our resource in our case is the BankOfDotNet.API and our client is one is 
				// Postman and the other is console app.
				// We will be setting up a config file as parameter
				//.AddInMemoryApiResources(Config.GetAllApiResources())
				// We have clients that will be registered to our Identity service and those clients
				// are managed by IdentiyServer4 and have permissions to access some resources.
				//.AddInMemoryClients(Config.GetClients())
				// Add the in-memory test users for testing and to be used
				// For the GrantTypes.ResourceOwnerPassword grant types in the BankOfDotNet.ConsoleResourceOwner project
				//.AddTestUsers(Config.GetTestUsers())
				// Add the Open-ID Connect Identity scope
				//.AddInMemoryIdentityResources(Config.GetidentityResources());

				//configureation Store:clients and resources
				.AddConfigurationStore(options =>
				{ 


				options.ConfigureDbContext = builder =>
											 builder.UseSqlServer(connectionString,
													sql => sql.MigrationsAssembly(migrationsAssembly));
				})

				//Operational Store: Tokens,consents,codes,etc ..
				.AddOperationalStore(options=> {
					options.ConfigureDbContext = builder =>
												 builder.UseSqlServer(connectionString,
														sql => sql.MigrationsAssembly(migrationsAssembly));
				});
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
			InitializedIdentityServerDataBase(app); 

			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}

			// We are going to plug-in the pipeline and use the identity service
			app.UseIdentityServer();

			// We are going to allow for static files for the wwwroot (css, js, lib, html)
			app.UseStaticFiles();

			// Use MVC with default route
			app.UseMvcWithDefaultRoute();

			//app.UseEndpoints(endpoints =>
			//{
			//	endpoints.MapGet("/", async context =>
			//	{
			//		await context.Response.WriteAsync("Hello World!");
			//	});
			//});
		}

		private void InitializedIdentityServerDataBase(IApplicationBuilder app)
		{
			using (var serviceScope = app.ApplicationServices.GetService<IServiceScopeFactory>().CreateScope())
			{
				serviceScope.ServiceProvider.GetRequiredService<PersistedGrantDbContext>().Database.Migrate();
				var context = serviceScope.ServiceProvider.GetRequiredService<ConfigurationDbContext>();
				context.Database.Migrate();

				//seed the Data
				//seed clients
				if (!context.Clients.Any())
				{
				foreach(var client in Config.GetClients())
					{
						context.Clients.Add(client.ToEntity());
					}
					context.SaveChanges();
				}
				//seed Identity Resourses

				if (!context.IdentityResources.Any())
				{
					foreach (var resources in Config.GetidentityResources())
					{
						context.IdentityResources.Add(resources.ToEntity());
					}
					context.SaveChanges();
				}

				//Seed ApiResources
				if (!context.ApiResources.Any())
				{
					foreach (var resources in Config.GetAllApiResources())
					{
						context.ApiResources.Add(resources.ToEntity());
					}
					context.SaveChanges();
				}

			}
		}
	}
}
