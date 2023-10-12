using MuzUStandard.data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VidU_Studio.model
{
    public class SequenceModel
    {
        private Sequence sequence;

        public SequenceModel(Sequence x)
        {
            this.sequence = x;
        }

        public string Name => sequence.Name;

        public List<string> Lyrics { get; internal set; }
        public List<PropertyModel> Properties { get; internal set; }
    }
}
