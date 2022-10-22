using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using slowpoke.core.Models.Node.Docs;
using SlowPokeIMS.Web.Controllers.Attributes;

namespace SlowPokeIMS.Web.ViewModels;

public class DocAction
{
    public string Text { get; set; }

    public string Tooltip { get; set; }

    public bool Enabled { get; set; } = true;
    
    public string Href { get; set; }
    
    public Func<string, string> HrefFn { get; set; }
    
    public bool InlineAction { get; set; }
}

public class DocActionsList
{
    public List<DocAction> Items { get; set; }
}

public static class DocActionExtensions
{
    public static DocActionsList GetActions(this IReadOnlyNode node, IUrlHelper url, IServiceProvider services)
    {
        if (node is IReadOnlyDocument doc)
        {
            return doc.GetActions(url, services);
        }
        else if (node is IReadOnlyFolder folder)
        {
            return folder.GetActions(url, services);
        }
        else
        {
            throw new NotSupportedException();
        }
    }

    public static DocActionsList GetActions(this IReadOnlyDocument doc, IUrlHelper url, IServiceProvider services)
    {
        return GetActions(doc, url, services, doc.ContentType);
    }

    public static DocActionsList GetActions(this IReadOnlyFolder doc, IUrlHelper url, IServiceProvider services)
    {
        return GetActions(doc, url, services, string.Empty);
    }

    private static DocActionsList GetActions(IReadOnlyNode doc, IUrlHelper url, IServiceProvider services, string contentType)
    {
        var controllerTypesForContentType = Assembly.GetExecutingAssembly().GetTypes()
                    .Where(t => t.IsSubclassOf(typeof(Controller)) &&
                                (t.GetCustomAttribute<DocControllerAttribute>()?.IsContentTypeMatch(contentType) ?? false));

        var actionMethods = controllerTypesForContentType.SelectMany(c => c.GetMethods()
                                .Select(m => (m, m.GetCustomAttribute<DocActionAttribute>(), 
                                                 m.GetCustomAttribute<DocActionContentTypeAttribute>(),
                                                 m.GetCustomAttribute<DocActionLabelAttribute>(),
                                                 m.GetCustomAttribute<DocActionVisibilityAttribute>())))
                                .Where(m => m.Item2 != null)
                                .Where(m => m.Item3?.IsContentTypeMatch(contentType) ?? true)
                                .Where(m => m.Item5?.VisibleFn?.Invoke(doc, services) ?? true);

        var actions = actionMethods.Select(m =>
        {
            var attr = m.Item2;
            var controllerName = m.m.DeclaringType.Name.Substring(0, m.m.DeclaringType.Name.Length - "Controller".Length);
            return new DocAction { Text = m.Item4?.Label ?? m.m.Name, Href = url.Action(m.m.Name, controllerName, new { path = doc.Path.PathValue }), InlineAction = attr?.InlineAction ?? false };
        }).ToList();
        return new DocActionsList { Items = actions };
    }
}