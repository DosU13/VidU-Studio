using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VidU;
using VidU_Studio.util;
using Windows.Storage;
using Windows.Storage.AccessCache;

namespace VidU_Studio.viewmodel
{
    internal class ProjectRepoViewModel : BindableBase
    {
        private IMainPage mainPage;
        public ProjectRepoViewModel(IMainPage MainPage)
        {
            mainPage = MainPage;
            _ = LoadProjectFileLocalSettings();
        }

        private VidUProject project;
        private VidUProject Project { get => project; set { SetProperty(ref project, value); 
                OnPropertyChanged(nameof(ExistProject)); mainPage.ProjectChanged(value); } }

        private StorageFile projectFile;
        private StorageFile ProjectFile { get => projectFile; set { SetProperty(ref projectFile, value);
                OnPropertyChanged(nameof(ExistProjectFile)); SaveLocalSettings(); } }

        public bool ExistProject { get => project != null; }
        public bool ExistProjectFile { get => projectFile != null; } // I didn't understand Binding quit well, save button doesn't enabling, it didn't get notification

        internal async Task Save()
        {
            if (ExistProjectFile) await SaveToFile(ProjectFile);
            else await SaveAs();
        }

        internal async Task SaveAs()
        {
            var picker = new Windows.Storage.Pickers.FileSavePicker();
            picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.DocumentsLibrary;
            picker.FileTypeChoices.Add("VidU file", new List<string>() { ".vidu" });
            picker.SuggestedFileName = nameof(Project); 
            try
            {
                StorageFile file = await picker.PickSaveFileAsync();
                if (file != null)
                {
                    await SaveToFile(file);
                }
            }
            catch (Exception) { }
        }

        private async Task<bool> SaveToFile(StorageFile file)
        {
            using (var stream = await file.OpenStreamForWriteAsync())
            {
                Project.Save(stream);
                stream.Close();
                if (ProjectFile == null) ProjectFile = file;
                return true;
            }
        }

        internal async Task Open()
        {
            if(!(await mainPage.SaveWorkDialog())) return;
            var picker = new Windows.Storage.Pickers.FileOpenPicker();
            picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.DocumentsLibrary;
            picker.FileTypeFilter.Add(".vidu");
            try
            {
                StorageFile file = await picker.PickSingleFileAsync();
                if (file == null) return;
                await LoadFromFile(file);
            }
            catch (Exception) { }
        }

        private async Task<bool> LoadFromFile(StorageFile file)
        {
            if (file == null) return false;
            using (var stream = await file.OpenAsync(FileAccessMode.Read))
            {
                ProjectFile = file;
                Project = new VidUProject(stream.AsStream());
                return true;
            }
        }

        internal async Task NewEmpty()
        {
            if (!(await mainPage.SaveWorkDialog())) return;
            ProjectFile = null;
            Project = new VidUProject();
            Debug.WriteLine("New Empty Pressed in ProjectRepoVM end");
        }

        private void SaveLocalSettings()
        {
            if (ProjectFile == null) ApplicationData.Current.LocalSettings.Values["ProjectFileFutureAccessToken"] = null;
            else
            {
                string faToken = StorageApplicationPermissions.FutureAccessList.Add(ProjectFile);
                ApplicationData.Current.LocalSettings.Values["ProjectFileFutureAccessToken"] = faToken;
            }
        }

        private async Task LoadProjectFileLocalSettings()
        {
            if (ApplicationData.Current.LocalSettings.Values.ContainsKey("ProjectFileFutureAccessToken"))
            {
                string faToken = ApplicationData.Current.LocalSettings.Values["ProjectFileFutureAccessToken"].ToString();
                StorageFile file = await StorageApplicationPermissions.FutureAccessList.GetFileAsync(faToken);
                if (file == null) return;
                await LoadFromFile(file);
            }
        }
    }
}
