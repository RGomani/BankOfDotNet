using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using BankOfDotNet.NewAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace BankOfDotNet.NewAPI
{
	public class Startup
	{
		public Startup(IConfiguration configuration)
		{
			Configuration = configuration;
		}

		public IConfiguration Configuration { get; }

		// This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices(IServiceCollection services)
		{
			services.AddControllers();

			// Set-up the plumming for our Identity server
			// We use "Bearer" which is the same output from Postman "token_type": "Bearer"
			// when we call http://localhost:2000/connect/token to grab a token
			services.AddAuthentication("Bearer")
				.AddIdentityServerAuthentication(options =>
				{
					// This is the BankOfDotNet.IdentityServer which is running on port 5000
					options.Authority = "http://localhost:2000";
					options.RequireHttpsMetadata = false;
					options.ApiName = "BankOfDotNetNewAPI";
				});
			// Add a reference to the BankContext and use in-memory database
			services.AddDbContext<BankContext>(options =>
				options.UseInMemoryDatabase("BankingDb"));

			// If we need to use Sqlite
			//services.AddDbContext<BankContext>(opts =>
			//    opts.UseSqlite(Configuration.GetConnectionString("Users")));

			services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
			 services.AddSwaggerGen(options => {

				 options.SwaggerDoc("v1",new OpenApiInfo { Title = "BankOfDotNetNewAPI", Version = "V1" });
				 options.OperationFilter<ChekAuthorizeOperationFilter>();
				 options.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
				 {
					 Type = SecuritySchemeType.OAuth2,
					 Flows = new OpenApiOAuthFlows
					 {
						 Implicit = new OpenApiOAuthFlow
						 {
							 AuthorizationUrl = new Uri("http://localhost:2000/connect/authorize"),
							 TokenUrl=new Uri("http://localhost:2000/connect/token"),
							 Scopes = new Dictionary<string, string>
 							{
 								{ "BankOfDotNetNewAPI", "BankOfDotNet New API" },
							 }
						}
					 }
				 });

			 });
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}

			app.UseRouting();

			app.UseAuthorization();

			app.UseEndpoints(endpoints =>
			{
				endpoints.MapControllers();
			});
			app.UseSwagger();
			app.UseSwaggerUI(options=> {

				options.SwaggerEndpoint("/swagger/v1/swagger.json","BankOfDotNet Api V1");
				options.OAuthClientId ("swaggerapiui");
				options.OAuthAppName("Awagger Api Ui");
			});
		}
	}

	internal class ChekAuthorizeOperationFilter : IOperationFilter
	{

    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var hasAuthorize = 
          context.MethodInfo.DeclaringType.GetCustomAttributes(true).OfType<AuthorizeAttribute>().Any() 
          || context.MethodInfo.GetCustomAttributes(true).OfType<AuthorizeAttribute>().Any();

        if (hasAuthorize)
        {
            operation.Responses.Add("401", new OpenApiResponse { Description = "Unauthorized" });
            operation.Responses.Add("403", new OpenApiResponse { Description = "Forbidden" });

            operation.Security = new List<OpenApiSecurityRequirement>
            {
                new OpenApiSecurityRequirement
                {
                    [
                        new OpenApiSecurityScheme {Reference = new OpenApiReference 
                        {
                            Type = ReferenceType.SecurityScheme, 
                            Id = "oauth2"}
                        }
                    ] = new[] { "BankOfDotNetNewAPI" }
                }
            };

        }
    }
	}
}
