using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Q42.Mvc.XsltViewEngine;
using System.Xml.XPath;
using System.Xml;
using System.Configuration;

namespace Qsite2012.Plugins
{
    public class DataPlugin : Plugin
    {
        public XPathNavigator Get()
        {
            XmlDocument xmlData = new XmlDocument();
            xmlData.Load(ConfigurationManager.AppSettings["DataPath"]);
            return xmlData.SelectSingleNode("/data/page[@url='" + Request.Url.AbsolutePath + "']").CreateNavigator();
        }
    }
}