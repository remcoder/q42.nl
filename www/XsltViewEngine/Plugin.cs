using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.SessionState;
using System.Xml;
using System.Xml.XPath;
using System.Xml.Xsl;

namespace Q42.Mvc.XsltViewEngine
{
  /// <summary>
  /// The base Plugin for all plugins to inherit from.
  /// </summary>
  public abstract class Plugin
  {
    private static List<string> assemblies = new List<string>();
    private static Dictionary<string, Plugin> plugins = new Dictionary<string, Plugin>();
    private static XPathNavigator nullNavigator = new XmlDocument().CreateNavigator();
    private static List<string> namespaceList = new List<string>();
    private static string[] namespaces;
    
    /// <summary>
    /// Offer direct access to the Request for all derived plugins
    /// </summary>
    protected HttpRequest Request { get { return HttpContext.Current.Request; } }

    /// <summary>
    /// Offer direct access to the Response for all derived plugins
    /// </summary>
    protected HttpResponse Response { get { return HttpContext.Current.Response; } }

    /// <summary>
    /// Offer direct access to the Session for all derived plugins
    /// </summary>
    protected HttpSessionState Session { get { return HttpContext.Current.Session; } }

    /// <summary>
    /// Prepare the collection of static plugins and other stuff
    /// </summary>
    static Plugin()
    {
      // add assemblies from the bin folder to the list
      string binPath = Path.Combine(HttpRuntime.AppDomainAppPath, "bin");
      if (Directory.Exists(binPath))
      {
        string[] dlls = Directory.GetFiles(binPath, "*.dll");
        foreach (string dll in dlls)
        {
          FileInfo dllFile = new FileInfo(dll);
          string asmName = dllFile.Name.Replace(".dll", "").Replace(".DLL", ""); ;
          assemblies.Add(asmName);
        }
      }
      
      // add the code assembly to the list
      assemblies.Add("App_Code");

      // add plugins from all assemblies in the list
      foreach (string assemblyName in assemblies)
      {        
        try
        {
          addPluginsByAssembly(Assembly.Load(assemblyName));
        }
        catch { }
      }
      addPluginsByAssembly(Assembly.GetCallingAssembly());
      namespaces = new string[namespaceList.Count];
      namespaceList.CopyTo(namespaces);
    }
    
    /// <summary>
    /// Scans an assembly for available plugins and makes them available to xsl
    /// </summary>
    /// <param name="asm">Assembly instance to retrieve types from.</param>
    private static void addPluginsByAssembly(Assembly asm)
    {
      if (asm == null) return;
      
      // create static list of plugins
      foreach (Type objectType in asm.GetTypes())
      {
        if (objectType.IsSubclassOf(typeof(Plugin)) && !objectType.IsAbstract)
        {
          Plugin plugin = (Plugin)Activator.CreateInstance(objectType);          
          if (!plugins.ContainsKey(objectType.FullName))
          {
            plugins[objectType.FullName] = plugin;
            namespaceList.Add(objectType.FullName);
          }
        }
      }
    }

    /// <summary>
    /// Adds all available plugins to the XsltArgumentList for use in xsl
    /// </summary>
    /// <param name="xslArgs"></param>
    internal static void AddAll(XsltArgumentList xslArgs)
    {
      foreach (string prefix in plugins.Keys)
      {
        Plugin plugin = plugins[prefix];
        xslArgs.AddExtensionObject(plugin.NamespaceUri, plugin);
      }
    }

    /// <summary>
    /// The namespace of a plugin, for instance "Quplo.Plugins.StringPlugin"
    /// </summary>
    public string NamespaceUri
    {
      get { return "urn:" + this.GetType().Name; }
    }
    
    /// <summary>
    /// The list of plugins.
    /// </summary>
    public static Dictionary<string, Plugin> Plugins
    {
      get { return plugins; }
    }

    /// <summary>
    /// A void XPathNavigator, for plugin functions to return nothing
    /// </summary>
    public static XPathNavigator NullNavigator
    {
      get { return nullNavigator; }
    }
    
    /// <summary>
    /// Strips a string from Plugin namespaces. Used for cleaning up the output
    /// before sending it to the response.
    /// </summary>
    /// <param name="s">String to clean</param>
    /// <returns>Cleaned up string</returns>
    public static string StripXmlNamespaces(string s)
    {
      string nsOr = String.Join("|", namespaces).Replace(".", "\\.");
      return Regex.Replace(s, "\\s*xmlns:\\w+=\"(" + nsOr + ")-\"", "", RegexOptions.IgnoreCase);
    }
  }
}