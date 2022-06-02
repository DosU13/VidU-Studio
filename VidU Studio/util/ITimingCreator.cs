using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VidU.data;
using VidU_Studio.viewmodel;

namespace VidU_Studio.util
{
    internal interface ITimingCreator
    {
        Task AddGroupDialog();
        Task<KeyValuePair<bool, DictionaryXml>> NormTimingsDialog(double startTime, double endTime);
    }
}
