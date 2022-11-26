using System.Reflection;
using System.Text.Json;
using slowpoke.core.Models.Configuration.Attributes;

namespace slowpoke.core.Models.Configuration;


public partial class Config
{
    public Config(IServiceProvider services)
    {
        foreach (var property in this.GetType().GetProperties())
        {
            property.SetValue(this, services.GetService(property.PropertyType)
                ?? throw new Exception($"Could not get required config {property.PropertyType}"));
        }
        SetDefaults();
        Load(CancellationToken.None);
    }
    
    // Empty constructor for deserialization
    public Config()
    {
        foreach (var property in this.GetType().GetProperties())
        {
            property.SetValue(this, Activator.CreateInstance(property.PropertyType));
        }
        SetDefaults();
    }

    public void SetDefaults()
    {
        // get all properties using DefaultAttribute
        foreach (var property in this.GetType().GetProperties())
        {
            foreach (var field in property.PropertyType.GetProperties())
            {
                if (property.GetMethod!.IsStatic)
                    continue;

                var attr = field.GetCustomAttribute<DefaultAttribute>(true);
                if (attr == null)
                    continue;
                
                field.SetValue(property.GetValue(this), attr.DefaultValue);
            }
        }
    }

    public void Save(CancellationToken cancellationToken)
    {
        File.WriteAllText(GetConfigPath(), JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true }));
    }

    public void Load(CancellationToken cancellationToken)
    {
        var path = GetConfigPath();
        if (!File.Exists(path))
            return;

        var json = File.ReadAllText(path);
        var val = JsonSerializer.Deserialize<Config>(json, new JsonSerializerOptions { });
        foreach (var property in this.GetType().GetProperties())
        {
            var loadedSection = property.GetValue(val);
            foreach (var field in property.PropertyType.GetProperties())
            {
                if (!field.CanWrite)
                    continue;

                field.SetValue(property.GetValue(this), field.GetValue(loadedSection));
            }
        }
    }

    public string GetConfigPath()
    {
        return Path.Combine(Paths.HomePath, ".slowpokeims-config.json");
    }
}