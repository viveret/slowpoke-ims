using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Razor.RuntimeCompilation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using slowpoke.core.Models.Node;
using slowpoke.core.Services.Identity;
using slowpoke.core.Services.Node;
using slowpoke.core.Services.Node.Docs;
using SlowPokeIMS.Core.Services.Node.Docs;
using SlowPokeIMS.Core.Services.Node.Docs.ReadOnly;
using SlowPokeIMS.Core.Services.Node.Docs.ReadWrite;
using SlowPokeIMS.Core.Util;
using SlowPokeIMS.Tests.Core.Services;
using SlowPokeIMS.Web.Controllers.Api;

namespace SlowPokeIMS.Integration.Tests.Core
{
    public class TestStartup: ProgramStartupBase
    {
        public override bool UseSession => true;

        public override bool UseMvc => true;

        public override bool UseRazorRuntimeCompilation => false;

        public override bool UseGraphQl => false;

        public TestStartup(IConfiguration configuration): base(configuration)
        {
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            ConfigureSlowPokeServices(services);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseRouting();
            app.UseEndpoints(cfg =>
            {
                cfg.MapDefaultControllerRoute();
            });
            app.UseSession();
        }
        
        protected override void ConfigureMvcServices(IMvcBuilder mvc, bool useRazorRuntimeCompilation)
        {
            mvc.Services.AddControllers().PartManager.ApplicationParts.Add(new AssemblyPart(typeof(ApiController).Assembly));
            base.ConfigureMvcServices(mvc, useRazorRuntimeCompilation);
        }

        protected override void ConfigureRazorRuntimeCompilation(MvcRazorRuntimeCompilationOptions options)
        {
            options.FileProviders.Add(new EmbeddedFileProvider(typeof(ApiController).Assembly));
            base.ConfigureRazorRuntimeCompilation(options);
        }

        protected override void AddDocumentResolvers(IServiceCollection services)
        {
            services.AddSingleton<InMemoryGenericDocumentRepository>();
            services.AddTransient<IReadOnlyDocumentResolver, GenericReadOnlyDocumentResolver>();
            services.AddTransient<IWritableDocumentResolver, StubWritableDocumentResolver>();
        }

        protected override void AddDocumentResolverProvider(IServiceCollection services)
        {
            services.AddTransient<IDocumentProviderResolver, TestDocumentProviderResolver>();
        }

        protected override void AddAuthIdentityService(IServiceCollection services)
        {
            services.AddTransient<IIdentityAuthenticationService, TestIdentityAuthenticationService>();
        }

        protected override void AddHostProviders(IServiceCollection services)
        {
            services.AddTransient<ISlowPokeHost, LocalSlowPokeHost>();
            services.AddTransient<ISlowPokeHostProvider, TestSlowPokeHostProvider>();
        }
    }
}