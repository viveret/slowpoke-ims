using System.Reflection;
using slowpoke.core.Models.Config.Attributes;

namespace slowpoke.core.Models.Config;


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
    }

    public void SetDefaults()
    {
        // get all properties using DefaultAttribute
        foreach (var property in this.GetType().GetProperties())
        {
            foreach (var field in property.PropertyType.GetProperties())
            {
                var attr = field.GetCustomAttribute<DefaultAttribute>(true);
                if (attr == null)
                    continue;
                
                field.SetValue(property.GetValue(this), attr.DefaultValue);
            }
        }
    }
}