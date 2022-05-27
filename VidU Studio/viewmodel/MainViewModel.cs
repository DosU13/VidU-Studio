using MuzU;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VidU;
using VidU_Studio.model;
using Windows.Storage;

namespace VidU_Studio.viewmodel
{
    internal class MainViewModel: BindableBase
    {
        public VidUProject _project;
        public VidUProject Project
        {
            get => _project;
            set
            {
                SetProperty(ref _project, value);
                Task.Run(async () => LoadMuzUProject(await StorageFile.GetFileFromPathAsync(value.data.MuzUPath)));
                CompositionModel = new CompositionModel(_project.data.Clips);
                CompositionModel.mediaPlayerElement = mediaPlayerElement;
                Task.Run(async () => CompositionModel.BackMusic = await StorageFile.GetFileFromPathAsync(value.data.MusicPath));
                Storyboard.CompositionModel = CompositionModel;
                Bindings.Update();
            }
        }

        private IStorageFile projectFile = null;
        private bool existProject => Project != null;
        private bool existProjectFile => projectFile != null;
        public MuzUProject MuzU;
        private CompositionModel _compositionModel;
        private CompositionModel CompositionModel
        {
            get => _compositionModel; set
            {
                _compositionModel = value; _compositionModel.PropertyChanged += _compositionModel_PropertyChanged; ;
            }
        }

        private void _compositionModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (GroupMediaView.GroupClipVM != CompositionModel.SelectedGroupClip)
                GroupMediaView.GroupClipVM = CompositionModel.SelectedGroupClip;
            Bindings.Update();
        }
    }
}
