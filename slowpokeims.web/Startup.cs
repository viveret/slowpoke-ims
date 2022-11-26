using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using GraphiQl;
using SlowPokeIMS.Core.Util;


namespace SlowPokeIMS.Web;


public class Startup: ProgramStartupBase
{
    public override bool UseSession => true;

    public override bool UseMvc => true;

    public override bool UseRazorRuntimeCompilation => true;

    public override bool UseGraphQl => true;

    public Startup(IConfiguration configuration): base(configuration)
    {
    }

    // This method gets called by the runtime. Use this method to add services to the container.
    public virtual void ConfigureServices(IServiceCollection services)
    {
        ConfigureSlowPokeServices(services);
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public virtual void Configure(IApplicationBuilder app, IWebHostEnvironment env)
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