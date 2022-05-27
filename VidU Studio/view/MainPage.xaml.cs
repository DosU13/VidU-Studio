using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Storage;
using System.Threading.Tasks;
using Windows.Storage.Streams;
using Windows.UI.Core.Preview;
using Windows.Storage.AccessCache;
using VidU;
using MuzU;
using Windows.Media.Core;
using VidU_Studio.model;
using System.ComponentModel;
using VidU_Studio.view;
using VidU_Studio.util;
using VidU_Studio.viewmodel;
using VidU.data;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace VidU_Studio
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page, TimingCreator
    {
        public MainPage()
        {
            this.InitializeComponent();

            _ = LoadLocalSettingsAsync();
            SystemNavigationManagerPreview.GetForCurrentView().CloseRequested += OnWindowClose;
        }

        public VidUProject _project;
        public VidUProject Project
        {
            get => _project;
            set
            {
                _project = value;
                Task.Run(async () => LoadMuzUProject(await StorageFile.GetFileFromPathAsync(value.data.MuzUPath)));
                CompositionModel = new CompositionModel(_project.data.Clips);
                CompositionModel.mediaPlayerElement = mediaPlayerElement;
                Task.Run(async () => CompositionModel.BackMusic = await StorageFile.GetFileFromPathAsync(value.data.MusicPath));
                Storyboard.CompositionModel = CompositionModel;
                Bindings.Update();
            }
        }

        private IStorageFile projectFile = null;
        private bool existProject => Project != null;
        private bool existProjectFile => projectFile != null;
        public MuzUProject MuzU;
        private CompositionModel _compositionModel;
        private CompositionModel CompositionModel { get => _compositionModel; set {
                _compositionModel = value; _compositionModel.PropertyChanged += _compositionModel_PropertyChanged; ;
            } }

        private void _compositionModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (GroupMediaView.GroupClipVM != CompositionModel.SelectedGroupClip)
                GroupMediaView.GroupClipVM = CompositionModel.SelectedGroupClip;
            Bindings.Update();
        }

        private ContentDialog dialog = null;

        private async void MusicMuzU_Click(object sender, RoutedEventArgs e)
        {
            dialog = MusicMuzUContentDialog;
            await MusicMuzUContentDialog.ShowAsync();
        }

        private async void SelectMuzU_Click(object sender, RoutedEventArgs e)
        {
            await OpenMuzUFilePicker();
        }

        private async void SelectMusic_Click(object sender, RoutedEventArgs e)
        {
            await OpenMusicFilePicker();
        }

        private async void BPM_Click(object sender, RoutedEventArgs e)
        {
            dialog = BPMContentDialog;
            await BPMContentDialog.ShowAsync();
        }

        private async void FinishVideo_Click(object sender, RoutedEventArgs e)
        {
        }

        private async void NewEmpty_Click(object sender, RoutedEventArgs e)
        {
            if (existProject) if (!(await SaveWorkDialog())) return;
            Project = new VidUProject();
            projectFile = null;
        }

        private async void Open_Click(object sender, RoutedEventArgs e)
        {
            if (existProject) if (!(await SaveWorkDialog())) return;
            await LoadWithFilePicker();
        }

        private async void Save_Click(object sender, RoutedEventArgs e)
        {
            if (projectFile != null) await SaveToFile(projectFile);
            else await SaveWithFilePicker();
        }

        private async void SaveAs_Click(object sender, RoutedEventArgs e)
        {
            await SaveWithFilePicker();
        }

        private async Task<bool> SaveWorkDialog()
        {
            if(dialog != null) dialog.Hide();
            dialog = new ContentDialog();
            dialog.Title = "Save your work?";
            dialog.PrimaryButtonText = "Save";
            dialog.SecondaryButtonText = "Don't Save";
            dialog.CloseButtonText = "Cancel";
            dialog.DefaultButton = ContentDialogButton.Primary;

            var result = await dialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                if (projectFile != null) return await SaveToFile(projectFile);
                else return await SaveWithFilePicker();
            }
            else if (result == ContentDialogResult.Secondary) return true;
            else if (result == ContentDialogResult.None) return false;
            return false;
        }

        private async Task<bool> SaveWithFilePicker()
        {
            var picker = new Windows.Storage.Pickers.FileSavePicker();
            picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.DocumentsLibrary;
            picker.FileTypeChoices.Add("VidU file", new List<string>() { ".vidu" });
            picker.SuggestedFileName = nameof(Project); // project name
            try
            {
                StorageFile file = await picker.PickSaveFileAsync();
                if (file != null)
                {
                    return await SaveToFile(file);
                }
            }
            catch (Exception ex)
            { Debug.WriteLine(ex); }
            return false;
        }

        private async Task<bool> SaveToFile(IStorageFile file)
        {
            using (var stream = await file.OpenStreamForWriteAsync())
            {
                Project.Save(stream);
                stream.Close();
                if (projectFile == null) projectFile = file;
                return true;
            }
        }

        private async Task<bool> LoadWithFilePicker()
        {
            var picker = new Windows.Storage.Pickers.FileOpenPicker();
            picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.DocumentsLibrary;
            picker.FileTypeFilter.Add(".vidu");
            try
            {
                StorageFile file = await picker.PickSingleFileAsync();
                return await LoadFromFile(file);
            }
            catch (Exception ex)
            {
                Console.WriteLine("HERE: -->>" + ex.ToString());
            }
            return false;
        }

        private async Task<bool> OpenMuzUFilePicker()
        {
            var picker = new Windows.Storage.Pickers.FileOpenPicker();
            picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.DocumentsLibrary;
            picker.FileTypeFilter.Add(".muzu");
            try
            {
                StorageFile file = await picker.PickSingleFileAsync();
                if(await LoadMuzUProject(file))
                {
                    Project.data.MuzUPath = file.Path;
                    Project.data.MusicPath = MuzU.data.MusicPath;
                    Project.data.MusicAllignSec = MuzU.data.MusicAllign_μs / 1000000.0;
                    Project.data.BPM = MuzU.BPM;
                    Project.data.BeatsPerBar = MuzU.BeatsPerBar;
                }
                StorageApplicationPermissions.FutureAccessList.Add(file);
            }
            catch (Exception ex)
            {
                Console.WriteLine("HERE: -->>" + ex.ToString());
            }
            return false;
        }

        private async Task<bool> OpenMusicFilePicker()
        {
            var picker = new Windows.Storage.Pickers.FileOpenPicker();
            picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.MusicLibrary;
            picker.FileTypeFilter.Add(".mp3");
            picker.FileTypeFilter.Add(".wav");
            try
            {
                StorageFile file = await picker.PickSingleFileAsync();
                Project.data.MusicPath = file.Path;
                CompositionModel.BackMusic = file;
                StorageApplicationPermissions.FutureAccessList.Add(file);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("HERE: -->>" + ex.ToString());
            }
            return false;
        }

        private async Task<bool> LoadFromFile(IStorageFile file)
        {
            if (file == null) return false;   
            using (var stream = await file.OpenAsync(FileAccessMode.Read))
            {
                Project = new VidUProject(stream.AsStream());
                projectFile = file;
                return true;
            }
        }

        private async Task<bool> LoadMuzUProject(IStorageFile file)
        {
            if (file == null) return false;
            using (var stream = await file.OpenAsync(FileAccessMode.Read))
            {
                MuzU = new MuzUProject(stream.AsStream());
                Storyboard.TimingCreator = this;
                Bindings.Update();
                return true;
            }
            throw new NotImplementedException();
        }

        async Task TimingCreator.AddGroupDialog()
        {
            dialog = new AddGroupDialog(MuzU);
            if(await dialog.ShowAsync() == ContentDialogResult.Primary)
            {
                CompositionModel.Clips.Add(new GroupClipViewModel((dialog as AddGroupDialog).Result));
            }
        }

        private async void OnWindowClose(object sender, SystemNavigationCloseRequestedPreviewEventArgs args)
        {
            args.Handled = true;
            if (existProject) if (!(await SaveWorkDialog())) return;
            SaveLocalSettings();
            App.Current.Exit();
        }

        private void SaveLocalSettings()
        {
            if (projectFile == null) ApplicationData.Current.LocalSettings.Values["ProjectFileFutureAccessToken"] = null;
            else
            {
                string faToken = StorageApplicationPermissions.FutureAccessList.Add(projectFile);
                ApplicationData.Current.LocalSettings.Values["ProjectFileFutureAccessToken"] = faToken;
            }
        }

        private async Task LoadLocalSettingsAsync()
        {
            if (ApplicationData.Current.LocalSettings.Values.ContainsKey("ProjectFileFutureAccessToken"))
            {
                string faToken = ApplicationData.Current.LocalSettings.Values["ProjectFileFutureAccessToken"].ToString();
                IStorageFile _projectFile = await StorageApplicationPermissions.FutureAccessList.GetFileAsync(faToken);
                await LoadFromFile(_projectFile);
            }
        }
    }
}
