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
using VidU_Studio.model;
using VidU_Studio.util;
using Windows.Media.Editing;
using Windows.Media.Effects;
using Windows.Storage;

namespace VidU_Studio.viewmodel
{
    public abstract class ClipViewModel: BindableBase, IChildItemRemove
    {
        internal BaseClip Data { get; }
        internal abstract StorageFile Thumbnail { get; }
        private protected IStoryBoard StoryBoard;

        protected internal ClipViewModel(BaseClip data, IStoryBoard storyBoard)
        {
            Data = data;
            duration = data.Duration;
            StoryBoard = storyBoard;
            foreach (var e in data.Effects) EffectVMs.Add(
                new EffectViewModel(e, StoryBoard, StartTime, EndTime, this));
            EffectVMs.CollectionChanged += EffectVMs_CollectionChanged;
        }

        private void EffectVMs_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    var newItem = e.NewItems[0] as EffectViewModel;
                    Data.Effects.Add(newItem.Data);
                    //CompModel.UpdateComposition();
                    break;
                case NotifyCollectionChangedAction.Remove:
                    var oldItem = e.OldItems[0] as EffectViewModel;
                    Data.Effects.Remove(oldItem.Data);
                    //CompModel.UpdateComposition();
                    break;
                default:
                    Debug.WriteLine("Collection change not fully implemented");
                    break;
            }
        }

        private double duration;
        internal double Duration
        {
            get => duration;
            set
            {
                SetProperty(ref duration, value);
                Data.Duration = value;
            }
        }

        internal ObservableCollection<EffectViewModel> EffectVMs { get; } =
            new ObservableCollection<EffectViewModel>();

        internal double StartTime => StoryBoard.GetStartTime(this);
        internal double EndTime => StartTime + Duration;

        internal void UpdateTime()
        {
            OnPropertyChanged(nameof(Duration));
        }

        internal void AddEffect(string effectName)
        {
            Effect effect = EffectsModel.CreateEffect(effectName);
            EffectVMs.Add(new EffectViewModel(effect, StoryBoard, StartTime, EndTime, this));
        }

        void IChildItemRemove.Remove(object child)
        {
            EffectViewModel effect = (EffectViewModel)child;
            EffectVMs.Remove(effect);
        }

        internal abstract Task<List<MediaClip>> CreateMediaClipsAsync();

        private IEnumerable<VideoEffectDefinition> CreateEffects()
        {
            return EffectVMs.Select(it => it.CreateDefinition(StartTime, EndTime));
        }

        internal async Task<List<MediaClip>> CreateMediaClipsWithEffectsAsync()
        {
            try
            {
                var clips = await CreateMediaClipsAsync();
                foreach (var e in CreateEffects())
                    foreach (var cl in clips)
                    {
                        cl.VideoEffectDefinitions.Add(e);
                    }
                return clips;
            }catch (InvalidCastException ex)
            {
                Debug.WriteLine("♥ InvalidCastException " + ex.Source);
                return await CreateMediaClipsAsync();
            }
        }
    }
}
