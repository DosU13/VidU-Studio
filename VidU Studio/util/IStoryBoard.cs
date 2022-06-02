using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VidU_Studio.viewmodel;

namespace VidU_Studio.util
{
    public interface IStoryBoard
    {
        double GetStartTime(ClipViewModel caller);
        void UpdateComposition();
    }
}
