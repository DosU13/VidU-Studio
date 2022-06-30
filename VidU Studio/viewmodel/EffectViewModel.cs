using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VidU.data;
using VidU_Studio.model;
using VidU_Studio.util;
using Windows.Foundation.Collections;
using Windows.Media.Effects;
using Windows.UI.Xaml;

namespace VidU_Studio.viewmodel
{
    internal class EffectViewModel : BindableBase
    {
        internal Effect Data;
        private IStoryBoard StoryBoard;
        internal double ParentStartTime;
        internal double ParentEndTime;
        private IChildItemRemove ChildItemRemove;

        public EffectViewModel(Effect data, IStoryBoard storyBoard,
            double parentStartTime, double parentEndTime, IChildItemRemove childItemRemove)
        {
            Data = data;
            StoryBoard = storyBoard;
            ParentStartTime = parentStartTime;
            ParentEndTime = parentEndTime;
            ChildItemRemove = childItemRemove;
            Properties = data.Properties.Select(it => new EffectPropertyViewModel(it, StoryBoard,
                EffectsModel.MinimumOf(Data.Name, it.Name),
                EffectsModel.MaximumOf(Data.Name, it.Name))).ToList();
            IsMuzU = data.IsMuzU;
        }

        internal string Name => Data.Name;

        private bool isMuzU;
        internal bool IsMuzU { get => isMuzU;
            set { isMuzU = value;
                foreach (var e in Properties) e.IsMuzU = value;
                Data.IsMuzU = value;
                StoryBoard.UpdateComposition();
            }
        }

        internal void Remove() => ChildItemRemove.Remove(this);

        internal List<EffectPropertyViewModel> Properties = new List<EffectPropertyViewModel>();

        internal VideoEffectDefinition CreateDefinition(double Start, double End)
        {
            IPropertySet propSet = new PropertySet();
            if (IsMuzU)
            {
                try
                {
                    Debug.WriteLine("♥♥♥ " + Data.TimingsWithValues.Dict.Select(it => "[" + it.Key + ":" + it.Value + "]\r").Aggregate(string.Empty, (s, v) => s + v));
                    Dictionary<float, float> Values = new Dictionary<float, float>();
                    Data.TimingsWithValues.Dict.ForEach(
                        it => Values.Add(Convert.ToSingle(it.Key), Convert.ToSingle(it.Value)));
                    propSet.Add("Values", Values);
                }catch (Exception ex) { Debug.WriteLine("♥♥♥ "+ex); }
            }
            propSet.Add("Start", Start);
            propSet.Add("End", End);
            propSet.Add("IsMuzU", IsMuzU);
            foreach (var ps in Properties) ps.ApplyPropertySet(propSet);
            return new VideoEffectDefinition("VidUVideoEffects." + Data.Name, propSet);
        }

        internal void ChangeMuzU(bool isMuzU, NumberDictionaryXml value)
        {
            Data.TimingsWithValues = value;
            IsMuzU = isMuzU;
        }
    }

    internal class EffectPropertyViewModel : BindableBase
    {
        private EffectProperty Data;
        private IStoryBoard StoryBoard;

        public EffectPropertyViewModel(EffectProperty data, IStoryBoard storyBoard, 
                                               double minimum, double maximum)
        {
            Data = data;
            StoryBoard = storyBoard;
            Minimum = minimum;
            Maximum = maximum;
            _value = data.Value;
            minValue = data.MinValue;
            maxValue = data.MaxValue;
            TransitionType = data.TransitionType;
        }

        public string Name => Data.Name;

        private bool isMuzU = false;
        public bool IsMuzU { get => isMuzU;
            set { SetProperty(ref isMuzU, value); OnPropertyChanged(nameof(VisibilityIfMuzUOn)); OnPropertyChanged(nameof(VisibilityIfMuzUOff)); } }

        private string transitionType;
        internal string TransitionType { get => transitionType;
            set { SetProperty(ref transitionType, value); 
                  Data.TransitionType = value;
                  StoryBoard.UpdateComposition();}}

        internal double Minimum { get; }
        internal double Maximum { get; }
        private double _value;
        internal double Value { get => _value; set{
                SetProperty(ref _value, value);
                Data.Value = value;
            }}

        private double minValue;
        internal double MinValue
        {
            get => minValue; set
            {
                SetProperty(ref minValue, value);
                Data.MinValue = value;
            }
        }

        private double maxValue;
        internal double MaxValue
        {
            get => maxValue; set
            {
                SetProperty(ref maxValue, value);
                Data.MaxValue = value;
            }
        }
        internal void UpdateComposition() => StoryBoard.UpdateComposition();

        internal Visibility VisibilityIfMuzUOn { get { if(IsMuzU) return Visibility.Visible; return Visibility.Collapsed; } }
        internal Visibility VisibilityIfMuzUOff { get { if(IsMuzU) return Visibility.Collapsed; return Visibility.Visible; } }

        internal void ApplyPropertySet(IPropertySet propSet)
        {
            if (IsMuzU)
            {
                propSet.Add(Name + "_" + nameof(MinValue), MinValue);
                propSet.Add(Name + "_" + nameof(MaxValue), MaxValue);
            } else propSet.Add(Name + "_" + nameof(Value), Value);
            propSet.Add(Name+"_"+ nameof(TransitionType), TransitionType);
        }
    }
}
