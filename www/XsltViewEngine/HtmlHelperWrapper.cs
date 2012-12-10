using System.Web.Mvc;
using System.Web.Mvc.Html;
using System.Xml.XPath;
using MvcContrib.UI;
using System.Xml;

namespace Q42.Mvc.XsltViewEngine
{
  /// <summary>
  /// Wrapper around the HtmlHelper to allow RenderPartial, ActionLink and more from within XSLT.
  /// </summary>
  public class HtmlHelperWrapper
  {
    private HtmlHelper helper;
    private ViewContext viewContext;
    
    public HtmlHelperWrapper(ViewContext viewContext, IViewDataContainer viewDataContainer)      
    {
      this.viewContext = viewContext;
      helper = new HtmlHelper(viewContext, viewDataContainer);
    }
    
    private string nullable(MvcHtmlString value)
    {
      return value == null || string.IsNullOrEmpty(value.ToHtmlString()) ? "" : value.ToHtmlString();
    }

    public string ActionLink(string linkText, string actionName)
    {
      return helper.ActionLink(linkText, actionName).ToHtmlString();
    }

    public string ActionLink(string linkText, string actionName, string controllerName)
    {
      return helper.ActionLink(linkText, actionName, controllerName).ToHtmlString();
    }

    public string RenderPartial(string partialViewName)
    {
      var blockRenderer = new BlockRenderer(viewContext.HttpContext);
      string contents = blockRenderer.Capture(() => RenderPartialExtensions.RenderPartial(helper, partialViewName));
      return contents;
    }

    public string RenderPartial(string partialViewName, XPathNavigator model)
    {
      var blockRenderer = new BlockRenderer(viewContext.HttpContext);
      string contents = blockRenderer.Capture(() => RenderPartialExtensions.RenderPartial(helper, partialViewName, model.UnderlyingObject));
      return contents;
    }

    public string ValidationSummary()
    {
      return nullable(helper.ValidationSummary());
    }

    public string ValidationSummary(string message)
    {
      return nullable(helper.ValidationSummary(message));
    }

    public string ValidationMessage(string modelName)
    {
      return nullable(helper.ValidationMessage(modelName));
    }

    public string ValidationMessage(string modelName, string validationMessage)
    {
      return nullable(helper.ValidationMessage(modelName, validationMessage));
    }

    public string Encode(string value)
    {
      return helper.Encode(value);
    }

    public string TextBox(string name)
    {
      return helper.TextBox(name).ToHtmlString();
    }

    public string TextBox(string name, string value)
    {
      return helper.TextBox(name, value).ToHtmlString();
    }

    public string CheckBox(string name)
    {
      return helper.CheckBox(name).ToHtmlString();
    }

    public string CheckBox(string name, string value)
    {
      return helper.CheckBox(name, value).ToHtmlString();
    }

    public string Password(string name)
    {
      return helper.Password(name).ToHtmlString();
    }

    public string Password(string name, string value)
    {
      return helper.Password(name, value).ToHtmlString();
    }
  }
}
