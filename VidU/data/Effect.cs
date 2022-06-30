using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace VidU.data
{
    public class Effect : XmlBase
    {
        public Effect() { }
        public Effect(XElement xElement) : base(xElement) { }

        public string Name { get; set; } = "EffectName";
        public bool IsMuzU { get; set; } = false;
        public NumberDictionaryXml TimingsWithValues = new NumberDictionaryXml();
        public List<EffectProperty> Properties { get; set; } = new List<EffectProperty>();

        internal override XElement ToXElement()
        {
            ThisElement = new XElement(nameof(Effect),
                            new XElement(nameof(Name), Name),
                            new XElement(nameof(IsMuzU), IsMuzU),
                            TimingsWithValues.ToXElement(),
                            new XElement(nameof(Properties), XmlConverter.ListToElement(Properties)));
            return ThisElement;
        }

        internal override void LoadFromXElement(XElement xElement)
        {
            ThisElement = xElement.Element(nameof(Effect));
            Name = ThisElement.Element(nameof(Name)).Value;
            IsMuzU = bool.Parse(ThisElement.Element(nameof(IsMuzU))?.Value??"false");
            TimingsWithValues = new NumberDictionaryXml(ThisElement);
            Properties = XmlConverter.ElementToList<EffectProperty>(ThisElement.Element(nameof(Properties)));
            base.LoadFromXElement(ThisElement);
        }
    }

    public class EffectProperty: XmlBase
    {
        public EffectProperty() { }
        public EffectProperty(XElement xElement) : base(xElement) { }

        public string Name { get; set; } = "PropertyName";
        public double Value { get; set; } = 0;
        public double MinValue { get; set; } = 0;
        public double MaxValue { get; set; } = 1;
        public string TransitionType { get; set; } = "None";

        internal override XElement ToXElement()
        {
            ThisElement = new XElement(nameof(EffectProperty),
                            new XElement(nameof(Name), Name),
                            new XElement(nameof(Value), Value),
                            new XElement(nameof(MinValue), MinValue),
                            new XElement(nameof(MaxValue), MaxValue),
                            new XElement(nameof(TransitionType), TransitionType));
            return ThisElement;
        }

        internal override void LoadFromXElement(XElement xElement)
        {
            ThisElement = xElement.Element(nameof(EffectProperty));
            Name = ThisElement.Element(nameof(Name)).Value;
            Value = double.Parse(ThisElement.Element(nameof(Value))?.Value??"0");
            MinValue = double.Parse(ThisElement.Element(nameof(MinValue))?.Value??"0");
            MaxValue = double.Parse(ThisElement.Element(nameof(MaxValue))?.Value ?? "1");
            TransitionType = ThisElement.Element(nameof(TransitionType)).Value;
            base.LoadFromXElement(ThisElement);
        }
    }
}
