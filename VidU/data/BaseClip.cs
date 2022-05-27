using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace VidU.data
{
    public abstract class BaseClip : XmlBase
    {
        public BaseClip(){}
        public BaseClip(XElement xElement) : base(xElement) { }
        
        public double StartPos { get; set; } = 1;

        internal override XElement ToXElement()
        {
            ThisElement.Add(new XElement(nameof(StartPos), StartPos));
            return base.ToXElement();
        }

        internal override void LoadFromXElement(XElement xElement)
        {
            StartPos = double.Parse(ThisElement.Element(nameof(StartPos))?.Value??"0");
            base.LoadFromXElement(ThisElement);
        }

        internal static BaseClip Create(XElement xElement)
        {
            var el = xElement.Elements().First();
            if (el.Name == nameof(SingleClip)) return new SingleClip(xElement);
            else if (el.Name == nameof(SequencerClip)) return new SequencerClip(xElement);
            else if (el.Name == nameof(AutoSequencerClip)) return new AutoSequencerClip(xElement);
            else return null;
        }
    }

    public class SingleClip : BaseClip
    {
        public SingleClip(){}
        public SingleClip(XElement xElement) : base(xElement){}

        public Media Media { get; set; }

        internal override XElement ToXElement()
        {
            ThisElement = new XElement(nameof(SingleClip), 
                Media.ToXElement());
            return base.ToXElement();
        }

        internal override void LoadFromXElement(XElement xElement)
        {
            ThisElement = xElement.Element(nameof(SingleClip));
            Media = Media.Create(ThisElement);
            base.LoadFromXElement(ThisElement);
        }
    }

    public class SequencerClip: BaseClip
    {
        public SequencerClip() { }
        public SequencerClip(XElement xElement) : base(xElement) { }

        public List<Media> Medias { get; set; } = new List<Media>();
        public DictionaryXml TimingsWithValues = new DictionaryXml();  

        internal override XElement ToXElement()
        {
            ThisElement = new XElement(nameof(SequencerClip),
                new XElement(nameof(Medias), XmlConverter.ListToElement(Medias)),
                TimingsWithValues.ToXElement());
            return base.ToXElement();
        }

        internal override void LoadFromXElement(XElement xElement)
        {
            ThisElement = xElement.Element(nameof(SequencerClip));
            Medias = XmlConverter.ElementToMediaList(ThisElement.Element(nameof(Medias)));
            TimingsWithValues = new DictionaryXml(ThisElement);
            base.LoadFromXElement(ThisElement);
        }
    }

    public class AutoSequencerClip : BaseClip
    {
        public AutoSequencerClip() { }
        public AutoSequencerClip(XElement xElement) : base(xElement) { }

        public List<Media> Medias { get; set; } = new List<Media>();
        public List<double> Values = new List<double>();
        public bool IsValueInteger = false;
        public DictionaryXml TimingsWithIndices = new DictionaryXml();

        internal override XElement ToXElement()
        {
            ThisElement = new XElement(nameof(AutoSequencerClip),
                new XElement(nameof(Medias), XmlConverter.ListToElement(Medias)),
                new XElement(nameof(Values), XmlConverter.ListToElement(Values)),
                new XElement(nameof(IsValueInteger), IsValueInteger),
                TimingsWithIndices.ToXElement());
            return base.ToXElement();
        }

        internal override void LoadFromXElement(XElement xElement)
        {
            ThisElement = xElement.Element(nameof(AutoSequencerClip));
            Medias = XmlConverter.ElementToMediaList(ThisElement.Element(nameof(Medias)));
            Values = XmlConverter.ElementToList(ThisElement.Element(nameof(Values)));
            IsValueInteger = bool.Parse(ThisElement.Element(nameof(IsValueInteger))?.Value??"false");
            TimingsWithIndices = new DictionaryXml(ThisElement);
            base.LoadFromXElement(ThisElement);
        }
    }
}
