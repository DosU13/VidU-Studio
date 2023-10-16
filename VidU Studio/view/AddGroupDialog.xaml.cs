using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using VidU.data;
using VidU_Studio.model;
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
        private MuzUModel muzUModel;

        public AddGroupDialog(MuzUModel muzUModel, double startTime)
        {
            this.muzUModel = muzUModel;
            StartPos = startTime;
            this.InitializeComponent();
        }

        public MuzUModel MuzUModel => muzUModel;
        private bool IsPrimaryBtnEnabled => (!isMuzUOn || SelectedProperty != null);

        private double StartPos { get; set; } = 0;
        private double Duration => SecondsBeatConverter.ConvertBack(DurationTxtBox.Text);
        private double EndPos => StartPos + Duration;
        private SequenceModel selectedSequence;
        private SequenceModel SelectedSequence { 
            get => selectedSequence; 
            set {
                if (selectedSequence == value) return;
                selectedSequence = value;
                Bindings.Update();
            } }
        private string selectedProperty = null;
        private string SelectedProperty { 
            get=> selectedProperty; 
            set { 
                if (selectedProperty == value) return;
                selectedProperty = value; 
                Bindings.Update(); }}

        private bool isMuzUOn = true;
        private bool IsMuzUOn { 
            get => isMuzUOn; 
            set { 
                if(isMuzUOn == value) return;
                isMuzUOn = value; 
                Bindings.Update(); } }

        private Visibility VisibilityIfMuzUOn { get {
                if(isMuzUOn) return Visibility.Visible;
                else return Visibility.Collapsed; } }

        private Visibility VisibilityIfMuzUOff { get {
                if (isMuzUOn) return Visibility.Collapsed;
                else return Visibility.Visible; } }

        private double DefaultDuration { get; set; }

        private bool isAutoLocate = true;
        private bool IsAutoLocate { 
            get=>isAutoLocate ; 
            set {
                if (isAutoLocate == value) return;
                isAutoLocate = value; 
                Bindings.Update(); } }

        private string Info { get
            {
                if (IsMuzUOn && IsAutoLocate) return "! Choose only few medias (one per parameter),\r" +
                        "MuzU will locate them according to Parameter value";
                else if (IsMuzUOn && !IsAutoLocate) return "! You will choose medias independently,\r" +
                        "parameter values will only act like a hint";
                else return "";
            } }

        private List<string> Properties => SelectedSequence?.Properties;

        public BaseClip Result;
        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            if (IsMuzUOn)
            {
                StringDictionaryXml dict = new StringDictionaryXml();
                switch (SelectedProperty)
                {
                    case "Lyrics":
                        dict = new StringDictionaryXml()
                        { Dict = SelectedSequence.GetLyrics(StartPos, EndPos) };
                        break;
                    case "Note":
                        {
                            var n = new NumberDictionaryXml()
                            {
                                Dict = SelectedSequence.GetNotes(StartPos, EndPos),
                                IsValueInteger = true
                            };
                            dict = StringDictionaryXml.NumbToStrDictXml(n);
                            break;
                        }

                    case "Length":
                        {
                            var n = new NumberDictionaryXml()
                            {
                                Dict = SelectedSequence.GetLengths(StartPos, EndPos),
                                IsValueInteger = false
                            };
                            dict = StringDictionaryXml.NumbToStrDictXml(n);
                            break;
                        }
                }
                if (IsAutoLocate)
                {
                    AutoSequencerClip clip = new AutoSequencerClip();
                    var values = dict.Dict.Select(x => x.Value).Distinct().ToList();
                    values.Sort();
                    clip.Values = values;
                    clip.TimingsWithIndices = new NumberDictionaryXml()
                    { Dict = dict.Dict.Select(x => KeyValuePair.Create(x.Key, (double)values.IndexOf(x.Value))).ToList() };
                    Result = clip;
                }
                else Result = new SequencerClip() { TimingsWithValues = dict };
                Result.Duration = Duration;
            }
        }
    }
}
