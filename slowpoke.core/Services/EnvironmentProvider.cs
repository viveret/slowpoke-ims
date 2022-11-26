using System.IO;
using Microsoft.AspNetCore.Hosting;
using slowpoke.core.Models;

namespace SlowPokeIMS.Core.Services;



public class EnvironmentProvider : IEnvironment
{
    private readonly IWebHostEnvironment env;

    public EnvironmentProvider(IWebHostEnvironment env)
    {
        this.env = env;
    }

    public string AppRootPath => env.ContentRootPath;

    public string ContentRootPath => env.WebRootPath ?? (Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"));
}