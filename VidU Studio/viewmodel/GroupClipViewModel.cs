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
using Windows.Storage;

namespace VidU_Studio.viewmodel
{
    public class GroupClipViewModel : ClipViewModel
    {
        public BaseClip data = null;
        public BaseClip Data { get => data; set => data = value; }
        public List<Media> DataMedias { get
            {
                if (Data is AutoSequencerClip) return (Data as AutoSequencerClip).Medias;
                else if (Data is SequencerClip) return (Data as SequencerClip).Medias;
                else return null;
            } }

        public GroupClipViewModel(BaseClip groupClip)
        {
            Data = groupClip;
            var mediasIt = new ObservableCollection<MediaViewModel>();
            foreach (var m in DataMedias) mediasIt.Add(new MediaViewModel(m));
            medias = mediasIt;
            if (Data is SequencerClip)
                if ((Data as SequencerClip).TimingsWithValues.IsValueInteger)
                    values = (Data as SequencerClip).TimingsWithValues.Dict.Select(it => 
                        new StringViewModel(Convert.ToInt32(it.Value).ToString())).ToList();
                else values = (Data as SequencerClip).TimingsWithValues.Dict.Select(it => 
                    new StringViewModel(it.Value.ToString())).ToList();
            else if ((Data as AutoSequencerClip).IsValueInteger)
                values = (Data as AutoSequencerClip).Values.Select(it=>
                    new StringViewModel(Convert.ToInt32(it).ToString())).ToList();
            else values = (Data as AutoSequencerClip).Values.Select(it=>
                    new StringViewModel(it.ToString())).ToList();
            RaisePropertyChanged(nameof(Values));
            medias.CollectionChanged += Medias_CollectionChanged;
        }

        //internal GroupClipViewModel(List<StorageFile> storageFiles)
        //{
        //    Data = new SequencerClip();
        //    medias = new ObservableCollection<MediaViewModel>();
        //    foreach(var f in storageFiles) medias.Add(new MediaViewModel(f));
        //    foreach(var m in medias) DataMedias.Add(m.data);
        //    medias.CollectionChanged += Medias_CollectionChanged;
        //}

        private List<StringViewModel> values = null;
        internal List<StringViewModel> Values => values;
        private ObservableCollection<MediaViewModel> medias;
        internal ObservableCollection<MediaViewModel> Medias { get => medias; set
            {
                medias = value;
                RaisePropertyChanged(nameof(Medias));
            } }

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
        }

        public void AddFiles(IReadOnlyList<StorageFile> files)
        {
            foreach(var f in files)
            {
                var m = new MediaViewModel(f);
                Medias.Add(m);
            }
        }

        public StorageFile Thumbnail { get {
                if(Medias==null) return null;
                if(Medias.Count==0) return null;
                return Medias[0].File;
            }}

        public double Duration
        {
            get => Data.StartPos;
            set
            {
                Data.StartPos = value;
                RaisePropertyChanged(nameof(Duration));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }
    }
}
