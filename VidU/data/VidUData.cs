using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace VidU.data
{
    public class VidUData: XmlBase
    {
        public VidUData() { }
        public VidUData(XElement xElement) : base(xElement) { }

        public string Name { get; set; } = "NoName";
        public string MuzUPath { get; set; }
        public string MusicPath { get; set; }
        public double MusicAllignSec { get; set; } = 0;
        public double BPM { get; set; } = 60;
        public List<BaseClip> Clips { get; set; } = new List<BaseClip>();

        internal override XElement ToXElement()
        {
            ThisElement = new XElement("VidU",
                            new XElement(nameof(Name), Name),
                            new XElement(nameof(MuzUPath), MuzUPath),
                            new XElement(nameof(MusicPath), MusicPath),
                            new XElement(nameof(MusicAllignSec), MusicAllignSec),
                            new XElement(nameof(BPM), BPM),
                            new XElement(nameof(Clips), XmlConverter.ListToElement(Clips)));
            return ThisElement;
        }

        internal override void LoadFromXElement(XElement xElement)
        {
            ThisElement = xElement;
            Name = ThisElement.Element(nameof(Name)).Value;
            MuzUPath = ThisElement.Element(nameof(MuzUPath))?.Value;
            MusicPath = ThisElement.Element(nameof(MusicPath))?.Value;
            MusicAllignSec = double.Parse(ThisElement.Element(nameof(MusicAllignSec))?.Value??"0");
            BPM = double.Parse(ThisElement.Element(nameof(BPM))?.Value??"60");
            Clips = XmlConverter.ElementToMediaClipList(ThisElement.Element(nameof(Clips)));
            base.LoadFromXElement(ThisElement);
        }
    }
}
