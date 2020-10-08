using SmartPlus.API.Helpers;
using SmartPlus.API.PipelineBehaviors;
using SmartPlus.Data.IRepositories;
using SmartPlus.Data.Repositories;
using SmartPlus.Model;
using SmartPlus.Domain.Dxos;
using SmartPlus.Service.Services.Helpers;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using SmartPlus.Model.Models;

namespace SmartPlus.API.App_Start
{
    public static class Dependencies_Start
    {

        /// <summary>
        /// Resolve all the dependencies in the application
        /// </summary>
        /// <param name="services"></param>
        /// <param name="Configuration"></param>
        public static void ResolveDepenciesServices(this IServiceCollection services, IConfiguration Configuration)
        {
            // //Sql server 
            // //master database
            // services.AddDbContext<SmartPlus_Master_DbContext>(options =>
            // {
            //     options.UseSqlServer(Configuration.GetConnectionString("MasterConnection"),
            //         sqlOptions =>
            //         {
            //             sqlOptions.EnableRetryOnFailure(maxRetryCount: 3, maxRetryDelay: TimeSpan.FromSeconds(30),
            //                 errorNumbersToAdd: null);
            //         });
            // });

            //Add connection string based on the tenant. Pick the tenantid in the header
            services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            services.AddDbContext<SmartPlusDbContext>((serviceProvider, options) =>
            {
                var httpContext = serviceProvider.GetService<IHttpContextAccessor>().HttpContext;
                var httpRequest = httpContext.Request;
                var connection = GetConnection(httpRequest, Configuration);
                options.UseSqlServer(connection, sqlOptions =>
                {
                    sqlOptions.EnableRetryOnFailure(maxRetryCount: 3, maxRetryDelay: TimeSpan.FromSeconds(30),
                        errorNumbersToAdd: null);
                });
            });




            //Add DIs

            services.AddScoped<IAstuceRepository, AstuceRepository>();
            services.AddScoped<IAstuceDxos, AstuceDxos>();

            //User
            services.AddScoped<ITokenHelper, TokenHelper>();

            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IUserDxos, UserDxos>();

            services.AddScoped<IRoleRepository, RoleRepository>();
            services.AddScoped<IRoleDxos, RoleDxos>();
            
            

            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        }

        private static string GetConnection(HttpRequest httpRequest, IConfiguration Configuration)
        {
            string tenantId = httpRequest.Headers["TenantId"].ToString();

            Console.WriteLine($"TenantId: {tenantId}");

            if (!string.IsNullOrWhiteSpace(tenantId))
            {
                return Configuration.GetConnectionString("DefaultConnection").Replace("{TenantId}", tenantId);
            }
            else
            {
                return Configuration.GetConnectionString("DefaultConnection").Replace("_{TenantId}", "");
            }


        }
    }
}
