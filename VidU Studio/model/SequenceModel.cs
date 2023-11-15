using MuzUStandard.data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace VidU_Studio.model
{
    public class SequenceModel
    {
        private Sequence _sequence;

        public SequenceModel(Sequence sequence)
        {
            this._sequence = sequence;
            
            Properties = new List<string>();
            var template = _sequence.SequenceTemplate;
            if (template.NoteEnabled) Properties.Add("Note");
            if (template.LengthEnabled) Properties.Add("Length");
            if (template.LyricsEnabled) Properties.Add("Lyrics");
        }

        public string Name => _sequence.Name;

        public List<string> Properties { get; private set; }

        internal List<KeyValuePair<double, string>> GetLyrics(double startPos, double endPos)
        {
            List<KeyValuePair<double, string>> res = new List<KeyValuePair<double, string>>();
            string word = "";
            List<(int iter, double time)> temp = new List<(int, double)>();
            int iter = 1;
            foreach(var n in _sequence.NodeList)
            {
                if (n.Lyrics == null) continue;
                var time = n.Time / 1_000_000.0;
                if (startPos <= time && time < endPos)
                {
                    word += Regex.Replace(n.Lyrics, "[^a-zA-Z0-9']", "").ToLower();
                    temp.Add((iter++, time));
                    if (n.Lyrics.EndsWith(' ') || n.Lyrics.EndsWith('\n'))
                    {
                        foreach (var t in temp)
                        {
                            res.Add(KeyValuePair.Create(t.time, t.iter == 1 ? word : $"{word}_{t.iter}"));
                        }
                        iter = 1;
                        word = "";
                        temp.Clear();
                    }
                }
            }
            return res;
        }

        internal List<KeyValuePair<double, double>> GetNotes(double startPos, double endPos)
        {
            List<KeyValuePair<double, double>> res = new List<KeyValuePair<double, double>>();
            foreach (var n in _sequence.NodeList)
            {
                var time = n.Time / 1_000_000.0;
                if (startPos <= time && time < endPos)
                {
                    res.Add(KeyValuePair.Create(time, (double)n.Note));
                }
            }
            return res;
        }

        internal List<KeyValuePair<double, double>> GetLengths(double startPos, double endPos)
        {
            List<KeyValuePair<double, double>> res = new List<KeyValuePair<double, double>>();
            foreach (var n in _sequence.NodeList)
            {
                var time = n.Time / 1_000_000.0;
                if (startPos <= time && time < endPos)
                {
                    res.Add(KeyValuePair.Create(time, (double)n.Length));
                }
            }
            return res;
        }
    }
}
