using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VidU;

namespace VidU_Studio.util
{
    internal interface IMainPage
    {
        Task<bool> SaveWorkDialog();

        void ProjectChanged(VidUProject newProject);

        void BPMChanged();
    }
}
