using SkiaSharp;
using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;
using Microsoft.Maui.Controls;
using System;
using System.IO;

namespace SkiaSharpTest.Controls
{
    public class CachedImage : SKCanvasView
    {
        public static readonly BindableProperty SourceProperty = BindableProperty.Create(nameof(Source), typeof(string), typeof(CachedImage), string.Empty, propertyChanged: OnSourceChanged);

        public string Source { get => (string)GetValue(SourceProperty); set => SetValue(SourceProperty, value); }

        private SKBitmap? _bitmap;

        public CachedImage()
        {
            IgnorePixelScaling = true;
            PaintSurface += OnPaintSurface;
        }

        private static void OnSourceChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is CachedImage ci && newValue is string filename && !string.IsNullOrEmpty(filename))
            {
                var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, filename);
                if (File.Exists(path))
                {
                    using var stream = File.OpenRead(path);
                    ci._bitmap = SKBitmap.Decode(stream);
                }
                ci.InvalidateSurface();
            }
        }

        private void OnPaintSurface(object? sender, SKPaintSurfaceEventArgs e)
        {
            var canvas = e.Surface.Canvas;
            canvas.Clear(SKColors.Transparent);

            if (_bitmap != null)
            {
                using var paint = new SKPaint { IsAntialias = true };
                var sourceRect = new SKRect(0, 0, _bitmap.Width, _bitmap.Height);
                var destRect = new SKRect(0, 0, e.Info.Width, e.Info.Height);
                canvas.DrawBitmap(_bitmap, sourceRect, destRect, 
                    //new SKSamplingOptions(SKFilterMode.Linear), 
                    paint);
            }
        }
    }
}
