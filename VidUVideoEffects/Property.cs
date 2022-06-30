using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation.Collections;

namespace VidUVideoEffects
{
    internal class Property
    {
        private bool IsMuzU;
        private Dictionary<float, float> Values;
        private string TransitionType;
        private double Value;
        private double MinValue;
        private double MaxValue;

        public Property(string name, IPropertySet configuration)
        {
            IsMuzU = (bool)configuration[nameof(IsMuzU)];
            if(!IsMuzU) Value = (double)configuration[name+"_"+nameof(Value)];
            else
            {
                Values = (Dictionary<float, float>)configuration[nameof(Values)];
                TransitionType = (string)configuration[name + "_" + nameof(TransitionType)];
                MinValue = (double)configuration[name + "_" + nameof(MinValue)];
                MaxValue = (double)configuration[name + "_" + nameof(MaxValue)];
            }
        }

        private Random random = new Random();
        private double trnTime = 0.1;
        private double trnValue = 0.2;
        internal float Get(double time)
        {
            if (!IsMuzU) return Convert.ToSingle(Value);
            else
            {
                float lastTime = new List<float>(Values.Keys).FindLast(it => it <= time);
                double v;
                if (lastTime == 0 || lastTime == -1) v = 0;
                else v = Values[lastTime];
                double tv = 0;
                if (time - lastTime < trnTime) tv = trnValue * (trnTime - time + lastTime) / trnTime;
                switch (TransitionType)
                {
                    case "None": break;
                    case "Growth": v -= tv; break;
                    case "Decay": v += tv; break;
                    case "Shake": v += tv*(random.NextDouble()*2-1); break;
                }
                if (v < 0) v = 0;
                if (v > 1) v = 1;
                Debug.WriteLine(v);
                return Convert.ToSingle(v * (MaxValue - MinValue) + MinValue);
            }
        }
    }
}
