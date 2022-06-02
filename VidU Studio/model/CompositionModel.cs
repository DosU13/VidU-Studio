using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using VidU.data;
using VidU_Studio.viewmodel;
using Windows.Foundation;
using Windows.Media.Core;
using Windows.Media.Editing;
using Windows.Media.MediaProperties;
using Windows.Media.Transcoding;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;

namespace VidU_Studio.model
{
    internal class CompositionModel
    {
        private MediaComposition MediaComp = new MediaComposition();

        Random random = new Random();
        internal void UpdateComposition()
        {
            int id = random.Next(int.MinValue, int.MaxValue);
            currentTaskId = id;
            _ = UpdateCompositionAsync(id);
        }

        private int currentTaskId = 0;

        internal ObservableCollection<ClipViewModel> Clips { get; set; } = new ObservableCollection<ClipViewModel>();

        private async Task UpdateCompositionAsync(int id)
        {
            MediaComp.Clips.Clear();
            foreach (var clip in Clips)
            {
                var mediaClips = await clip.CreateMediaClipsWithEffectsAsync();
                if (id != currentTaskId) throw new OperationCanceledException();
                foreach (var m in mediaClips) MediaComp.Clips.Add(m);
            }
            if (id != currentTaskId) throw new OperationCanceledException();
           await UpdateMediaElementSource();
        }

        internal async void UpdateMusic(StorageFile musicFile, double musicAllignSec)
        {
            MediaComp.BackgroundAudioTracks.Clear();
            if (musicFile == null) { UpdateComposition(); return; }
            var backgroundTrack = await BackgroundAudioTrack.CreateFromFileAsync(musicFile);
            if (musicAllignSec < 0) backgroundTrack.TrimTimeFromStart -= TimeSpan.FromSeconds(musicAllignSec);
            else backgroundTrack.Delay = TimeSpan.FromSeconds(musicAllignSec);
            MediaComp.BackgroundAudioTracks.Add(backgroundTrack);
            UpdateComposition();
        }

        private MediaStreamSource mediaStreamSource;
        internal MediaPlayerElement MediaPlayerElement { private get; set; }
        public object Dispatcher { get; private set; }

        public async Task UpdateMediaElementSource()
        {
            if (MediaComp.Clips.Count == 0) MediaPlayerElement.Source = null;
            else
            {
                await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                    () =>
                    {
                        try
                        {
                            mediaStreamSource = MediaComp.GeneratePreviewMediaStreamSource(
                                (int)MediaPlayerElement.ActualWidth, (int)MediaPlayerElement.ActualHeight);
                            MediaPlayerElement.Source = MediaSource.CreateFromMediaStreamSource(mediaStreamSource);
                        }catch(Exception) { };
                    });
            }
        }

        internal async Task<IAsyncOperationWithProgress<TranscodeFailureReason, double>> RenderVideo(int qualityIndex, bool isFastEncoding)
        {
            var picker = new FileSavePicker();
            picker.SuggestedStartLocation = PickerLocationId.VideosLibrary;
            picker.FileTypeChoices.Add("MP4 files", new List<string>() { ".mp4" });
            picker.SuggestedFileName = "New Video.mp4";

            StorageFile file = await picker.PickSaveFileAsync();
            if (file != null)
            {
                MediaTrimmingPreference preference;
                if (isFastEncoding) preference = MediaTrimmingPreference.Fast;
                else preference = MediaTrimmingPreference.Precise;
                MediaEncodingProfile profile;
                if (qualityIndex == 0) profile = MediaEncodingProfile.CreateMp4(VideoEncodingQuality.HD1080p);
                else if(qualityIndex == 1) profile = MediaEncodingProfile.CreateMp4(VideoEncodingQuality.HD720p);
                else profile = MediaEncodingProfile.CreateMp4(VideoEncodingQuality.Wvga);
                var saveOperation = MediaComp.RenderToFileAsync(file, preference, profile);
                return saveOperation;
            } return null;
        }
    }
}
