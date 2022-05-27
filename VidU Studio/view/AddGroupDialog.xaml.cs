using MuzU;
using MuzU.data;
using MuzU.util;
using System;
using System.Collections.Generic;
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
    public sealed partial class AddGroupDialog : ContentDialog
    {
        private MuzUProject MuzUProject;

        public AddGroupDialog(MuzUProject muzUProject)
        {
            MuzUProject = muzUProject;
            this.InitializeComponent();
        }

        private bool IsPrimaryBtnEnabled => (!isMuzUOn || SelectedPropertyIndex != -1);

        private double StartPos { get; set; } = 0;
        private double EndPos { get; set; } = 10;
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

        private Visibility VisibilityIfMuzUOff { get {
                if (isMuzUOn) return Visibility.Collapsed;
                else return Visibility.Visible; } }

        private double DefaultDuration { get; set; }

        private bool isAutoLocate = true;
        private bool IsAutoLocate { get=>isAutoLocate ; set { isAutoLocate = value; Bindings.Update(); } }

        private string Info { get
            {
                if (IsMuzUOn && IsAutoLocate) return "! Choose only few medias (one per parameter),\r" +
                        "MuzU will locate them according to Parameter value";
                else if (IsMuzUOn && !IsAutoLocate) return "! You will choose medias independently,\r" +
                        "parameter values will only act like a hint";
                else return "";
            } }

        public BaseClip Result;
        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            if (IsMuzUOn)
            {
                var dict = new DictionaryXml()
                {
                    Dict = MuzUExtractor.Extract(SelectedSequence, SelectedPropertyIndex, StartPos, EndPos),
                    IsValueInteger = SelectedSequence.TimingTemplate.Properties[SelectedPropertyIndex].Type == MuzU.data.ValueType.Integer
                };
                if (IsAutoLocate)
                {
                    AutoSequencerClip clip = new AutoSequencerClip();
                    var v = dict.Dict.Select(x => x.Value).ToList();
                    v.Sort();
                    List<double> values = v.Distinct().ToList();
                    dict.Dict = dict.Dict.Select(x => KeyValuePair.Create(x.Key, (double)values.IndexOf(x.Value))).ToList();
                    clip.Values = values;
                    clip.IsValueInteger = dict.IsValueInteger;
                    clip.TimingsWithIndices = dict;
                    Result = clip;
                }
                else Result = new SequencerClip() { TimingsWithValues = dict };
                Result.StartPos = StartPos;
            }
        }

        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Bindings.Update();
        }
    }
}
