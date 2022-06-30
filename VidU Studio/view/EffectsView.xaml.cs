using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using VidU_Studio.model;
using VidU_Studio.util;
using VidU_Studio.viewmodel;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace VidU_Studio.view
{
    public sealed partial class EffectsView : UserControl
    {
        public EffectsView()
        {
            this.InitializeComponent();
        }

        private StoryBoardViewModel _storyBoardVM;
        internal StoryBoardViewModel StoryBoardVM
        {
            get => _storyBoardVM; set { _storyBoardVM = value; Bindings.Update(); }
        } 
        internal ITimingCreator TimingCreator;

        private async void EffectMuzU_Click(object sender, RoutedEventArgs e)
        {
            EffectViewModel effectVM = (EffectViewModel)((Button)sender).DataContext;
            var res = await TimingCreator.NormTimingsDialog(effectVM.ParentStartTime, effectVM.ParentEndTime);
            if (!res.Key) return;
            effectVM.ChangeMuzU(res.Value != null, res.Value);
        }

        private void AddEffect_Click(object sender, RoutedEventArgs e)
        {
            if (AddEffectComboBox.SelectedIndex != -1)
                StoryBoardVM.SelectedClip.AddEffect((string)AddEffectComboBox.SelectedItem);
        }

        internal void Refresh() => Bindings.Update();
    }
}
