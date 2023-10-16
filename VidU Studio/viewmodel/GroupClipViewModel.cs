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
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace VidU_Studio.viewmodel
{
    public class GroupClipViewModel : ClipViewModel
    {
        internal GroupClipViewModel(BaseClip groupClip, IStoryBoard storyBoard): 
            base(groupClip, storyBoard)
        {
            if (Data is SequencerClip sequencerClip)
               Values = (Data as SequencerClip).TimingsWithValues.Dict.Select(it => 
                        new StringViewModel(it.Value)).ToList();
            else Values = (Data as AutoSequencerClip).Values.Select(it=>
                    new StringViewModel(it)).ToList();
            InitMedias();
        }

        private bool InitMediasCompleted = false;
        private async void InitMedias()
        {
            foreach (var media in DataMedias)
            {
                Medias.Add(await MediaViewModel.Create(media));
            }
            OnPropertyChanged(nameof(Thumbnail));
            Medias.CollectionChanged += Medias_CollectionChanged;
            InitMediasCompleted = true;
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
            Dictionary<string, StorageFile> filesDict = 
                files.ToDictionary(x => Path.GetFileNameWithoutExtension(x.DisplayName));
            List<string> notFound = new List<string>();
            var values = Values.Select(it => it.Str).ToList();
            CompositionFreeze = true;
            while (Medias.Count < Values.Count) Medias.Add(new MediaViewModel());
            for(int i=0; i<values.Count(); i++)
            {
                if (filesDict.TryGetValue(values[i], out StorageFile file)){
                    Medias.RemoveAt(i);
                    Medias.Insert(i, new MediaViewModel(file));
                }
                else
                {
                    notFound.Add(values[i]);
                }
            }
            if (notFound.Count != 0) {
                
                await ListWords(notFound).ShowAsync();
            }
            CompositionFreeze = false;
            StoryBoard.UpdateComposition();
        }

        private ContentDialog ListWords(List<string> words)
        {
            ContentDialog dialog = new ContentDialog();
            dialog.Title = words.Count + " words not found: ";

            StackPanel contentPanel = new StackPanel();

            TextBlock txt = new TextBlock
            {
                Text = string.Join("; ", words.Distinct()),
                IsTextSelectionEnabled = true,
                TextWrapping = TextWrapping.WrapWholeWords
            };

            contentPanel.Children.Add(txt);

            Button openImagesButton = new Button();
            openImagesButton.Content = "Open Google Images";
            openImagesButton.Click += (e, arg) => 
                OpenImagesOnGoogle(words.Where(x => !x.Contains('_')).ToList()); 

            contentPanel.Children.Add(openImagesButton); 

            dialog.Content = contentPanel; 

            dialog.CloseButtonText = "OK";
            return dialog;
        }

        private async void OpenImagesOnGoogle(List<string> words)
        {
            foreach (string word in words)
            {
                string googleImagesURL = $"https://www.google.com/search?q={Uri.EscapeDataString(word)}&tbm=isch&tbs=isz:l";
                Uri uri = new Uri(googleImagesURL);
                await Windows.System.Launcher.LaunchUriAsync(uri);
            }
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
            if (!InitMediasCompleted)
            {
                return null;
            }
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

        public bool IsLyrics { get { return !double.TryParse
                    (Values.Find(it => it.Str != "").Str, out double _); } }
    }
}
