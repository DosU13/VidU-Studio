using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using VidU;
using VidU.data;
using VidU_Studio.model;
using VidU_Studio.util;
using VidU_Studio.viewmodel;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.Editing;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

namespace VidU_Studio.view
{
    internal sealed partial class Storyboard : UserControl
    {
        public Storyboard()
        {
            this.InitializeComponent();
        }

        public StoryBoardViewModel storyBoardVM;
        public StoryBoardViewModel StoryBoardVM
        {
            get => storyBoardVM;
            set
            {
                storyBoardVM = value;
                try { Bindings.Update(); } catch (Exception) { };
            }
        }
        internal ITimingCreator TimingCreator;

        private async void AddSingle_Click(object sender, RoutedEventArgs e)
        {
            var file = await PickFile();
            if (file == null) return;
            StoryBoardVM.AddSingle(file, SecondsBeatConverter.ConvertBack(SingleClipDurTxtBox.Text));
            Bindings.Update();
        }

        private async void Replace_Click(object sender, RoutedEventArgs e)
        {
            var file = await PickFile();
            if (file == null) return;
            StoryBoardVM.SelectedSingleClip.File = file;
            Bindings.Update();
        }

        private async void AddGroup_Click(object sender, RoutedEventArgs e)
        {
            await TimingCreator.AddGroupDialog();
            //IReadOnlyList<StorageFile> files = await PickFilesCollection();
            //if(files==null || files.Count==0) return;
            //CompositionModel.AddGroup(files.ToList());
        }

        private async Task<StorageFile> PickFile()
        {
            var picker = new FileOpenPicker();
            picker.SuggestedStartLocation = PickerLocationId.VideosLibrary;
            foreach (var t in Util.SupportedFileTypes) picker.FileTypeFilter.Add(t);
            StorageFile pickedFile = await picker.PickSingleFileAsync();
            if (pickedFile == null) return null;

            // These files could be picked from a location that we won't have access to later
            var storageItemAccessList = Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList;
            storageItemAccessList.Add(pickedFile);
            return pickedFile;
        }

        private async Task<IReadOnlyList<StorageFile>> PickFilesCollection()
        {
            var picker = new FileOpenPicker();
            picker.SuggestedStartLocation = PickerLocationId.VideosLibrary;
            foreach (var t in Util.SupportedFileTypes) picker.FileTypeFilter.Add(t);
            IReadOnlyList<StorageFile> pickedFiles = await picker.PickMultipleFilesAsync();
            if (pickedFiles == null) return null;
            if(pickedFiles.Count == 0) return null;

            // These files could be picked from a location that we won't have access to later
            var storageItemAccessList = Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList;
            foreach(var f in pickedFiles) storageItemAccessList.Add(f);
            return pickedFiles;
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            StoryBoardVM.Clips.RemoveAt(TimelineGrid.SelectedIndex);
        }

        private void TimelineGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            StoryBoardVM.SelectedClip = TimelineGrid.SelectedItem as ClipViewModel;
            Bindings.Update();
        }

        internal void BindingsUpdate() => Bindings.Update();
    }
}
