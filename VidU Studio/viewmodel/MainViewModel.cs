using MuzU;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VidU;
using VidU.data;
using VidU_Studio.model;
using VidU_Studio.util;
using Windows.Storage;
using Windows.Storage.AccessCache;

namespace VidU_Studio.viewmodel
{
    internal class MainViewModel : BindableBase
    {
        private readonly VidUData data;
        private MusicViewModel musicVM;
        private IMainPage IMainPage;
        public MainViewModel(VidUData vidUData, MusicViewModel musicViewModel, IMainPage mainPage)
        {
            data = vidUData;
            musicVM = musicViewModel;
            IMainPage = mainPage;
            BPM = data.BPM;
            MuzUPath = data.MuzUPath;
            Task.Run(async () => {
                try { await LoadMuzUProject(await StorageFile.GetFileFromPathAsync(data.MuzUPath)); }
                catch (Exception) { MuzUPath = ""; }});
        }

        private string muzUPath;
        internal string MuzUPath { get => muzUPath; 
                                   set { SetProperty(ref muzUPath, value);
                                         data.MuzUPath = value;}}

        private MuzUProject muzUProject;
        internal MuzUProject MuzUProject { get => muzUProject; set{
                muzUProject = value;
            } }

        private double bPM;
        public double BPM { get => bPM; set {
                SetProperty(ref bPM, value);
                data.BPM = value;
                SecondsBeatConverter.BPM = value;
                IMainPage.BPMChanged();
            }}

        internal async Task<bool> OpenMuzUFilePicker()
        {
            var picker = new Windows.Storage.Pickers.FileOpenPicker();
            picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.DocumentsLibrary;
            picker.FileTypeFilter.Add(".muzu");
            try
            {
                StorageFile file = await picker.PickSingleFileAsync();
                if(file == null) return false;
                if (await LoadMuzUProject(file))
                {
                    MuzUPath = file.Path;
                    BPM = MuzUProject.BPM;
                    musicVM.SetMusicData(MuzUProject.data.MusicPath, MuzUProject.data.MusicAllign_μs / 1000000.0);
                }
                StorageApplicationPermissions.FutureAccessList.Add(file);
            }
            catch (Exception) { }
            return false;
        }

        private async Task<bool> LoadMuzUProject(IStorageFile file)
        {
            if (file == null) { return false; }
            using (var stream = await file.OpenAsync(FileAccessMode.Read))
            {
                MuzUProject = new MuzUProject(stream.AsStream());
                return true;
            }
        }
    }
}
