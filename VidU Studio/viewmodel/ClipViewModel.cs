using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VidU.data;
using Windows.Storage;

namespace VidU_Studio.viewmodel
{
    public interface ClipViewModel: INotifyPropertyChanged
    {
        BaseClip Data { get; }
        StorageFile Thumbnail { get; }
        double Duration { get; }
    }
}
