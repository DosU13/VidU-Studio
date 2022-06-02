﻿using System;
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
    public sealed class Saturation : IBasicVideoEffect
    {
        private Vector2 WhitePoint(float Intensity)
        {
            if (Intensity > 0) return new Vector2(1 - Intensity, 1);
            else return new Vector2(1, 1 + Intensity);
        }

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
                var saturation = new SaturationEffect()
                {
                    Source = inputBitmap,
                    Saturation = Intensity.Get(time.TotalSeconds)
                };
                ds.DrawImage(saturation);
            }
        }

        private Property Intensity;

        float Start;
        float End;
        public void SetProperties(IPropertySet configuration)
        {
            Start = Convert.ToSingle(configuration["Start"]);
            End = Convert.ToSingle(configuration["End"]);
            //START HERE
            Intensity = new Property(nameof(Intensity), configuration);
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
                    // NOTE: Specifying width and height is only necessary due to bug in media pipeline when
                    // effect is being used with Media Capture. 
                    // This can be changed to "0, 0" in a future release of FBL_IMPRESSIVE. 
                    VideoEncodingProperties.CreateUncompressed(MediaEncodingSubtypes.Argb32, 800, 600)
                };
            }
        }

        public void Close(MediaEffectClosedReason reason)
        {
            // Clean up devices
            if (_canvasDevice != null)
                _canvasDevice.Dispose();
        }

        public void DiscardQueuedFrames()
        {
            // No cached frames to discard
        }
    }
}