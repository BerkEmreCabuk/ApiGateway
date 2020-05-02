using System;
using System.Text;
using ApiGateway.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Serilog;

namespace ApiGateway
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
            services.AddOcelot(Configuration);
            var appSettingsSection = Configuration.GetSection("Identity");
            services.Configure<AppSettingsModel>(appSettingsSection);
            var appSettings = appSettingsSection.Get<AppSettingsModel>();
            var signingKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(appSettings.SecretKey));
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = signingKey,
                ValidateIssuer = true,
                ValidIssuer = appSettings.Iss,
                ValidateAudience = true,
                ValidAudience = appSettings.Aud,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero,
                RequireExpirationTime = true,
            };

            services.AddAuthentication()
            .AddJwtBearer("TestKey", x =>
              {
                  x.RequireHttpsMetadata = false;
                  x.TokenValidationParameters = tokenValidationParameters;
              });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public async void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
            app.UseSerilogRequestLogging();
            await app.UseOcelot();
        }
    }
}
