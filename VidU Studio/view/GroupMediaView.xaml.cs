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
    public sealed partial class GroupMediaView : UserControl
    {
        public GroupMediaView()
        {
            this.InitializeComponent();
        }

        public GroupClipViewModel groupClipVM;
        public GroupClipViewModel GroupClipVM
        {
            get => groupClipVM; set
            {
                groupClipVM = value;
                if (groupClipVM == null) return;
                groupClipVM.PropertyChanged += (object sender, System.ComponentModel.PropertyChangedEventArgs e) => Bindings.Update();
                Bindings.Update();
            }
        }

        //{
        //    get { return (GroupClipViewModel)GetValue(GroupClipVMProperty); }
        //    set { SetValue(GroupClipVMProperty, value); Bindings.Update(); }
        //}
        //public static readonly DependencyProperty GroupClipVMProperty =
        //    DependencyProperty.Register(nameof(GroupClipVM), typeof(GroupClipViewModel), typeof(ClipThumbnail), null);

        private async void Add_Click(object sender, RoutedEventArgs e)
        {
            var files = await PickFiles();
            if (files == null) return;
            if (files.Count == 0) return;
            GroupClipVM.AddFiles(files);
            Bindings.Update();
        }

        private async void Replace_Click(object sender, RoutedEventArgs e)
        {
            var file = await PickFile();
            if (file == null) return;
            int ind = MediasGrid.SelectedIndex;
            GroupClipVM.Medias.RemoveAt(ind);
            GroupClipVM.Medias.Insert(ind, new MediaViewModel(file));
            Bindings.Update();
        }

        private async Task<IReadOnlyList<StorageFile>> PickFiles()
        {
            var picker = new FileOpenPicker();
            picker.SuggestedStartLocation = PickerLocationId.VideosLibrary;
            foreach(var t in Util.SupportedFileTypes) picker.FileTypeFilter.Add(t);
            var pickedFiles = await picker.PickMultipleFilesAsync();

            // These files could be picked from a location that we won't have access to later
            var storageItemAccessList = Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList;
            foreach(var f in pickedFiles) storageItemAccessList.Add(f);
            return pickedFiles;
        }

        private async Task<StorageFile> PickFile()
        {
            var picker = new FileOpenPicker();
            picker.SuggestedStartLocation = PickerLocationId.VideosLibrary;
            foreach (var t in Util.SupportedFileTypes) picker.FileTypeFilter.Add(t);
            var pickedFile = await picker.PickSingleFileAsync();

            // These files could be picked from a location that we won't have access to later
            var storageItemAccessList = Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList;
            storageItemAccessList.Add(pickedFile);
            return pickedFile;
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            List<MediaViewModel> selectedMediaVM = new List<MediaViewModel>();
            foreach(var s in MediasGrid.SelectedItems) selectedMediaVM.Add(s as MediaViewModel);
            foreach(var m in selectedMediaVM) GroupClipVM.Medias.Remove(m);
        }

        private Visibility VisibilityIfSelected
        {
            get
            {
                if (MediasGrid.SelectedIndex == -1) return Visibility.Collapsed;
                else return Visibility.Visible;
            }
        }

        private Visibility VisibilityIfOne
        {
            get
            {
                if (MediasGrid.SelectedIndex != -1 && (MediasGrid.SelectedItems.Count == 1)) return Visibility.Visible;
                else return Visibility.Collapsed;
            }
        }

        private Visibility VisibilityIfVideo
        {
            get
            {
                if (MediasGrid.SelectedIndex != -1 && (MediasGrid.SelectedItems.Count == 1)
                        && !(MediasGrid.SelectedItem as MediaViewModel).IsImage) return Visibility.Visible;
                else return Visibility.Collapsed;
            }
        }

        private void OnSelectedChanged(object sender, SelectionChangedEventArgs e)
        {
            //double[] durs = compositionCrtl.durations;
            //double sum = 0;
            //for (int i = 0; i < selectedInd; i++)
            //{
            //    if (i < durs.Length) sum += durs[i];
            //    else sum += compositionCrtl.defaultDur;
            //}
            //try
            //{
            //    compositionCrtl.mediaPlayerElement.MediaPlayer.PlaybackSession.Position = TimeSpan.FromSeconds(sum);
            //}
            //catch (Exception ex) { compositionCrtl.status.Text = "Exception seeking: " + ex.Message; }
            Bindings.Update();
        }
    }
}
