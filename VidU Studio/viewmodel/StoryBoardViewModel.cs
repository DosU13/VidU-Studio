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
using VidU_Studio.view;
using VidU_Studio.viewmodel;
using Windows.Media.Core;
using Windows.Media.Editing;
using Windows.Storage;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace VidU_Studio.model
{
    public class StoryBoardViewModel: BindableBase, IStoryBoard
    {
        private List<BaseClip> data;
        private CompositionModel CompModel;
        private GroupMediaView GroupMediaView;
        private EffectsView EffectsView;
        internal StoryBoardViewModel(List<BaseClip> data, CompositionModel compositionModel,
            GroupMediaView groupMediaView, EffectsView effectsView)
        {
            this.data = data;
            CompModel = compositionModel;
            GroupMediaView = groupMediaView;
            EffectsView = effectsView;
            Clips.Clear();
            CompModel.UpdateComposition();
            foreach (var cl in data)
            {
                ClipViewModel clipVM;
                if(cl is SingleClip) clipVM = new SingleClipViewModel(cl as SingleClip, this);
                else clipVM = new GroupClipViewModel(cl, this);
                Clips.Add(clipVM);
                clipVM.PropertyChanged += new PropertyChangedEventHandler(
                    (object o, PropertyChangedEventArgs p) => CompModel.UpdateComposition());
            }
            Clips.CollectionChanged += Clips_CollectionChanged;
        }

        internal ObservableCollection<ClipViewModel> Clips { get => CompModel.Clips; }

        private void Clips_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    var newItem = e.NewItems[0] as ClipViewModel;
                    data.Add(newItem.Data);
                    CompModel.UpdateComposition();
                    break;
                case NotifyCollectionChangedAction.Remove:
                    var oldItem = e.OldItems[0] as ClipViewModel;
                    data.Remove(oldItem.Data);
                    CompModel.UpdateComposition();
                    break;
                default:
                    Debug.WriteLine("Collection change not fully implemented");
                    break;
            }
        }

        internal void AddSingle(StorageFile storageFile, double duration)
        {
            var s = new SingleClip();
            if (storageFile.IsVideo()) s.Media = new Video();
            else s.Media = new VidU.data.Image();
            s.Media.Path = storageFile.Path;
            s.Duration = duration;
            Clips.Add(new SingleClipViewModel(s, this) { File = storageFile});
        }

        internal void AddGroup(BaseClip clip)
        {
            Clips.Add(new GroupClipViewModel(clip, this));
        }

        private ClipViewModel _selectedClip = null;
        internal ClipViewModel SelectedClip
        {
            get { return _selectedClip; }
            set { SetProperty(ref _selectedClip, value);
                OnPropertyChanged(nameof(VisibilityIfSelected));
                OnPropertyChanged(nameof(VisibilityIfSingle));
                OnPropertyChanged(nameof(VisibilityIfVideo));
                OnPropertyChanged(nameof(VisibilityIfGroup));
                GroupMediaView.GroupClipVM = SelectedGroupClip;
                EffectsView.Refresh();
            }
        }
        internal Visibility VisibilityIfSelected => VisibilityIf(SelectedClip != null);
        internal Visibility VisibilityIfLast => VisibilityIf(SelectedClip == Clips.LastOrDefault());
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
                if (SelectedClip is GroupClipViewModel) return SelectedClip as GroupClipViewModel;
                else return null;
            }
        }

        private Visibility VisibilityIf(bool v)
        {
            if (v) return Visibility.Visible;
            else return Visibility.Collapsed;
        }

        public double GetStartTime(ClipViewModel caller)
        {
            int index = Clips.IndexOf(caller);
            if(index < 0) return 0;
            double result = 0;
            for(int i = 0; i < index; i++)
            {
                result += Clips[i].Duration;
            }
            return result;
        }

        void IStoryBoard.UpdateComposition() => CompModel.UpdateComposition();
    }
}