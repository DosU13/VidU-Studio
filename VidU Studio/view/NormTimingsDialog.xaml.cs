using MuzU;
using MuzU.data;
using MuzU.util;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using VidU.data;
using VidU_Studio.util;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Content Dialog item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace VidU_Studio.view
{
    public sealed partial class NormTimingsDialog : ContentDialog
    {
        private MuzUProject MuzUProject;

        public NormTimingsDialog(MuzUProject muzUProject, double startTime, double endTime)
        {
            MuzUProject = muzUProject;
            StartPos = startTime;
            EndPos = endTime;
            this.InitializeComponent();
        }

        private bool IsPrimaryBtnEnabled => (!isMuzUOn || SelectedPropertyIndex != -1);

        private double StartPos;
        private double EndPos;
        private TimingSequence selectedSequence;
        private TimingSequence SelectedSequence { get=>selectedSequence; 
            set { selectedSequence = value;} }
        private int selectedPropertyIndex = -1;
        private int SelectedPropertyIndex { get=> selectedPropertyIndex; set { selectedPropertyIndex = value; Bindings.Update(); }}

        private bool isMuzUOn = true;
        private bool IsMuzUOn { get => isMuzUOn; set { isMuzUOn = value; Bindings.Update(); } }

        private Visibility VisibilityIfMuzUOn { get {
                if(isMuzUOn) return Visibility.Visible;
                else return Visibility.Collapsed; } }

        public NumberDictionaryXml Result = null;
        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            if (IsMuzUOn){
                var Dict = MuzUExtractor.Extract(SelectedSequence, SelectedPropertyIndex, StartPos, EndPos);
                double minValue = Dict.Min(it => it.Value);
                double maxValue = Dict.Max(it => it.Value);
                if (minValue == maxValue) Result = new NumberDictionaryXml()
                    { Dict = Dict.Select(it => KeyValuePair.Create(it.Key, 0.0)).ToList()};
                else Result = new NumberDictionaryXml()
                    { Dict = Dict.Select(it => KeyValuePair.Create(it.Key, (it.Value - minValue) / (maxValue - minValue))).ToList() };
            }else Result = null;
        }

        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Bindings.Update();
        }
    }
}
