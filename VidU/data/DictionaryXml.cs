using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace VidU.data
{
    public class DictionaryXml : XmlBase
    {
        public DictionaryXml() { }
        public DictionaryXml(XElement xElement) : base(xElement) { }

        public bool IsValueInteger = false;
        public List<KeyValuePair<double, double>> Dict { get; set; } = new List<KeyValuePair<double, double>>();

        internal override XElement ToXElement()
        {
            ThisElement = new XElement(nameof(DictionaryXml),
                            new XElement(nameof(IsValueInteger), IsValueInteger),
                            ListToElement(Dict));
            return base.ToXElement();
        }

        internal override void LoadFromXElement(XElement xElement)
        {
            ThisElement = xElement.Element(nameof(DictionaryXml));
            if (ThisElement == null) return;
            IsValueInteger = bool.Parse(ThisElement.Element(nameof(IsValueInteger))?.Value??"false");
            Dict = ElementToDict(ThisElement);
            base.LoadFromXElement(ThisElement);
        }

        private static string KeyName = "TimeSec";
        private static string ValueName = "Value";

        private static XElement ListToElement(List<KeyValuePair<double, double>> list)
        {
            XElement element = new XElement("list", new XAttribute("count", list.Count));
            for (int i = 0; i < list.Count; i++)
            {
                XElement xElement = new XElement("item-" + i,
                                                 new XElement(KeyName, list[i].Key),
                                                 new XElement(ValueName, list[i].Value));
                element.Add(xElement);
            }
            return element;
        }

        private static List<KeyValuePair<double, double>> ElementToDict(XElement xElement)
        {
            XElement element = xElement.Element("list");
            int count = int.Parse(element.Attribute("count").Value);
            List<KeyValuePair<double, double>> list = new List<KeyValuePair<double, double>>(count);
            for (int i = 0; i < count; i++)
            {
                XElement item = element.Element("item-" + i);
                list.Insert(i, KeyValuePair.Create(double.Parse(item.Element(KeyName).Value),
                                                   double.Parse(item.Element(ValueName).Value)));
            }
            return list;
        }
    }
}
