using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VidU.data;
using VidU_Studio.util;
using Windows.Media.Editing;
using Windows.Storage;
using Windows.UI;

namespace VidU_Studio.viewmodel
{
    public class SingleClipViewModel: ClipViewModel
    {
        internal SingleClipViewModel(SingleClip singleClip, IStoryBoard storyBoard): 
            base(singleClip, storyBoard)
        {
            Task.Run(async () => File = await StorageFile.GetFileFromPathAsync(singleClip.Media.Path));
            if(singleClip.Media is Video)
            {
                volume = (singleClip.Media as Video).Volume;
                trimStart = (singleClip.Media as Video).TrimFromStart;
            }
        }
        internal SingleClip Data_ => Data as SingleClip;

        private StorageFile file = null;
        internal StorageFile File { get => file;
            set {
                if (value == null) return;
                SetProperty(ref file, value);
                if (value.IsVideo()) { if (Data_.Media is Image) Data_.Media = new Video(); { Volume = volume; trimStart = 0; } }
                else if (Data_.Media is Video) Data_.Media = new Image();
                Data_.Media.Path = value.Path;
                StoryBoard.UpdateComposition();
            }}

        internal override StorageFile Thumbnail => file;

        public bool IsImage => Data_.Media is Image;

        private int volume;
        public int Volume
        {
            get => volume;
            set
            {
                SetProperty(ref volume, value);
                if (Data_.Media is Video) (Data_.Media as Video).Volume = value;
                StoryBoard.UpdateComposition();
            }
        }

        private double trimStart;
        public double TrimStart
        {
            get => trimStart;
            set
            {
                SetProperty(ref trimStart, value);
                if (Data_.Media is Video) (Data_.Media as Video).TrimFromStart = value;
                StoryBoard.UpdateComposition();
            }
        }

        internal override async Task<List<MediaClip>> CreateMediaClipsAsync()
        {
            MediaClip mediaClip;
            try
            {
                if (File == null) mediaClip = MediaClip.CreateFromColor(Color.FromArgb(255, 0, 0, 0), TimeSpan.FromSeconds(Duration));
                else if (IsImage) mediaClip = await MediaClip.CreateFromImageFileAsync(File, TimeSpan.FromSeconds(Data_.Duration));
                else
                {
                    mediaClip = await MediaClip.CreateFromFileAsync(File);
                    mediaClip.TrimTimeFromStart = TimeSpan.FromSeconds(TrimStart);
                    mediaClip.TrimTimeFromEnd = mediaClip.OriginalDuration - mediaClip.TrimTimeFromStart - TimeSpan.FromSeconds(Duration);
                    mediaClip.Volume = 2 * (Volume / 100.0);
                }
            }catch (Exception) { mediaClip = MediaClip.CreateFromColor(
                Color.FromArgb(255, 0, 0, 0), TimeSpan.FromSeconds(Duration));}
            return new List<MediaClip> { mediaClip };
        }
    }
}
