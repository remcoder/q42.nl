using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Caching;
using System.Web.Mvc;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using System.Xml.Xsl;
using System.Web.Mvc.Html;
using System.Linq;

namespace Q42.Mvc.XsltViewEngine
{
  public delegate object PluginConstructor(ViewContext viewContext, IViewDataContainer viewDataContainer);

  public class XsltView : IView, IViewDataContainer
  {
    private static string defaultViewsPath = resolvePath("~/Views");
    private XslCompiledTransform xsl = null;
    private XsltArgumentList arguments = null;
    private ViewContext viewContext = null;
    private Dictionary<string, PluginConstructor> pluginConstructors = null;

    public XsltView(ControllerContext controllerContext, string partialPath, Dictionary<string, PluginConstructor> pluginConstructors)
    {
      xsl = getXsl(partialPath);
      arguments = new XsltArgumentList();
      this.pluginConstructors = pluginConstructors;

    }

    public XsltView(ControllerContext controllerContext, string viewPath, string masterPath, Dictionary<string, PluginConstructor> pluginConstructors)
    {
      xsl = getXsl(viewPath);
      arguments = new XsltArgumentList();
      this.pluginConstructors = pluginConstructors;
    }


    #region IView Members

    public void Render(ViewContext viewContext, TextWriter writer)
    {
      this.viewContext = viewContext;

      // add plugins, model, viewdata and tempdata
      Plugin.AddAll(arguments);
      addObject("Model", viewContext.ViewData.Model);
      foreach (string key in viewContext.ViewData.Keys)
        addObject(key, viewContext.ViewData[key]);
      foreach (string key in viewContext.TempData.Select(pair => pair.Key))
        addObject(key, viewContext.TempData[key]);

      // add our html helper wrapper
      arguments.AddExtensionObject("urn:HtmlHelper", new HtmlHelperWrapper(viewContext, this));
      foreach (KeyValuePair<string, PluginConstructor> plugin in pluginConstructors)
        arguments.AddExtensionObject(plugin.Key, plugin.Value(viewContext, this));

      // set the content type based on the xsl
      HttpResponseBase response = viewContext.HttpContext.Response;
      string mediaType = getMediaType();

      // when media-type is set to an xsl file, transform to xml, otherwise to response
      #region optional xsl pipelining
      while (!String.IsNullOrEmpty(mediaType) && mediaType.Contains(".xsl"))
      {
        string transformXsl = resolvePath(mediaType);
        XmlDocument output = new XmlDocument();
        using (XmlWriter tempWriter = XmlWriter.Create(output.CreateNavigator().AppendChild()))
        {
          xsl.Transform(new XDocument().CreateReader(), arguments, tempWriter);
        }

        xsl = getXsl(transformXsl);
        mediaType = getMediaType();
      }
      #endregion

      // set the mediatype
      response.ContentType = String.IsNullOrEmpty(mediaType) ? "text/html" : mediaType;

      // transform using the correct model
      StringWriter sw = new StringWriter();
      using (XmlWriter xmlWriter = XmlWriter.Create(sw, xsl.OutputSettings))
      {
        xsl.Transform(new XDocument().CreateReader(), arguments, xmlWriter);
      }

      // clean up plugin namespaces
      string result = sw.ToString();
      result = Plugin.StripXmlNamespaces(result);
      writer.Write(result);
    }

    #endregion

    /// <summary>
    /// Returns an Xsl file
    /// </summary>
    /// <param name="fileName"></param>
    /// <returns></returns>
    private static XslCompiledTransform getXsl(string fileName)
    {
      // complete the fileName
      fileName = resolvePath(fileName);

      // check cache for an available xsl
      string cacheKey = "Q42.Mvc." + fileName;
      XslCompiledTransform xsl = HttpRuntime.Cache[cacheKey] as XslCompiledTransform;
      if (xsl == null)
      {
        xsl = new XslCompiledTransform(true);
        xsl.Load(fileName);

        // make cache dependant on filechanges within the view path
        // also try to add the current filename as it might live outside of ~/Views (defaultViewsPath);
        var viewDir = Path.GetDirectoryName(fileName);
        var viewDirs = GetDirectories(viewDir);
        var defaultViewDirs = GetDirectories(defaultViewsPath);

        var dirList = viewDirs.Concat(defaultViewDirs).DistinctBy(s => s);
        HttpRuntime.Cache.Insert(cacheKey, xsl, new CacheDependency(dirList.ToArray()));
      }
      return xsl;
    }

    private static IEnumerable<string> GetDirectories(string path)
    {
      if (!Directory.Exists(path))
        return Enumerable.Empty<string>();

      DirectoryInfo dir = new DirectoryInfo(path);
      DirectoryInfo[] dirs = dir.GetDirectories("*", SearchOption.AllDirectories);
      List<string> dirList = new List<string>();
      dirList.Add(dir.FullName);
      foreach (DirectoryInfo d in dirs) dirList.Add(d.FullName);
      return dirList;
    }

    /// <summary>
    /// Resolves a virtual path to an absolute path
    /// </summary>
    /// <param name="path">Virtual path to resolve</param>
    /// <returns></returns>
    private static string resolvePath(string path)
    {
      if (path.StartsWith("~"))
      {
        string root = HttpRuntime.AppDomainAppPath.Replace("/", "\\");
        path = path.Replace("~", root + "\\").Replace("\\\\", "\\");
      }
      return new FileInfo(path).FullName;
    }

    /// <summary>
    /// Retrieves the media type set by the xsl's output element.
    /// This is done by reflection due to its protection level.
    /// </summary>
    /// <returns>String</returns>
    private string getMediaType()
    {
      try
      {
        return typeof(XmlWriterSettings).GetField("mediaType", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(xsl.OutputSettings) as string;
      }
      catch
      {
        return null;
      }
    }

    /// <summary>
    /// Adds an object as parameter to the xsl, so it can be obtained from within xsl by a global param 
    /// definition with the same name.
    /// The object will be serialized automatically if needed to.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="obj"></param>
    private void addObject(string name, object obj)
    {
      if (obj == null) return;
      if (obj is ValueType || obj is String || obj is XmlNode)
        arguments.AddParam(name, "", obj);
      else
      {
        XDocument xObj = obj as XDocument;
        if (xObj != null)
          arguments.AddParam(name, "", xObj.Document);
        else
          arguments.AddParam(name, "", ((XmlDocument)Serializer.ToXml(obj)).DocumentElement);
      }
    }

    #region IViewDataContainer Members

    public ViewDataDictionary ViewData
    {
      get { return viewContext.ViewData; }
      set { throw new NotSupportedException(); }
    }

    #endregion
  }
}
