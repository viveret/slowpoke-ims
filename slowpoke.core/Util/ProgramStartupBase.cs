using System.Reflection;
using GraphQL;
using GraphQL.DI;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc.Razor.RuntimeCompilation;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using slowpoke.core.Extensions;
using slowpoke.core.Models;
using slowpoke.core.Models.Configuration;
using slowpoke.core.Models.GraphQL;
using slowpoke.core.Models.Node;
using slowpoke.core.Services;
using slowpoke.core.Services.Broadcast;
using slowpoke.core.Services.Http;
using slowpoke.core.Services.Identity;
using slowpoke.core.Services.Node;
using slowpoke.core.Services.Node.Docs;
using slowpoke.core.Services.Scheduled;
using slowpoke.core.Services.Scheduled.Tasks;
using SlowPokeIMS.Core.Services;

namespace SlowPokeIMS.Core.Util;


public abstract class ProgramStartupBase
{
    public ProgramStartupBase(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    public abstract bool UseSession { get; }
    public abstract bool UseMvc { get; }
    public abstract bool UseRazorRuntimeCompilation { get; }
    public abstract bool UseGraphQl { get; }

    // This method gets called by the runtime. Use this method to add services to the container.
    public void ConfigureSlowPokeServices(IServiceCollection services)
    {
        services.AddDistributedMemoryCache();
        ConfigureServing(services);

        AddContextAndEnvProviders(services);
        
        AddConfig(services);

        AddAuthIdentityService(services);
        services.AddTransient<IIpAddressHistoryService, IpAddressHistoryService>();

        AddBroadcastProviders(services);
        AddDocumentResolvers(services);
        AddDocumentResolverProvider(services);
        AddHostProviders(services);

        services.AddTransient<ISyncStateManager, SyncStateManager>();

        AddBroadcastHandlers(services);
        AddScheduledTasks(services);

        if (UseGraphQl)
        {
            services.AddGraphQL(ConfigureGraphQlService);
        }
        
        if (UseSession)
        {
            services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromSeconds(10);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });
        }

        if (UseMvc)
        {
            this.ConfigureMvcServices(services.AddMvc(), UseRazorRuntimeCompilation);
        }
        
        services.AddHttpContextAccessor();
        services.AddSingleton<IAssemblyAccessor, AssemblyAccessor>();
    }

    protected virtual void AddAuthIdentityService(IServiceCollection services)
    {
        services.AddTransient<IIdentityAuthenticationService, LocalIdentityAuthenticationService>();
    }

    protected virtual void ConfigureGraphQlService(IGraphQLBuilder cfg)
    {
        cfg.ConfigureExecutionOptions(opt =>
            {
            })
            .AddSchema<ApiSchema>()
            .UseMemoryCache()
            .AddSystemTextJson()
            .AddGraphTypes(typeof(ApiSchema).Assembly)
            .AddErrorInfoProvider(opt => opt.ExposeExceptionDetails = true);
    }

    protected virtual void ConfigureMvcServices(IMvcBuilder mvc, bool useRazorRuntimeCompilation)
    {
        mvc.AddControllersAsServices();
        
        if (useRazorRuntimeCompilation)
        {
            mvc.AddRazorRuntimeCompilation(ConfigureRazorRuntimeCompilation);
        }
    }

    protected virtual void ConfigureRazorRuntimeCompilation(MvcRazorRuntimeCompilationOptions options)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var libraryPath = System.IO.Path.GetFullPath(assembly.GetName().Name!);
        if (Directory.Exists(libraryPath))
        {
            options.FileProviders.Add(new PhysicalFileProvider(libraryPath));
        }
        else if (File.Exists(libraryPath))
        {
            options.FileProviders.Add(new EmbeddedFileProvider(assembly));
        }
    }

    protected virtual void ConfigureServing(IServiceCollection services)
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
    }

    protected virtual void AddContextAndEnvProviders(IServiceCollection services)
    {
        services.AddTransient<IEnvironment, EnvironmentProvider>();
        services.AddTransient<IServingInfoProvider, ServingInfoProvider>();
    }

    protected virtual void AddConfig(IServiceCollection services)
    {
        services.AddTransient<Config.SystemConfig>();
        services.AddTransient<Config.PathsConfig>();
        services.AddTransient<Config.StyleConfig>();
        services.AddTransient<Config.P2PConfig>();
        services.AddTransient<Config>();
    }

    protected virtual void AddBroadcastProviders(IServiceCollection services)
    {
        services.AddTransient<IBroadcastProvider, InMemoryBroadcastProvider>();
        services.AddTransient<IBroadcastProvider, HttpBroadcastProvider>();
        services.AddTransient<IBroadcastProviderResolver, BroadcastProviderResolver>();
    }

    protected virtual void AddDocumentResolvers(IServiceCollection services)
    {
        services.AddTransient<IReadOnlyDocumentResolver, ReadOnlyLocalDocumentResolver>();
        services.AddTransient<IWritableDocumentResolver, WritableLocalDocumentResolver>();
    }

    protected virtual void AddDocumentResolverProvider(IServiceCollection services)
    {
        services.AddTransient<IDocumentProviderResolver, LocalDocumentProviderResolver>();
    }

    protected virtual void AddHostProviders(IServiceCollection services)
    {
        services.AddTransient<ISlowPokeHost, LocalSlowPokeHost>();
        services.AddTransient<ISlowPokeHostProvider, LocalSlowPokeHostProvider>();
    }

    protected virtual void AddBroadcastHandlers(IServiceCollection services)
    {
        services.AddTransient<IBroadcastMessageHandler, DocumentBroadcastMessageHandler>();
        services.AddTransient<IBroadcastMessageHandlerResolver, BroadcastMessageHandlerResolver>();
    }

    protected virtual void AddScheduledTasks(IServiceCollection services)
    {
        services.AddSingleton<ScheduledTaskContextStorage>();
        services.AddTransient<IScheduledTask, SendAndReceiveBroadcastMessagesScheduledTask>();
        services.AddTransient<IScheduledTask, ScanLocalNetworkForPeersScheduledTask>();
        services.AddTransient<IScheduledTask, ScanLocalAndPublishChangesScheduledTask>();
        services.AddTransient<IScheduledTask, ScanLocalAndPullChangesScheduledTask>();
        services.AddTransient<IScheduledTask, ProcessBroadcastMessagesScheduledTask>();
        services.AddTransient<IScheduledTaskManager, ScheduledTaskManager>();
        services.AddHostedService<ScheduledTaskRunnerHostedService>();
    }
}