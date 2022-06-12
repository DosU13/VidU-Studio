using System;
using System.Collections.Generic;
using Windows.Foundation.Collections;
using Windows.Graphics.DirectX.Direct3D11;
using Windows.Media.Effects;
using Windows.Media.MediaProperties;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Effects;
using System.Numerics;
using System.Diagnostics;

namespace VidUVideoEffects
{
    public sealed class Transform2D : IBasicVideoEffect
    {
        public void ProcessFrame(ProcessVideoFrameContext context)
        {
            using (CanvasBitmap inputBitmap = CanvasBitmap.CreateFromDirect3D11Surface(_canvasDevice, context.InputFrame.Direct3DSurface))
            using (CanvasRenderTarget renderTarget = CanvasRenderTarget.CreateFromDirect3D11Surface(_canvasDevice, context.OutputFrame.Direct3DSurface))
            using (CanvasDrawingSession ds = renderTarget.CreateDrawingSession())
            {
                TimeSpan time = context.InputFrame.RelativeTime.HasValue ? context.InputFrame.RelativeTime.Value : new TimeSpan();
                if (time.TotalSeconds < Start || time.TotalSeconds > End)
                {
                    ds.DrawImage(inputBitmap);
                    return;
                }

                // START HERE
                double s = Scale.Get(time.TotalSeconds);
                double w = inputBitmap.Size.Width;
                double h = inputBitmap.Size.Height;
                var trans = new Transform2DEffect();
                trans.TransformMatrix = new Matrix3x2(
                    Scale.Get(time.TotalSeconds), 0, 0, Scale.Get(time.TotalSeconds),
                    (float)((1 - s + (s+1)*Horizontal.Get(time.TotalSeconds))*w/2),
                    (float)((1 - s + (s+1)*Vertical.Get(time.TotalSeconds))*h/2));
                trans.Source = inputBitmap;
                Debug.WriteLine("♥♥♥ "+ Horizontal.Get(time.TotalSeconds));
                ds.DrawImage(trans);
            }
        }

        private Property Scale;
        private Property Horizontal;
        private Property Vertical;

        float Start;
        float End;
        public void SetProperties(IPropertySet configuration)
        {
            Start = Convert.ToSingle(configuration["Start"]);
            End = Convert.ToSingle(configuration["End"]);
            //START HERE
            Scale = new Property(nameof(Scale), configuration);
            Horizontal = new Property(nameof(Horizontal), configuration);
            Vertical = new Property(nameof(Vertical), configuration);
        }

        private CanvasDevice _canvasDevice;

        public void SetEncodingProperties(VideoEncodingProperties encodingProperties, IDirect3DDevice device)
        {
            //_currentEncodingProperties = encodingProperties;
            _canvasDevice = CanvasDevice.CreateFromDirect3D11Device(device);
            CanvasDevice.DebugLevel = CanvasDebugLevel.Error;
        }

        public bool IsReadOnly { get { return false; } }
        public MediaMemoryTypes SupportedMemoryTypes { get { return MediaMemoryTypes.Gpu; } }
        public bool TimeIndependent { get { return false; } }

        public IReadOnlyList<VideoEncodingProperties> SupportedEncodingProperties
        {
            get
            {
                return new List<VideoEncodingProperties>()
                {
                    VideoEncodingProperties.CreateUncompressed(MediaEncodingSubtypes.Argb32, 800, 600)
                };
            }
        }

        public void Close(MediaEffectClosedReason reason)
        {
            if (_canvasDevice != null)
                _canvasDevice.Dispose();
        }

        public void DiscardQueuedFrames()
        {
        }
    }
}