using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using VidU.data;
using Windows.Storage;

namespace VidU
{
    public class VidUProject
    {
        public VidUData _data;
        public VidUData data
        {
            get { return _data; }
            set
            {
                _data = value;
            }
        }

        public VidUProject() => data = new VidUData();

        public VidUProject(Stream stream) => data = LoadFromStream(stream);

        private VidUData LoadFromStream(Stream stream) 
        {
            return new VidUData(XDocument.Load(stream).Root);
        }

        public void Save(Stream stream)
        { 
            stream.SetLength(0);
            ToXDocument().Save(stream);
        }

        private XDocument ToXDocument()
        {
            XDocument doc = new XDocument(data.ToXElement());
            doc.Declaration = new XDeclaration("1.0", "utf-8", "true");
            return doc;
        }
    }
}
