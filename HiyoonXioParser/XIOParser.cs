using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Xml;

namespace HiyoonXioParser
{
    class XIOParser
    {
        private static int preIdx = 1109;
        private static int postIdx = 2;
        private static int sttIdx = 0;
        private static System.Text.Encoding euckr = System.Text.Encoding.GetEncoding(51949);

        public static List<XIOData> parse(String filePath, String contents)
        {
            sttIdx = 0;
            List<XIOData> XIODatas = new List<XIOData>();
            byte[] xioByte = getByte(contents);
            int totalLength = getTotalLength(filePath);
            if (xioByte.Length == totalLength + preIdx + postIdx)
            {
                xioByte = xioByte.Skip(preIdx).Take(totalLength).ToArray();
            }

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

        public static byte[] getByte(String contents)
        {
            return euckr.GetBytes(contents);
        }

        public static int getTotalLength(String filePath)
        {
            int result = 0;
            XmlNodeList nodeList = getNodeList(filePath, "include");
            if (nodeList != null && nodeList.Count > 0)
            {
                String includeFilePath = String.Format(@"{0}\\{1}.xio", Path.GetDirectoryName(filePath), nodeList[0].Attributes["id"].Value);
                XmlNodeList includeFields = getNodeList(includeFilePath, "field");
                foreach (XmlNode field in includeFields)
                {
                    result += int.Parse(field.Attributes["length"].Value);
                };
            }

            XmlNodeList fields = getNodeList(filePath, "field");
            foreach (XmlNode field in fields)
            {
                result += int.Parse(field.Attributes["length"].Value);
            };

            return result;
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
