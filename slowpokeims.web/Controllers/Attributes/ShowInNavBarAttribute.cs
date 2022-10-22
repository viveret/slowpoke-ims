using System;

namespace SlowPokeIMS.Web.Controllers.Attributes;

[AttributeUsage(System.AttributeTargets.Method | System.AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
public sealed class ShowInNavBarAttribute : System.Attribute
{
    public string Title { get; }
    
    public string Tooltip { get; }
    
    public bool ShowInHorizontal { get; }

    public string CustomViewName { get; }
    
    public bool UseCustomView => !string.IsNullOrEmpty(CustomViewName);

    public ShowInNavBarAttribute(string title = null, string tooltip = null, bool showInHorizontal = true, string customView = null)
    {
        Title = title;
        Tooltip = tooltip;
        ShowInHorizontal = showInHorizontal;
        CustomViewName = customView;
    }
}