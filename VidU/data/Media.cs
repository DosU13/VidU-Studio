using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace VidU.data
{
    public abstract class Media : XmlBase
    {
        public Media() { }
        public Media(XElement xElement) : base(xElement) { }

        public string Path { get; set; }

        internal override XElement ToXElement()
        {
            ThisElement.Add(new XElement(nameof(Path), Path));
            return base.ToXElement();
        }

        internal override void LoadFromXElement(XElement xElement)
        {
            Path = ThisElement.Element(nameof(Path))?.Value;
            base.LoadFromXElement(ThisElement);
        }

        internal static Media Create(XElement xElement)
        {
            bool tryImage = xElement.Element(nameof(Image)) != null;
            if (tryImage) return new Image(xElement);
            else return new Video(xElement);
        }
    }

    public class Image : Media
    {
        public Image() { }
        public Image(XElement xElement) : base(xElement) { }

        internal override XElement ToXElement()
        {
            ThisElement = new XElement(nameof(Image));
            return base.ToXElement();
        }

        internal override void LoadFromXElement(XElement xElement)
        {
            ThisElement = xElement.Element(nameof(Image));
            base.LoadFromXElement(ThisElement);
        }
    }

    public class Video : Media
    {
        public Video() { }
        public Video(XElement xElement) : base(xElement) { }

        public double TrimFromStart { get; set; } = 0;
        public int Volume { get; set; } = 50;

        internal override XElement ToXElement()
        {
            ThisElement = new XElement(nameof(Video),
                new XElement(nameof(TrimFromStart), TrimFromStart),
                new XElement(nameof(Volume), Volume));
            return base.ToXElement();
        }

        internal override void LoadFromXElement(XElement xElement)
        {
            ThisElement = xElement.Element(nameof(Video));
            TrimFromStart = double.Parse(ThisElement.Element(nameof(TrimFromStart))?.Value ?? "0");
            Volume = int.Parse(ThisElement.Element(nameof(Volume))?.Value ?? "0");
            base.LoadFromXElement(ThisElement);
        }
    }
}
