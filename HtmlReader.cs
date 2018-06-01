using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

namespace magic.html
{
    /* 包装xmlReader,提供针对html文档的过滤某些字符串的处理
     * */
    public class HtmlReader : IDisposable
    {
        private static String EntityReplaceCharacter = "{[##*#^#}";
        private String html = null;
        private String formatHtml = null;
        private StringReader stringReader = null;
        private XmlReader xmlReader = null;



        public String Name { get { return xmlReader.Name; } }
        public String Value { get { return decode(xmlReader.Value); } }
        public int Depth { get { return xmlReader.Depth; } }
        public XmlNodeType NodeType { get { return xmlReader.NodeType; } }

        public HtmlReader(String html)
        {
            this.html = html;

            //格式化html文档
            formatHtml = magic.html.HtmlHelper.FormatHtml(html);
            //过滤控制字符
            List<char> filters = null;
            formatHtml = magic.html.HtmlHelper.FilterControlChar(formatHtml, out filters);

#if DEBUG
            if (filters.Count > 0)
            {
                magic.debug.Utility.WriteLine("过滤掉控制字符：",ConsoleColor.DarkRed);
                foreach (char c in filters)
                {
                    magic.debug.Utility.WriteLine(((int)c).ToString("x"),ConsoleColor.DarkRed);
                }
            }
#endif

            //编码&
            formatHtml = encode(formatHtml);

            //保存到文件
            using (FileStream fs = new FileStream("html.html", FileMode.Create, FileAccess.Write))
            {
                Byte[] data = Encoding.UTF8.GetBytes(formatHtml);
                fs.Write(data, 0, data.Length);
            }

            stringReader = new StringReader(formatHtml);
            xmlReader = XmlReader.Create(stringReader, new XmlReaderSettings() { XmlResolver = null });

        }


        public Boolean Read()
        {
            return xmlReader.Read();
        }
        public String ReadInnerXml()
        {
            return decode(XmlTools.ReadInnerHtml(xmlReader.ReadSubtree()));
        }
        public String ReadOuterXml()
        {
            //return decode(XmlReader.ReadOuterXml());
            return decode(XmlTools.ReadOuterHtml(xmlReader.ReadSubtree()));
        }
        /// <summary>
        /// 读取当前节点的所有属性
        /// </summary>
        /// <returns></returns>
        public Dictionary<String, String> ReadAttributes()
        {
            Dictionary<String, String> attrs = new Dictionary<string, string>();
            if (xmlReader.HasAttributes)
            {
                xmlReader.MoveToFirstAttribute();
                //处理替换的&
                attrs.Add(xmlReader.Name, decode(xmlReader.Value));
                for (int i = 1; i < xmlReader.AttributeCount; i++)
                {
                    xmlReader.MoveToNextAttribute();
                    ////处理替换的&
                    attrs.Add(xmlReader.Name, decode(xmlReader.Value));
                }
                xmlReader.MoveToElement();
            }


            return attrs;
        }

        /// <summary>
        /// 读取当前节点的所有Text
        /// </summary>
        /// <returns></returns>
        public String ReadAllText()
        {
            return decode(XmlTools.ReadAllText(xmlReader.ReadSubtree()));
        }

        /// <summary>
        /// 紧读取当前节点的文本
        /// </summary>
        /// <returns></returns>
        public String ReadText()
        {
            return decode(XmlTools.ReadText(xmlReader.ReadSubtree()));
        }

        public String ReadIntelligentText()
        {
            return decode(XmlTools.ReadIntelligentText(xmlReader.ReadSubtree()));
        }

        /// <summary>
        /// 替换原实体引用字符&
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        private String encode(String text)
        {
            //处理实体字符串&
            text = text.Replace("&", EntityReplaceCharacter);
            return text;
        }

        /// <summary>
        /// 恢复原实体引用字符&
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        private String decode(String text)
        {
            //处理实体字符串&
            text =System.Web.HttpUtility.HtmlDecode( text.Replace(EntityReplaceCharacter, "&"));
            return text;
        }


        public void Dispose()
        {
            xmlReader.Close();
            stringReader.Close();
        }




       

    }
}
