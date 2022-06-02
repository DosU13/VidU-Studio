using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
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
    public class GroupClipViewModel : ClipViewModel
    {
        internal GroupClipViewModel(BaseClip groupClip, IStoryBoard storyBoard): 
            base(groupClip, storyBoard)
        {
            foreach (var m in DataMedias) Medias.Add(new MediaViewModel(m));
            if (Data is SequencerClip)
                if ((Data as SequencerClip).TimingsWithValues.IsValueInteger)
                    Values = (Data as SequencerClip).TimingsWithValues.Dict.Select(it => 
                        new StringViewModel(Convert.ToInt32(it.Value).ToString())).ToList();
                else Values = (Data as SequencerClip).TimingsWithValues.Dict.Select(it => 
                        new StringViewModel(it.Value.ToString())).ToList();
            else if ((Data as AutoSequencerClip).IsValueInteger)
                Values = (Data as AutoSequencerClip).Values.Select(it=>
                    new StringViewModel(Convert.ToInt32(it).ToString())).ToList();
            else Values = (Data as AutoSequencerClip).Values.Select(it=>
                    new StringViewModel(it.ToString())).ToList();
            OnPropertyChanged(nameof(Thumbnail));
            Medias.CollectionChanged += Medias_CollectionChanged;
        }

        public List<Media> DataMedias
        {
            get
            {
                if (Data is AutoSequencerClip) return (Data as AutoSequencerClip).Medias;
                else if (Data is SequencerClip) return (Data as SequencerClip).Medias;
                else return null;
            }
        }

        internal List<StringViewModel> Values;
        internal ObservableCollection<MediaViewModel> Medias = new ObservableCollection<MediaViewModel>();

        private void Medias_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    for(int i = e.NewItems.Count - 1; i>=0; i--)
                        DataMedias.Insert(e.NewStartingIndex, (e.NewItems[i] as MediaViewModel).data);
                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach (var m in e.OldItems) DataMedias.Remove((m as MediaViewModel).data);
                    break;
                default:
                    Debug.WriteLine("LibraryVM collection change not fully implemented");
                    break;
            }
            OnPropertyChanged(nameof(Thumbnail));
            StoryBoard.UpdateComposition();
        }

        public void AddFiles(IReadOnlyList<StorageFile> files)
        {
            foreach(var f in files)
            {
                var m = new MediaViewModel(f);
                Medias.Add(m);
            }
        }

        internal override StorageFile Thumbnail { get {
                if(Medias==null) return null;
                if(Medias.Count==0) return null;
                return Medias[0].File;
            }}

        internal override async Task<List<MediaClip>> CreateMediaClipsAsync()
        {
            if(Data is AutoSequencerClip)
            {
                var auto = Data as AutoSequencerClip;
                var result = new List<MediaClip>();
                MediaViewModel media; double dur;
                for(int i = 0; i<auto.TimingsWithIndices.Dict.Count-1;i++)
                {
                    media = Medias[(int)auto.TimingsWithIndices.Dict[i].Value];
                    dur = auto.TimingsWithIndices.Dict[i+1].Key - auto.TimingsWithIndices.Dict[i].Key;
                    result.Add(await media.CreateMediaClip(dur));
                }
                media = Medias[(int)auto.TimingsWithIndices.Dict.Last().Value];
                dur = EndTime - auto.TimingsWithIndices.Dict.Last().Key;
                result.Add(await media.CreateMediaClip(dur));
                return result;
            }else if(Data is SequencerClip)
            {
                var seq = Data as SequencerClip;
                var result = new List<MediaClip>();
                MediaViewModel media; double dur;
                for (int i = 0; i < seq.TimingsWithValues.Dict.Count - 1; i++)
                {
                    media = Medias[i];
                    dur = seq.TimingsWithValues.Dict[i + 1].Key - seq.TimingsWithValues.Dict[i].Key;
                    result.Add(await media.CreateMediaClip(dur));
                }
                media = Medias[seq.TimingsWithValues.Dict.Count-1];
                dur = EndTime - seq.TimingsWithValues.Dict.Last().Key;
                result.Add(await media.CreateMediaClip(dur));
                return result;
            }
            return new List<MediaClip>{MediaClip
                .CreateFromColor(Color.FromArgb(255,0,0,0), TimeSpan.FromSeconds(Duration)) };
        }
    }
}
