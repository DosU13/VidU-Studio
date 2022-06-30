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
    public sealed class LyricsRotation: IBasicVideoEffect
    {
        private int side;

        public void ProcessFrame(ProcessVideoFrameContext context)
        {
            using (CanvasBitmap inputBitmap = CanvasBitmap.CreateFromDirect3D11Surface(_canvasDevice, context.InputFrame.Direct3DSurface))
            using (CanvasRenderTarget renderTarget = CanvasRenderTarget.CreateFromDirect3D11Surface(_canvasDevice, context.OutputFrame.Direct3DSurface))
            using (CanvasDrawingSession ds = renderTarget.CreateDrawingSession())
            {
                var rot = new StraightenEffect()
                {
                    Source = inputBitmap,
                    MaintainSize = true,
                    Angle = side * MathF.PI * 0.0625f
                };
                ds.DrawImage(rot);
            }
        }

        public void SetProperties(IPropertySet configuration)
        {
            side = Convert.ToInt32(configuration["side"]);
        }

        private CanvasDevice _canvasDevice;

        public void SetEncodingProperties(VideoEncodingProperties encodingProperties, IDirect3DDevice device)
        {
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