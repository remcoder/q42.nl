<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0"
            xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
            xmlns:Html="urn:HtmlHelper"
            xmlns:ext="http://exslt.org/common"
            xmlns:dataplugin="urn:dataplugin">
  <xsl:output method="html" indent="yes"/>

  <xsl:template match="/">
    <html>
      <head>
        <title>Test</title>
      </head>
      <body>
        Hallo?
        <textarea>
          <xsl:copy-of select="dataplugin:Get()"/>
        </textarea>
      </body>
    </html>
  </xsl:template>
</xsl:stylesheet>
