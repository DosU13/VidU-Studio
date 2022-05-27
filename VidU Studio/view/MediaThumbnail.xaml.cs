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
    public sealed partial class MediaThumbnail : UserControl
    {
        public MediaViewModel MediaVM
        {
            get { return (MediaViewModel)GetValue(MediaVMProperty); }
            set { SetValue(MediaVMProperty, value); Bindings.Update(); _ = UpdateImageSource();
                MediaVM.PropertyChanged += Value_PropertyChanged;}
        }

        public static readonly DependencyProperty MediaVMProperty = 
            DependencyProperty.Register(nameof(MediaVM), typeof(MediaViewModel), typeof(MediaThumbnail), new PropertyMetadata(null));

        private void Value_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            _ = Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                async () =>
                {
                    await UpdateImageSource();
                    Bindings.Update();
                });
        }

        public MediaThumbnail()
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
                if (MediaVM == null) return Symbol.Download;
                if (MediaVM.File == null) return Symbol.Download;
                if (MediaVM.IsImage) return Symbol.Camera;
                else return Symbol.Video;
            }
        }

        private Visibility TypeIconVisibility
        {
            get
            {
                if(MediaVM==null) return Visibility.Collapsed;
                if (MediaVM.File == null) return Visibility.Collapsed;
                else return Visibility.Visible;
            }
        }

        private async Task UpdateImageSource()
        {
            try
            {
                if (MediaVM.File == null) return;
                BitmapImage bitmap = new BitmapImage();
                bitmap.DecodePixelHeight = 224;
                bitmap.DecodePixelType = DecodePixelType.Logical;
                if (MediaVM.File.IsVideo())
                    await bitmap.SetSourceAsync(await MediaVM.File.GetThumbnailAsync(ThumbnailMode.SingleItem));
                else 
                {
                    using (IRandomAccessStream fileStream = await MediaVM.File.OpenAsync(FileAccessMode.Read))
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
