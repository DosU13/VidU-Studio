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
    public sealed class DirectionalBlur : IBasicVideoEffect
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
                var directionalBlur = new DirectionalBlurEffect()
                {
                    Source = inputBitmap,
                    BlurAmount = BlurAmount.Get(time.TotalSeconds),
                    Angle = Angle.Get(time.TotalSeconds)
                };
                ds.DrawImage(directionalBlur);
            }
        }

        private Property BlurAmount;
        private Property Angle;

        float Start;
        float End;
        public void SetProperties(IPropertySet configuration)
        {
            Start = Convert.ToSingle(configuration["Start"]);
            End = Convert.ToSingle(configuration["End"]);
            //START HERE
            BlurAmount = new Property(nameof(BlurAmount), configuration);
            Angle = new Property(nameof(Angle), configuration);
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