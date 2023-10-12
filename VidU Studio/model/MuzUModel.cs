using MuzUStandard;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VidU_Studio.model
{
    public class MuzUModel
    {
        private MuzUProject project;

        public double BPM => project.MuzUData.Tempo.BPM;
        public string MusicPath => project.MuzUData.MusicLocal.MusicPath;
        public double MusicAllign_μs => project.MuzUData.MusicLocal.MusicOffsetMicroseconds;

        private List<SequenceModel> sequences;
        public List<SequenceModel> Sequences
        {
            get
            {
                if (project == null) return null;
                if(sequences == null) 
                    sequences = project.MuzUData.SequenceList.Select(x => new SequenceModel(x)).ToList();
                return sequences;
            }
        }

        internal void OpenProject(Stream stream)
        {
            project = new MuzUProject(stream);
            sequences = null;
        }
    }
}
