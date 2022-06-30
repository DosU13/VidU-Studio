using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Storage;
using System.Threading.Tasks;
using Windows.Storage.Streams;
using Windows.UI.Core.Preview;
using Windows.Storage.AccessCache;
using VidU;
using MuzU;
using Windows.Media.Core;
using VidU_Studio.model;
using System.ComponentModel;
using VidU_Studio.view;
using VidU_Studio.util;
using VidU_Studio.viewmodel;
using VidU.data;
using Windows.Media.Transcoding;
using Windows.UI.Core;
using Windows.UI.Popups;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace VidU_Studio
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page, ITimingCreator, IMainPage
    {
        private readonly ProjectRepoViewModel ProjectRepoVM; 
        private CompositionModel CompositionModel;
        public MainPage()
        {
            this.InitializeComponent();
            SystemNavigationManagerPreview.GetForCurrentView().CloseRequested += OnWindowClose;
            CompositionModel = new CompositionModel() { MediaPlayerElement = mediaPlayerElement };
            ProjectRepoVM = new ProjectRepoViewModel(this);
            Storyboard.TimingCreator = this;
            _EffectsView.TimingCreator = this;
        }

        private MusicViewModel MusicVM;
        private MainViewModel MainVM;
        private StoryBoardViewModel StoryBoardVM;
        void IMainPage.ProjectChanged(VidUProject newProject)
        {
            CompositionModel.Stop();
            MusicVM = new MusicViewModel(newProject.data, CompositionModel);
            MainVM = new MainViewModel(newProject.data, MusicVM, this);
            StoryBoardVM = new StoryBoardViewModel(newProject.data.Clips, CompositionModel, GroupMediaView, _EffectsView);
            Storyboard.StoryBoardVM = StoryBoardVM;
            _EffectsView.StoryBoardVM = StoryBoardVM;
            Bindings.Update();
        }

        void IMainPage.BPMChanged()
        {
            foreach (var c in CompositionModel.Clips) c.UpdateTime();
        }

        private void _compositionModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (GroupMediaView.GroupClipVM != StoryBoardVM.SelectedGroupClip)
                GroupMediaView.GroupClipVM = StoryBoardVM.SelectedGroupClip;
            Bindings.Update();
        }

        private ContentDialog dialog = null;

        private async void MusicMuzU_Click(object sender, RoutedEventArgs e)
        {
            dialog = MusicMuzUContentDialog;
            await MusicMuzUContentDialog.ShowAsync();
        }

        private async void FinishVideo_Click(object sender, RoutedEventArgs e)
        {
            dialog = FinishYourVideoDialog;
            if(await FinishYourVideoDialog.ShowAsync() == ContentDialogResult.Primary)
            {
                var saveOperation = await CompositionModel.RenderVideo(VideoQualityComboBox.SelectedIndex, FastEncodingCheckBox.IsChecked.Value);
                if (saveOperation == null) return;
                dialog = RenderProgressDialog;
                RenderProgressBar.Value = 0;
                _ = RenderProgressDialog.ShowAsync();
                saveOperation.Progress = new AsyncOperationProgressHandler<TranscodeFailureReason, double>(async (info, progress) =>
                {
                    await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, new DispatchedHandler(() =>
                    {
                        RenderProgressBar.Value = progress;
                    }));
                });
                saveOperation.Completed = new AsyncOperationWithProgressCompletedHandler<TranscodeFailureReason, double>(async (info, status) =>
                {
                    await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, new DispatchedHandler(async () =>
                    {
                        dialog.Hide();
                        try
                        {
                            var results = info.GetResults();
                            if (results != TranscodeFailureReason.None || status != AsyncStatus.Completed)
                            {
                                await (new MessageDialog("Saving was unsuccessful ❗❗❗")).ShowAsync();
                            }
                            else
                            {
                                await (new MessageDialog("Your video successfully renderred ♥♥♥")).ShowAsync();
                            }
                        }
                        catch (Exception) { }
                    }));
                });
            }
        }

        async Task<bool> IMainPage.SaveWorkDialog()
        {
            if (!ProjectRepoVM.ExistProject) return true;
            if(dialog != null) dialog.Hide();
            dialog = new ContentDialog();
            dialog.Title = "Save your work?";
            dialog.PrimaryButtonText = "Save";
            dialog.SecondaryButtonText = "Don't Save";
            dialog.CloseButtonText = "Cancel";
            dialog.DefaultButton = ContentDialogButton.Primary;

            var result = await dialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                await ProjectRepoVM.Save();
                return true;
            }
            else if (result == ContentDialogResult.Secondary) return true;
            else if (result == ContentDialogResult.None) return false;
            return false;
        }

        async Task ITimingCreator.AddGroupDialog()
        {
            double strTime;
            if (CompositionModel.Clips.Count == 0) strTime = 0.0;
            else strTime = CompositionModel.Clips.Last().EndTime;
            dialog = new AddGroupDialog(MainVM.MuzUProject, strTime);
            if(await dialog.ShowAsync() == ContentDialogResult.Primary)
            {
                StoryBoardVM.AddGroup((dialog as AddGroupDialog).Result);
            }
        }

        async Task<KeyValuePair<bool, NumberDictionaryXml>> ITimingCreator.NormTimingsDialog(double startTime, double endTime)
        {
            dialog = new NormTimingsDialog(MainVM.MuzUProject, startTime, endTime);
            if (await dialog.ShowAsync() == ContentDialogResult.Primary)
            {
                return KeyValuePair.Create(true,(dialog as NormTimingsDialog).Result);
            }
            else return KeyValuePair.Create(false, (NumberDictionaryXml)null);
        }

        private async void OnWindowClose(object sender, SystemNavigationCloseRequestedPreviewEventArgs args)
        {
            args.Handled = true;
            if(await (this as IMainPage).SaveWorkDialog()) App.Current.Exit();
        }
    }
}
