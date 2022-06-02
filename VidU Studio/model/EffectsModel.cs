using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VidU.data;

namespace VidU_Studio.model
{
    internal class EffectsModel
    {
        public static List<string> EffectNames => AllEffects.Select(x => x.Name).ToList();

        public static List<string> TransitionTypes = new List<string>
            {"None", "Growth", "Decay", "Shake"};

        public static List<VidUEffect> AllEffects = new List<VidUEffect>() {
            new VidUEffect("Brightness", new List<VidUEffectProperty>{
                new VidUEffectProperty("Intensity", -1, 1, 0)}),
            new VidUEffect("GaussianBlur", new List<VidUEffectProperty>{
                new VidUEffectProperty("BlurAmount", 0, 50, 3)}),
            new VidUEffect("Contrast", new List<VidUEffectProperty>{
                new VidUEffectProperty("Intensity", -1, 1, 0)}),
            new VidUEffect("Saturation", new List<VidUEffectProperty>{
                new VidUEffectProperty("Intensity", 0, 1, 0.5)}),
            new VidUEffect("Sharpness", new List<VidUEffectProperty>{
                new VidUEffectProperty("Amount", 0, 10, 0)}),
            new VidUEffect("Vignette", new List<VidUEffectProperty>{
                new VidUEffectProperty("Amount", 0, 1, 0.1)}),
            new VidUEffect("DirectionalBlur", new List<VidUEffectProperty>{
                new VidUEffectProperty("BlurAmount", 0, 50, 3),
                new VidUEffectProperty("Angle", 0, 1, 6.283)}),
            new VidUEffect("Transform2D", new List<VidUEffectProperty>{
                new VidUEffectProperty("Scale", 0, 5, 1),
                new VidUEffectProperty("Horizontal", -1, 1, 0),
                new VidUEffectProperty("Vertical", -1, 1, 0)})
        };

        internal static Effect CreateEffect(string effectName)
        {
            Effect effect = new Effect() { Name = effectName, IsMuzU=false};
            VidUEffect vidUEffect = AllEffects.Find(it=>it.Name==effectName);
            foreach(var p in vidUEffect.Properties)
            {
                EffectProperty property = new EffectProperty() { Name = p.Name, Value=p.InitialValue,
                        MinValue = p.MinValue, MaxValue = p.MaxValue};
                effect.Properties.Add(property);
            }
            return effect;
        }

        internal static double MaximumOf(string effectName, string propertyName)
        {
            return AllEffects.Find(it => it.Name == effectName).
                Properties.Find(it => it.Name == propertyName).MaxValue;
        }

        internal static double MinimumOf(string effectName, string propertyName)
        {
            return AllEffects.Find(it => it.Name == effectName).
                Properties.Find(it => it.Name == propertyName).MinValue;
        }
    }

    struct VidUEffect
    {
        public VidUEffect(string name, List<VidUEffectProperty> properties)
        {
            Name = name;
            Properties = properties;
        }

        public string Name { get; }
        public List<VidUEffectProperty> Properties { get; }
    }

    struct VidUEffectProperty
    {
        public VidUEffectProperty(string name, double minValue, double maxValue, double initialValue)
        {
            Name = name;
            MinValue = minValue;
            MaxValue = maxValue;
            InitialValue = initialValue;
        }

        public string Name { get; }
        public double MinValue { get; } 
        public double MaxValue { get; }
        public double InitialValue { get; }
    }
}
