using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using slowpoke.core;
using slowpoke.core.Models.Config;
using SlowPokeIMS.Web.Helpers.Services;
using slowpoke.core.Models;
using GraphiQl;
using GraphQL;
using slowpoke.core.Models.GraphQL;
using slowpoke.core.Services;
using Microsoft.Extensions.FileProviders;
using System.Reflection;
using slowpoke.core.Services.Node.Docs;
using slowpoke.core.Models.Node;
using slowpoke.core.Services.Node;
using SlowPokeIMS.Web.Services;
using slowpoke.core.Services.Broadcast;
using slowpoke.core.Services.Scheduled;
using slowpoke.core.Services.Scheduled.Tasks;
using slowpoke.core.Services.Identity;
using slowpoke.core.Services.Http;

namespace SlowPokeIMS.Web
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
            services.Configure<KestrelServerOptions>(options =>
            {
                options.AllowSynchronousIO = true;
            });
            services.Configure<IISServerOptions>(options =>
            {
                options.AllowSynchronousIO = true;
            });
            services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders =
                    ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
            });

            services.AddTransient<IEnvironment, EnvironmentProvider>();
            services.AddTransient<IServingInfoProvider, ServingInfoProvider>();
            
            services.AddTransient<Config.SystemConfig>();
            services.AddTransient<Config.PathsConfig>();
            services.AddTransient<Config.P2PConfig>();
            services.AddTransient<Config>();

            services.AddTransient<IIdentityAuthenticationService, IdentityAuthenticationService>();
            services.AddTransient<IIpAddressHistoryService, IpAddressHistoryService>();

            services.AddTransient<IBroadcastProvider, InMemoryBroadcastProvider>();
            services.AddTransient<IBroadcastProvider, HttpBroadcastProvider>();
            services.AddTransient<IBroadcastProviderResolver, BroadcastProviderResolver>();
            services.AddTransient<IUserSpecialFoldersProvider, UserSpecialFoldersProvider>();

            services.AddTransient<IReadOnlyDocumentResolver, ReadOnlyLocalDocumentResolver>();
            services.AddTransient<IWritableDocumentResolver, WritableLocalDocumentResolver>();
            services.AddTransient<IDocumentProviderResolver, LocalDocumentProviderResolver>();
            services.AddTransient<ISlowPokeHost, LocalSlowPokeHost>();
            services.AddTransient<ISlowPokeHostProvider, LocalSlowPokeHostProvider>();

            services.AddTransient<ISyncStateManager, SyncStateManager>();

            services.AddTransient<IBroadcastMessageHandler, DocumentBroadcastMessageHandler>();
            services.AddTransient<IBroadcastMessageHandlerResolver, BroadcastMessageHandlerResolver>();

            services.AddTransient<IScheduledTask, SendAndReceiveBroadcastMessagesScheduledTask>();
            services.AddTransient<IScheduledTask, ScanLocalNetworkForPeersScheduledTask>();
            services.AddTransient<IScheduledTask, ScanLocalAndPublishChangesScheduledTask>();
            services.AddTransient<IScheduledTask, ProcessBroadcastMessagesScheduledTask>();
            services.AddTransient<IScheduledTaskManager, ScheduledTaskManager>();
            services.AddHostedService<ScheduledTaskRunnerHostedService>();

            services.AddGraphQL(cfg => cfg
                .ConfigureExecutionOptions(opt =>
                {
                })
                .AddSchema<ApiSchema>()
                .UseMemoryCache()
                .AddSystemTextJson()
                .AddGraphTypes(typeof(ApiSchema).Assembly)
                .AddErrorInfoProvider(opt => opt.ExposeExceptionDetails = true));
            
            services.AddDistributedMemoryCache();
            services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromSeconds(10);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            var mvc = services.AddMvc().AddControllersAsServices().AddRazorRuntimeCompilation(options =>
            {
                var assembly = Assembly.GetExecutingAssembly();
                var libraryPath = System.IO.Path.GetFullPath(assembly.GetName().Name);
                options.FileProviders.Add(new PhysicalFileProvider(libraryPath));
            });

            services.AddHttpContextAccessor();
            services.AddSingleton<IAssemblyAccessor, AssemblyAccessor>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.EnvironmentName == "Development")
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseGraphQL("/api/graphql");
            app.UseGraphiQl("/graphiql", "/api/graphql");
            app.UseRouting();
            app.UseEndpoints(cfg =>
            {
                cfg.MapDefaultControllerRoute();
            });
            
            app.UseSession();
        }
    }
}