using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VidU.data;
using VidU_Studio.util;
using VidU_Studio.viewmodel;
using Windows.Media.Core;
using Windows.Media.Editing;
using Windows.Storage;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace VidU_Studio.model
{
    public class CompositionModel: BindableBase
    {
        private List<BaseClip> data;

        public CompositionModel(List<BaseClip> data)
        {
            this.data = data;
            Clips = new ObservableCollection<ClipViewModel>();
            foreach (var cl in data)
            {
                ClipViewModel svm;
                if(cl is SingleClip) svm = new SingleClipViewModel(cl as SingleClip);
                else svm = new GroupClipViewModel(cl);
                Clips.Add(svm);
                svm.PropertyChanged += new PropertyChangedEventHandler(
                    (object o, PropertyChangedEventArgs p) => UpdateComposition());
            }
            Clips.CollectionChanged += Clips_CollectionChanged;
        }

        internal ObservableCollection<ClipViewModel> Clips { get; }

        private void Clips_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    var newItem = e.NewItems[0] as ClipViewModel;
                    data.Add(newItem.Data);
                    UpdateComposition();
                    break;
                case NotifyCollectionChangedAction.Remove:
                    var oldItem = e.OldItems[0] as ClipViewModel;
                    data.Remove(oldItem.Data);
                    UpdateComposition();
                    break;
                default:
                    Debug.WriteLine("LibraryVM collection change not fully implemented");
                    break;
            }
        }

        public MediaPlayerElement mediaPlayerElement;
        internal MediaComposition mediaComposition = new MediaComposition();

        private MediaStreamSource mediaStreamSource;
        public async Task UpdateMediaElementSource()
        {
            if (mediaComposition.Clips.Count == 0) mediaPlayerElement.Source = null;
            else
            {
                await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                    () =>
                    {
                        mediaStreamSource = mediaComposition.GeneratePreviewMediaStreamSource(
                            (int)mediaPlayerElement.ActualWidth,
                            (int)mediaPlayerElement.ActualHeight);
                        mediaPlayerElement.Source = MediaSource.CreateFromMediaStreamSource(mediaStreamSource);
                    });
            }
        }

        Random random = new Random();
        private async void UpdateComposition()
        {
            int id = random.Next(int.MinValue, int.MaxValue);
            currentTaskId = id;
            try
            {
                await UpdateCompositionAsync(id);
                //Debug.WriteLine("await a job"+id);
            }
            catch (OperationCanceledException) { }//Debug.WriteLine("cancel a job"+id); }
        }

        private int currentTaskId = 0;
        private async Task UpdateCompositionAsync(int id)
        {
            mediaComposition.Clips.Clear();
            foreach (var clip in Clips)
            {
                if (clip is SingleClipViewModel)
                {
                    SingleClipViewModel cl = (SingleClipViewModel)clip;
                    if (cl.File == null) return;
                    MediaClip mediaClip;
                    if (cl.IsImage) mediaClip = await MediaClip.CreateFromImageFileAsync(cl.File, TimeSpan.FromSeconds(cl.data.StartPos));
                    else
                    {
                        mediaClip = await MediaClip.CreateFromFileAsync(cl.File);
                        mediaClip.TrimTimeFromStart = TimeSpan.FromSeconds((cl.data.Media as Video).TrimFromStart);
                        mediaClip.TrimTimeFromEnd = mediaClip.OriginalDuration - mediaClip.TrimTimeFromStart - TimeSpan.FromSeconds(cl.data.StartPos);
                        mediaClip.Volume = 2 * ((cl.data.Media as Video).Volume / 100.0);
                    }
                    if (id != currentTaskId) throw new OperationCanceledException();
                    mediaComposition.Clips.Add(mediaClip);
                }
            }
            if (id != currentTaskId) throw new OperationCanceledException();
            await UpdateMediaElementSource();
        }

        private double musicAllignSec = 0;
        public double MusicAllignSec { get => musicAllignSec; set { musicAllignSec = value; UpdateMusic(); } }
        private StorageFile _backMusic;
        public StorageFile BackMusic
        {
            get => _backMusic;
            internal set
            {
                _backMusic = value;
                Task.Run(async () =>
                {
                    mediaComposition.BackgroundAudioTracks.Clear();
                    mediaComposition.BackgroundAudioTracks.Add(await BackgroundAudioTrack.CreateFromFileAsync(value));
                });
            }
        }

        private async Task UpdateMusic()
        {

        }

        internal void AddSingle(StorageFile storageFile)
        {
            var s = new SingleClip();
            if (storageFile.IsVideo()) s.Media = new VidU.data.Video();
            else s.Media = new VidU.data.Image();
            s.Media.Path = storageFile.Path;
            Clips.Add(new SingleClipViewModel(s) { File = storageFile });
        }

        //internal void AddGroup(List<StorageFile> files)
        //{
        //    Clips.Add(new GroupClipViewModel(files));
        //}

        private ClipViewModel _selectedClip = null;
        internal ClipViewModel SelectedClip
        {
            get { return _selectedClip; }
            set { SetProperty(ref _selectedClip, value); }
        }
        internal Visibility VisibilityIfSelected => VisibilityIf(SelectedClip != null);
        internal Visibility VisibilityIfSingle => VisibilityIf(SelectedClip is SingleClipViewModel);
        internal Visibility VisibilityIfGroup => VisibilityIf(SelectedClip is GroupClipViewModel);
        internal Visibility VisibilityIfVideo => VisibilityIf((SelectedClip is SingleClipViewModel)
                                                    && !(SelectedClip as SingleClipViewModel).IsImage);

        internal SingleClipViewModel SelectedSingleClip
        {
            get
            {
                if (SelectedClip is SingleClipViewModel) return SelectedClip as SingleClipViewModel;
                else return null;
            }
        }
        internal GroupClipViewModel SelectedGroupClip
        {
            get
            {
                if (SelectedClip is GroupClipViewModel)
                {
                    Debug.WriteLine(SelectedClip as GroupClipViewModel);
                    return SelectedClip as GroupClipViewModel;
                }
                else return null;
            }
        }
        private Visibility VisibilityIf(bool v)
        {
            if (v) return Visibility.Visible;
            else return Visibility.Collapsed;
        }
    }
}