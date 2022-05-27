using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using VidU_Studio.util;
using VidU_Studio.viewmodel;
using Windows.ApplicationModel.Resources.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.Storage.Streams;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace VidU_Studio.view
{
    public sealed partial class ClipThumbnail : UserControl
    {
        public ClipViewModel ClipVM
        {
            get { return (ClipViewModel)GetValue(ClipVMProperty); }
            set { SetValue(ClipVMProperty, value); Bindings.Update(); _ = UpdateImageSource();
                ClipVM.PropertyChanged += Value_PropertyChanged;}
        }

        public static readonly DependencyProperty ClipVMProperty = 
            DependencyProperty.Register(nameof(ClipVM), typeof(ClipViewModel), typeof(ClipThumbnail), new PropertyMetadata(null));

        private void Value_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            _ = Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                async () =>
                {
                    await UpdateImageSource();
                    Bindings.Update();
                });
        }
        //public IStorageFile File
        //{
        //    get { return (IStorageFile)GetValue(FileProperty); }
        //    set { SetValue(FileProperty, value); UpdateImageSource(); }
        //}
        //public static readonly DependencyProperty FileProperty =
        //    DependencyProperty.Register(nameof(File), typeof(File), typeof(ClipThumbnail), null);

        //public string Duration
        //{
        //    get { return (string)GetValue(DurationProperty); }
        //    set { SetValue(DurationProperty, value); Bindings.Update(); }
        //}
        //public static readonly DependencyProperty DurationProperty =
        //    DependencyProperty.Register(nameof(Duration), typeof(Duration), typeof(ClipThumbnail), null);

        public ClipThumbnail()
        {
            this.InitializeComponent();
        }

        private ImageSource imageSource = null;
        private ImageSource ImageSource
        {
            get
            {
                return imageSource ?? new BitmapImage(ResourceManager.Current.MainResourceMap[@"Files/Assets/StoreLogo.png"].Uri);
            }
        }

        private Symbol TypeIcon
        {
            get
            {
                if (ClipVM == null) return Symbol.Download;
                if (ClipVM.Thumbnail == null) return Symbol.Download;
                if (ClipVM is GroupClipViewModel) return Symbol.Pictures;
                if (ClipVM.Thumbnail.IsVideo()) return Symbol.Video;
                else return Symbol.Camera;
            }
        }

        private Visibility TypeIconVisibility
        {
            get
            {
                if(ClipVM==null) return Visibility.Collapsed;
                if (ClipVM.Thumbnail == null) return Visibility.Collapsed;
                else return Visibility.Visible;
            }
        }

        private async Task UpdateImageSource()
        {
            try
            {
                if (ClipVM.Thumbnail == null) return;
                BitmapImage bitmap = new BitmapImage();
                bitmap.DecodePixelHeight = 224;
                bitmap.DecodePixelType = DecodePixelType.Logical;
                if (ClipVM.Thumbnail.IsVideo())
                    await bitmap.SetSourceAsync(await ClipVM.Thumbnail.GetThumbnailAsync(ThumbnailMode.SingleItem));
                else 
                {
                    using (IRandomAccessStream fileStream = await ClipVM.Thumbnail.OpenAsync(FileAccessMode.Read))
                    {
                        await bitmap.SetSourceAsync(fileStream);
                    }
                }
                imageSource = bitmap;
                Bindings.Update();
            }
            catch (Exception) { }
        }
    }
}
