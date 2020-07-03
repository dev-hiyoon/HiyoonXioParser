using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

namespace HiyoonXioParser
{
    class XIOFile
    {
        public String FilePath { get; set; }
        public String Name { get; set; }
        public int TotalLength { get; set; }

        public XIOFile(String path)
        {
            this.FilePath = path.Replace("\r\n", "");
            XmlDocument xDoc = new XmlDocument();
            xDoc.Load(this.FilePath);
            XmlNodeList name = xDoc.GetElementsByTagName("name");
            this.Name = name[0].InnerText + "(" + Path.GetFileName(this.FilePath) +  ")";
            this.TotalLength = XIOParser.getTotalLength(path);
        }
    }
}
