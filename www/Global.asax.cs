using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using log4net;
using log4net.Config;
using Qsite2012.Controllers;
using Q42.Mvc.XsltViewEngine;
using Qsite2012.Plugins;

namespace Qsite2012
{
  // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
  // visit http://go.microsoft.com/?LinkId=9394801

  public class MvcApplication : HttpApplication
  {
    private static ILog log = LogManager.GetLogger(typeof(MvcApplication));

    public static void RegisterGlobalFilters(GlobalFilterCollection filters)
    {
      filters.Add(new HandleErrorAttribute());
    }

    public static void RegisterRoutes(RouteCollection routes)
    {
      routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

      routes.MapRoute(
          "Default", // Route name
          "{id}", // URL with parameters
          new { controller = "Page", action = "Index" },
          new { id = "test|Games|Gereedschapskist|Interaction-Engineering|Over-Q42|Producten|Projecten|Systeembeheersysteem|w00tcamp" } // regular expression matching all valid controllers
      );

      routes.MapRoute(
         "Home", // Route name
         "", // URL with parameters
         new { controller = "Page", action = "Home", id = UrlParameter.Optional } // Parameter defaults
      );

    }

    protected void Application_Start()
    {
      XmlConfigurator.Configure();
      log.Info("Starting website");

      AreaRegistration.RegisterAllAreas();

      RegisterGlobalFilters(GlobalFilters.Filters);
      RegisterRoutes(RouteTable.Routes);
      //ViewEngines.Engines.Clear();
      var xlsEngine = new XsltViewEngine();
      xlsEngine.AddExtension("urn:dataplugin", (v, c) => new DataPlugin());
      ViewEngines.Engines.Add(xlsEngine);
    }

    protected void Application_Error(object sender, EventArgs e)
    {
      if (Context.IsCustomErrorEnabled)
      {
        ShowCustomErrorPage(Server.GetLastError());
      }
      else {
        log.Error(string.Format("ERROR at: {0}", Request.Url.AbsoluteUri), Server.GetLastError());
      }
    }

    /// <summary>
    /// Redirect to errorcontroller with the given exception
    /// </summary>
    /// <param name="exception"></param>
    /// <remarks>
    /// copy-pasted largely from http://www.digitallycreated.net/Blog/57/getting-the-correct-http-status-codes-out-of-asp.net-custom-error-pages
    /// </remarks>
    private void ShowCustomErrorPage(Exception exception)
    {
      HttpException httpException = exception as HttpException;
      if (httpException == null)
        httpException = new HttpException(500, "Internal Server Error", exception);

      Response.Clear();
      RouteData routeData = new RouteData();
      routeData.Values.Add("controller", "Error");
      routeData.Values.Add("fromAppErrorEvent", true);

      switch (httpException.GetHttpCode())
      {
        case 403:
          routeData.Values.Add("action", "AccessDenied");
          log.Error(string.Format("ERROR [{0}] at : {1}", httpException.GetHttpCode(), Request.Url.AbsoluteUri), exception);
          break;
        case 404:
          log.Warn(string.Format("Page not found: {0} (referrer={1})", Request.Url.AbsoluteUri, Request.UrlReferrer));
          routeData.Values.Add("action", "NotFound");
          break;
        case 500:
          routeData.Values.Add("action", "ServerError");
          log.Error(string.Format("ERROR [{0}] at : {1}", httpException.GetHttpCode(), Request.Url.AbsoluteUri), exception);
          break;
        default:
          log.Error(string.Format("ERROR [{0}] at : {1}", httpException.GetHttpCode(), Request.Url.AbsoluteUri), exception);
          routeData.Values.Add("action", "OtherHttpStatusCode");
          routeData.Values.Add("httpStatusCode", httpException.GetHttpCode());
          break;
      }

      Server.ClearError();
      //try fix for somehow not seeing the custom error pages on acceptatie.schaatsen.nl http://stackoverflow.com/questions/553922/custom-asp-net-mvc-404-error-page
      Context.Response.TrySkipIisCustomErrors = true;
      IController controller = new ErrorController();
      controller.Execute(new RequestContext(new HttpContextWrapper(Context), routeData));
    }
  }
}