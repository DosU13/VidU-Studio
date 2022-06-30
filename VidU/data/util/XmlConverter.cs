using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace VidU.data
{
    public class XmlConverter
    {
        public static XElement ListToElement<T>(List<T> list) where T : XmlBase
        {
            XElement element = new XElement("list", new XAttribute("count", list.Count));
            for (int i = 0; i < list.Count; i++)
            {
                XElement xElement = new XElement("item-" + i, list[i]?.ToXElement());
                element.Add(xElement);
            }
            return element;
        }

        public static List<T> ElementToList<T>(XElement xElement) where T : XmlBase, new()
        {
            if (xElement == null) return new List<T>();
            XElement element = xElement.Element("list");
            int count = int.Parse(element.Attribute("count").Value);
            List<T> list = new List<T>(count);
            for (int i = 0; i < count; i++)
            {
                XElement item = element.Element("item-" + i);
                T t = new T();
                t.LoadFromXElement(item);
                list.Insert(i, t); // Use insert
            }
            return list;
        }

        public static XElement ListToElement(List<double> list)
        {
            XElement element = new XElement("list", new XAttribute("count", list.Count));
            for (int i = 0; i < list.Count; i++)
            {
                XElement xElement = new XElement("item-" + i, list[i]);
                element.Add(xElement);
            }
            return element;
        }

        public static List<double> ElementToList(XElement xElement)
        {
            XElement element = xElement.Element("list");
            int count = int.Parse(element.Attribute("count").Value);
            List<double> list = new List<double>(count);
            for (int i = 0; i < count; i++)
            {
                XElement item = element.Element("item-" + i);
                list.Insert(i, double.Parse(item.Value));
            }
            return list;
        }

        public static XElement ListToElement(List<string> list)
        {
            XElement element = new XElement("list", new XAttribute("count", list.Count));
            for (int i = 0; i < list.Count; i++)
            {
                XElement xElement = new XElement("item-" + i, list[i]);
                element.Add(xElement);
            }
            return element;
        }

        public static List<string> ElementToStrList(XElement xElement)
        {
            XElement element = xElement.Element("list");
            int count = int.Parse(element.Attribute("count").Value);
            List<string> list = new List<string>(count);
            for (int i = 0; i < count; i++)
            {
                XElement item = element.Element("item-" + i);
                list.Insert(i, item.Value);
            }
            return list;
        }

        public static List<Media> ElementToMediaList(XElement xElement)
        {
            XElement element = xElement.Element("list");
            int count = int.Parse(element.Attribute("count").Value);
            List<Media> list = new List<Media>(count);
            for (int i = 0; i < count; i++)
            {
                XElement item = element.Element("item-" + i);
                list.Insert(i, Media.Create(item)); // Use insert
            }
            return list;
        }

        public static List<BaseClip> ElementToMediaClipList(XElement xElement)
        {
            XElement element = xElement.Element("list");
            int count = int.Parse(element.Attribute("count").Value);
            List<BaseClip> list = new List<BaseClip>(count);
            for (int i = 0; i < count; i++)
            {
                XElement item = element.Element("item-" + i);
                list.Insert(i, BaseClip.Create(item)); // Use insert
            }
            return list;
        }
    }
}
