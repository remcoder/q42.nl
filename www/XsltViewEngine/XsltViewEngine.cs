using System.Web.Mvc;
using System.Collections.Generic;
using System.Diagnostics;

namespace Q42.Mvc.XsltViewEngine
{
  /// <summary>
  /// View engine for using XSLT in MVC.
  /// </summary>
  public class XsltViewEngine :  VirtualPathProviderViewEngine
  {
    public XsltViewEngine()
    {
      ViewLocationFormats = PartialViewLocationFormats = new[]
      {
        "~/Views/{1}/{0}.xslt",
        "~/Views/Shared/{0}.xslt",
      };
    }

    private Dictionary<string, PluginConstructor> pluginConstructors = new Dictionary<string,PluginConstructor>();

    public void AddExtension(string namespaceUri, PluginConstructor constructor)
    {
      pluginConstructors[namespaceUri] = constructor;
    }

    protected override IView CreatePartialView(ControllerContext controllerContext, string partialPath)
    {
      return new XsltView(controllerContext, partialPath, pluginConstructors);
    }

    protected override IView CreateView(ControllerContext controllerContext, string viewPath, string masterPath)
    {
      return new XsltView(controllerContext, viewPath, masterPath, pluginConstructors);
    }
  }
}
