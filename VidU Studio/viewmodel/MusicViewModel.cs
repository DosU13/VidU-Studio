using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VidU.data;
using VidU_Studio.model;
using Windows.Storage;
using Windows.Storage.AccessCache;

namespace VidU_Studio.viewmodel
{
    internal class MusicViewModel: BindableBase
    {
        private VidUData data;
        private CompositionModel CompModel;

        public MusicViewModel(VidUData vidUData, CompositionModel compositionModel)
        {
            data = vidUData;
            CompModel = compositionModel;
            SetMusicData(data.MusicPath, data.MusicAllignSec);
        }

        internal async void SetMusicData(string MusicPath, double MusicAllignSec)
        {
            SetProperty(ref musicAllignSec, MusicAllignSec, nameof(MusicAllignSec));
            data.MusicAllignSec = MusicAllignSec;
            if (MusicPath == null || MusicPath == "") File = null;
            else try{ File = await StorageFile.GetFileFromPathAsync(MusicPath);
                    }catch(Exception) { File = null; };
        }

        internal string MusicPath { get {
                if (file == null) return "";
                else return file.Path;
            } }

        private StorageFile file;
        public StorageFile File
        {
            get => file;
            private set
            {
                SetProperty(ref file, value, nameof(MusicPath));
                data.MusicPath = value?.Path;
                CompModel.UpdateMusic(File, MusicAllignSec);
            }
        }

        private double musicAllignSec;
        public double MusicAllignSec
        {
            get => musicAllignSec;
            set
            {
                SetProperty(ref musicAllignSec, value);
                data.MusicAllignSec = value;
                CompModel.UpdateMusic(File, MusicAllignSec);
            }
        }

        internal async Task<bool> OpenMusicFilePicker()
        {
            var picker = new Windows.Storage.Pickers.FileOpenPicker();
            picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.MusicLibrary;
            picker.FileTypeFilter.Add(".mp3");
            picker.FileTypeFilter.Add(".wav");
            try
            {
                StorageFile file = await picker.PickSingleFileAsync();
                if (file == null) return false;
                File = file;
                StorageApplicationPermissions.FutureAccessList.Add(file);
                return true;
            }
            catch (Exception) { }
            return false;
        }
    }
}
