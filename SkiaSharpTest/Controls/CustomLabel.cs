using SkiaSharp;
using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using System;
using System.IO;

namespace SkiaSharpTest.Controls
{
    public class CustomLabel : SKCanvasView
    {
        public static readonly BindableProperty TextProperty = BindableProperty.Create(nameof(Text), typeof(string), typeof(CustomLabel), string.Empty, propertyChanged: OnPropertyChanged);
        public static readonly BindableProperty FontFamilyProperty = BindableProperty.Create(nameof(FontFamily), typeof(string), typeof(CustomLabel), string.Empty, propertyChanged: OnPropertyChanged);
        public static readonly BindableProperty FontSizeProperty = BindableProperty.Create(nameof(FontSize), typeof(double), typeof(CustomLabel), 14.0, propertyChanged: OnPropertyChanged);
        public static readonly BindableProperty TextColorProperty = BindableProperty.Create(nameof(TextColor), typeof(Color), typeof(CustomLabel), Colors.White, propertyChanged: OnPropertyChanged);

        public string Text { get => (string)GetValue(TextProperty); set => SetValue(TextProperty, value); }
        public string FontFamily { get => (string)GetValue(FontFamilyProperty); set => SetValue(FontFamilyProperty, value); }
        public double FontSize { get => (double)GetValue(FontSizeProperty); set => SetValue(FontSizeProperty, value); }
        public Color TextColor { get => (Color)GetValue(TextColorProperty); set => SetValue(TextColorProperty, value); }

        private SKTypeface? _typeface;

        private static void OnPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is CustomLabel label)
            {
                if (oldValue != newValue && bindable is CustomLabel cl && newValue is string str && propertyChangedFontFamily(oldValue, newValue))
                {
                    cl.UpdateTypeface();
                }
                label.InvalidateSurface();
            }
        }

        private static bool propertyChangedFontFamily(object oldValue, object newValue)
        {
            return true; // We just reload or use default.
        }

        public CustomLabel()
        {
            IgnorePixelScaling = true;
            PaintSurface += OnPaintSurface;
        }

        private void UpdateTypeface()
        {
            if (!string.IsNullOrEmpty(FontFamily))
            {
                // Simple hardcoded map or just check Diablo
                if (FontFamily.ToLower().Contains("diablo"))
                {
                    var fontPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "diablo_h.ttf");
                    if (File.Exists(fontPath))
                    {
                        _typeface = SKTypeface.FromFile(fontPath);
                    }
                }
                else if (FontFamily.ToLower().Contains("immortal"))
                {
                    var fontPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "IMMORTAL.ttf");
                    if (File.Exists(fontPath))
                    {
                        _typeface = SKTypeface.FromFile(fontPath);
                    }
                }
            }
        }

        private void OnPaintSurface(object? sender, SKPaintSurfaceEventArgs e)
        {
            var canvas = e.Surface.Canvas;
            canvas.Clear(SKColors.Transparent);

            if (string.IsNullOrEmpty(Text))
                return;

            if (_typeface == null)
                UpdateTypeface();

            using var paint = new SKPaint
            {
                IsAntialias = true,
                Color = TextColor.ToSKColor(),
            };

            using var font = new SKFont
            {
                Size = (float)FontSize,
                Typeface = _typeface
            };

            // Draw centered
            var bounds = new SKRect();
            font.MeasureText(Text, out bounds, paint);

            float x = (e.Info.Width - bounds.Width) / 2.0f;
            float y = (e.Info.Height - bounds.Height) / 2.0f - bounds.Top;

            canvas.DrawText(Text, x, y, SKTextAlign.Left, font, paint);
        }
    }
}
