using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VidU.data;
using VidU_Studio.util;
using Windows.Foundation.Collections;
using Windows.Media.Editing;
using Windows.Media.Effects;
using Windows.Storage;
using Windows.UI;
using Windows.UI.Popups;
using Windows.UI.Xaml.Controls;

namespace VidU_Studio.viewmodel
{
    public class GroupClipViewModel : ClipViewModel
    {
        internal GroupClipViewModel(BaseClip groupClip, IStoryBoard storyBoard): 
            base(groupClip, storyBoard)
        {
            if (Data is SequencerClip)
               Values = (Data as SequencerClip).TimingsWithValues.Dict.Select(it => 
                        new StringViewModel(it.Value)).ToList();
            else Values = (Data as AutoSequencerClip).Values.Select(it=>
                    new StringViewModel(it)).ToList();
            InitMedias();
        }

        private bool MediasReady = false;
        private async void InitMedias()
        {
            foreach (var m in DataMedias)
            {
                Medias.Add(await MediaViewModel.Create(m));
            }
            OnPropertyChanged(nameof(Thumbnail));
            Medias.CollectionChanged += Medias_CollectionChanged;
            MediasReady = true;
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
            if (!CompositionFreeze) StoryBoard.UpdateComposition();
            Debug.WriteLine("GroupClipViewModel Col Changed");
        }

        private bool CompositionFreeze = false;
        internal async void AddFilesByName(List<StorageFile> files)
        {
            List<string> notFound = new List<string>();
            var values = Values.Select(it => it.Str).ToList();
            CompositionFreeze = true;
            while (Medias.Count < Values.Count) Medias.Add(new MediaViewModel());
            int wordLen = 0;
            for(int i=0; i<values.Count(); i++)
            {
                wordLen++;
                if (values[i].Contains('_')||i==values.Count-1)
                {
                    var word = values.GetRange(i+1 - wordLen, wordLen).Aggregate((a,b)=>a+b);
                    word = word.Replace("_", "").ToLower();
                    var f = files.Find(it => Path.GetFileNameWithoutExtension(it.DisplayName)
                                        .ToLower() == word);
                    if (f != null)
                    {
                        for (int j = 0; j < wordLen; j++)
                        {
                            Medias.RemoveAt(i + 1 - wordLen+j);
                            Medias.Insert(i + 1 - wordLen+j, new MediaViewModel(f));
                        }
                    }else notFound.Add(word);
                    wordLen = 0;
                }
            }
            if (notFound.Count != 0) {
                ContentDialog dialog = new ContentDialog();
                dialog.Title = notFound.Count + " words not found: ";
                TextBlock txt = new TextBlock();
                txt.Text = String.Join("; ", notFound.Distinct());
                txt.IsTextSelectionEnabled = true;
                txt.TextWrapping = Windows.UI.Xaml.TextWrapping.WrapWholeWords;
                dialog.Content = txt;
                dialog.CloseButtonText = "OK";
                await dialog.ShowAsync();
            }
            CompositionFreeze = false;
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
            if (!MediasReady) return null;
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
                if (IsLyrics) UpdateRotationEffect(result);
                return result;
            }
            return new List<MediaClip>{MediaClip
                .CreateFromColor(Color.FromArgb(255,0,0,0), TimeSpan.FromSeconds(Duration)) };
        }

        public bool IsLyrics { get { return !double.TryParse
                    (Values.Find(it => it.Str != "").Str, out double _); } }

        private void UpdateRotationEffect(List<MediaClip> result)
        {
            int side = 1;
            for(int i = 1; i<Values.Count; i++)
            {
                if (!Values[i - 1].Str.Contains('_'))
                {
                    var lyrRot = new VideoEffectDefinition("VidUVideoEffects.LyricsRotation",
                        new PropertySet { { "side", side } });
                    result[i].VideoEffectDefinitions.Add(lyrRot);
                    side *= -1;
                }
            }
        }
    }
}
