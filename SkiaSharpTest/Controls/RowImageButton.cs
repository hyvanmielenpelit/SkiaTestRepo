using SkiaSharp;
using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;
using Microsoft.Maui.Controls;
using System.IO;
using System;
using System.Threading.Tasks;

namespace SkiaSharpTest.Controls
{
    public class RowImageButton : SKCanvasView
    {
        private SKImage? _bitmap;
        private static SKTypeface? s_diabloTypeface;
        
        public string Title { get; set; } = "Snapshot Name";
        public string Subtitle { get; set; } = "Level 1 - Dungeons of Doom";

        static RowImageButton()
        {
            var fontPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "Fonts", "diablo_h.ttf");
            if (File.Exists(fontPath))
            {
                s_diabloTypeface = SKTypeface.FromFile(fontPath);
            }
        }

        public RowImageButton()
        {
            PaintSurface += OnPaintSurface;
            LoadImage();
        }

        private async void LoadImage()
        {
            try 
            {
                using var stream = await FileSystem.OpenAppPackageFileAsync("you.png");
                using var data = SKData.Create(stream);
                _bitmap = SKImage.FromEncodedData(data);
                InvalidateSurface();
            }
            catch { }
        }

        private void OnPaintSurface(object? sender, SKPaintSurfaceEventArgs e)
        {
            var canvas = e.Surface.Canvas;
            canvas.Clear(SKColors.Transparent);

            using var paint = new SKPaint
            {
                IsAntialias = true,
                Color = SKColors.White,
            };
            
            using var font = new SKFont
            {
                Size = 32,
                Typeface = s_diabloTypeface
            };

            // Draw icon
            if (_bitmap != null)
            {
                var iconRect = new SKRect(20, 20, 100, 100);
                canvas.DrawImage(_bitmap, iconRect, SKSamplingOptions.Default);
            }

            // Draw text
            canvas.DrawText(Title, 140, 50, SKTextAlign.Left, font, paint);
            
            font.Size = 24;
            paint.Color = SKColors.LightGray;
            canvas.DrawText(Subtitle, 140, 90, SKTextAlign.Left, font, paint);
        }
    }
}
