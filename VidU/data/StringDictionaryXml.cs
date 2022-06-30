using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace VidU.data
{
    public class StringDictionaryXml : XmlBase
    {
        public StringDictionaryXml() { }
        public StringDictionaryXml(XElement xElement) : base(xElement) { }

        public List<KeyValuePair<double, string>> Dict { get; set; } = new List<KeyValuePair<double, string>>();

        internal override XElement ToXElement()
        {
            ThisElement = new XElement(nameof(StringDictionaryXml),
                            ListToElement(Dict));
            return base.ToXElement();
        }

        internal override void LoadFromXElement(XElement xElement)
        {
            ThisElement = xElement.Element(nameof(StringDictionaryXml));
            if (ThisElement == null) return;
            Dict = ElementToDict(ThisElement);
            base.LoadFromXElement(ThisElement);
        }

        private static string KeyName = "TimeSec";
        private static string ValueName = "Value";

        private static XElement ListToElement(List<KeyValuePair<double, string>> list)
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

        private static List<KeyValuePair<double, string>> ElementToDict(XElement xElement)
        {
            XElement element = xElement.Element("list");
            int count = int.Parse(element.Attribute("count").Value);
            List<KeyValuePair<double, string>> list = new List<KeyValuePair<double, string>>(count);
            for (int i = 0; i < count; i++)
            {
                XElement item = element.Element("item-" + i);
                list.Insert(i, KeyValuePair.Create(double.Parse(item.Element(KeyName).Value),
                                                   item.Element(ValueName).Value));
            }
            return list;
        }


        public static StringDictionaryXml NumbToStrDictXml(NumberDictionaryXml source)
        {
            List<KeyValuePair<double, string>> r;
            if(source.IsValueInteger) r = source.Dict.Select(it=>
                    KeyValuePair.Create(it.Key, ((int)it.Value).ToString())).ToList();
            else r = source.Dict.Select(it => KeyValuePair.Create(it.Key, it.Value.ToString())).ToList();
            return new StringDictionaryXml() { Dict = r.ToList() };
        }
    }
}