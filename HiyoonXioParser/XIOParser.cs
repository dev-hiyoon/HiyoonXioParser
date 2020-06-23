using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Xml;

namespace HiyoonXioParser
{
    class XIOParser
    {
        private static int sttIdx = 0;
        private static System.Text.Encoding euckr = System.Text.Encoding.GetEncoding(51949);

        public static List<XIOData> parse(String filePath, String xio)
        {
            sttIdx = 0;
            List<XIOData> XIODatas = new List<XIOData>();            
            byte[] xioByte = euckr.GetBytes(xio);

            XmlNodeList nodeList = getNodeList(filePath, "include");
            if (nodeList != null && nodeList.Count > 0)
            {
                String includeFilePath = String.Format(@"{0}\\{1}.xio", Path.GetDirectoryName(filePath), nodeList[0].Attributes["id"].Value);
                XmlNodeList includeFields = getNodeList(includeFilePath, "field");
                getXIODatas(includeFields, xioByte).ForEach(XIODatas.Add);
            }

            XmlNodeList fields = getNodeList(filePath, "field");
            getXIODatas(fields, xioByte).ForEach(XIODatas.Add);
            return XIODatas;
        }

        public static String merge(List<XIOData> xIODatas)
        {
            byte[] result = null;
            xIODatas.ForEach(x =>
            {
                byte[] bytes = euckr.GetBytes(x.Value).Take(x.Length).ToArray();
                if (result == null)
                {
                    result = bytes;
                } else
                {
                    result = result.Concat(bytes).ToArray();
                }

                for (int i = 0; i < x.Length - bytes.Length; i++)
                {
                    result = result.Concat(euckr.GetBytes(" ")).ToArray();
                }
            });

            return euckr.GetString(result);
        }

        private static XmlNodeList getNodeList(String filePath, String tagName)
        {
            String path = filePath.Replace("\r\n", "");
            XmlDocument xDoc = new XmlDocument();
            xDoc.Load(path);
            return xDoc.GetElementsByTagName(tagName);
        }

        private static List<XIOData> getXIODatas (XmlNodeList fields, byte[] xioByte)
        {
            List<XIOData> result = new List<XIOData>();
            try
            {
                foreach (XmlNode field in fields)
                {
                    String id = field.Attributes["id"].Value;
                    String name = field.Attributes["name"].Value;
                    int length = int.Parse(field.Attributes["length"].Value);
                    result.Add(new XIOData(id, name, euckr.GetString(xioByte.Skip(sttIdx).Take(length).ToArray()), length));
                    sttIdx += length;
                };
            }
            catch (Exception ex)
            {
                MessageBox.Show("파싱 오류", ex.ToString());
            }

            return result;
        }
    }
}
