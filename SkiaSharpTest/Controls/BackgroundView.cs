using SkiaSharp;
using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;
using Microsoft.Maui.Controls;
using System.IO;
using System.Threading.Tasks;
using System;

namespace SkiaSharpTest.Controls
{
    public class BackgroundView : SKCanvasView
    {
        private SKImage? _bitmap;
        private SKImage? _bordertl;
        private SKImage? _borderhorizontal;
        private SKImage? _bordervertical;

        public BackgroundView()
        {
            PaintSurface += OnPaintSurface;
            LoadImages();
        }

        private async void LoadImages()
        {
            try 
            {
                using var stream = await FileSystem.OpenAppPackageFileAsync("background-oldpaper.png");
                using var data = SKData.Create(stream);
                _bitmap = SKImage.FromEncodedData(data);

                using var tstream = await FileSystem.OpenAppPackageFileAsync("frame-topleft.png");
                using var tdata = SKData.Create(tstream);
                _bordertl = SKImage.FromEncodedData(tdata);

                using var hstream = await FileSystem.OpenAppPackageFileAsync("frame-horizontal.png");
                using var hdata = SKData.Create(hstream);
                _borderhorizontal = SKImage.FromEncodedData(hdata);

                using var vstream = await FileSystem.OpenAppPackageFileAsync("frame-vertical.png");
                using var vdata = SKData.Create(vstream);
                _bordervertical = SKImage.FromEncodedData(vdata);

                InvalidateSurface();
            } 
            catch { }
        }

        private void OnPaintSurface(object? sender, SKPaintSurfaceEventArgs e)
        {
            var canvas = e.Surface.Canvas;
            canvas.Clear(SKColors.Black);

            float canvaswidth = e.Info.Width;
            float canvasheight = e.Info.Height;

            using (SKPaint paint = new SKPaint())
            {
                if (_bitmap != null)
                {
                    float bmpWidth = _bitmap.Width;
                    float bmpHeight = _bitmap.Height;

                    if (bmpWidth > 0 && bmpHeight > 0)
                    {
                        int xTiles = (int)Math.Ceiling(canvaswidth / bmpWidth);
                        int yTiles = (int)Math.Ceiling(canvasheight / bmpHeight);
                        SKRect sourceRect = new SKRect();
                        SKRect targetRect = new SKRect();
                        for (int x = 0; x < xTiles; x++)
                        {
                            for (int y = 0; y < yTiles; y++)
                            {
                                targetRect.Left = x * bmpWidth;
                                targetRect.Top = y * bmpHeight;
                                targetRect.Right = Math.Min(canvaswidth, (x + 1) * bmpWidth);
                                targetRect.Bottom = Math.Min(canvasheight, (y + 1) * bmpHeight);
                                sourceRect.Left = 0;
                                sourceRect.Top = 0;
                                sourceRect.Right = targetRect.Width;
                                sourceRect.Bottom = targetRect.Height;
                                canvas.DrawImage(_bitmap, sourceRect, targetRect, SKSamplingOptions.Default, paint);
                            }
                        }
                    }
                }

                if (_bordertl != null && _borderhorizontal != null && _bordervertical != null)
                {
                    float borderscalex = (canvaswidth / 8.0f / 6.0f) / Math.Max(1, _bordervertical.Width);
                    float borderscaley = (canvasheight / 8.0f / 6.0f) / Math.Max(1, _borderhorizontal.Height);
                    float borderscale = Math.Max(0.10f, Math.Min(10.0f, (float)Math.Sqrt(borderscalex * borderscaley)));

                    for (int i = 0; i < 4; i++)
                    {
                        float tx = 0, ty = 0;
                        bool hflip = false, vflip = false;
                        using (SKAutoCanvasRestore res = new SKAutoCanvasRestore(canvas, true))
                        {
                            switch (i)
                            {
                                case 0:
                                    tx = 0;
                                    ty = 0;
                                    break;
                                case 1:
                                    tx = canvaswidth - _bordertl.Width * borderscale;
                                    ty = 0;
                                    hflip = true;
                                    break;
                                case 2:
                                    tx = canvaswidth - _bordertl.Width * borderscale;
                                    ty = canvasheight - _bordertl.Height * borderscale;
                                    hflip = true;
                                    vflip = true;
                                    break;
                                case 3:
                                    tx = 0;
                                    ty = canvasheight - _bordertl.Height * borderscale;
                                    vflip = true;
                                    break;
                            }
                            SKRect target_rect = new SKRect(0, 0, _bordertl.Width * borderscale, _bordertl.Height * borderscale);
                            canvas.Translate(tx + (hflip ? target_rect.Width : 0), ty + (vflip ? target_rect.Height : 0));
                            canvas.Scale(hflip ? -1 : 1, vflip ? -1 : 1, 0, 0);
                            canvas.DrawImage(_bordertl, target_rect, SKSamplingOptions.Default, paint);
                        }
                    }

                    for (int i = 0; i < 2; i++)
                    {
                        float tx = _bordertl.Width * borderscale, ty = 0;
                        bool hflip = false, vflip = false;
                        using (SKAutoCanvasRestore res = new SKAutoCanvasRestore(canvas, true))
                        {
                            if (i == 1)
                            {
                                ty = canvasheight - _borderhorizontal.Height * borderscale;
                                vflip = true;
                            }
                            SKRect target_rect = new SKRect(0, 0, canvaswidth - _bordertl.Width * borderscale - tx, _borderhorizontal.Height * borderscale);
                            canvas.Translate(tx + (hflip ? target_rect.Width : 0), ty + (vflip ? target_rect.Height : 0));
                            canvas.Scale(hflip ? -1 : 1, vflip ? -1 : 1, 0, 0);
                            canvas.DrawImage(_borderhorizontal, target_rect, SKSamplingOptions.Default, paint);
                        }
                    }

                    for (int i = 0; i < 2; i++)
                    {
                        float tx = 0, ty = _bordertl.Height * borderscale;
                        bool hflip = false, vflip = false;
                        using (SKAutoCanvasRestore res = new SKAutoCanvasRestore(canvas, true))
                        {
                            if (i == 1)
                            {
                                tx = canvaswidth - _bordervertical.Width * borderscale;
                                hflip = true;
                            }
                            SKRect target_rect = new SKRect(0, 0, _bordervertical.Width * borderscale, canvasheight - _bordertl.Height * borderscale - ty);
                            canvas.Translate(tx + (hflip ? target_rect.Width : 0), ty + (vflip ? target_rect.Height : 0));
                            canvas.Scale(hflip ? -1 : 1, vflip ? -1 : 1, 0, 0);
                            canvas.DrawImage(_bordervertical, target_rect, SKSamplingOptions.Default, paint);
                        }
                    }
                }
            }
        }
    }
}
