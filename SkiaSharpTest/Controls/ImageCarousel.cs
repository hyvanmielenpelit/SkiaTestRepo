using SkiaSharp;
using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;
using Microsoft.Maui.Controls;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System;

namespace SkiaSharpTest.Controls
{
    public struct CarouselBitmap
    {
        public string ResourcePath;
        public SKImage? Bitmap;
        public TextAlignment HorizontalFitAlignment;
        public TextAlignment VerticalFitAlignment;

        public CarouselBitmap(string resourcePath, TextAlignment hAlign, TextAlignment vAlign)
        {
            ResourcePath = resourcePath;
            HorizontalFitAlignment = hAlign;
            VerticalFitAlignment = vAlign;
            Bitmap = null;
        }
    }

    public class ImageCarousel : SKCanvasView
    {
        public ImageCarousel() : base()
        {
            PaintSurface += Base_PaintSurface;
            Init();
            Play();
        }

        CarouselBitmap[] _caruselBitmaps = new CarouselBitmap[]
        {
            new CarouselBitmap("main-menu-ranger.jpg", TextAlignment.Center, TextAlignment.Center),
            new CarouselBitmap("main-menu-dwarf.jpg", TextAlignment.Center, TextAlignment.Center),
            new CarouselBitmap("main-menu-gnoll.jpg", TextAlignment.Center, TextAlignment.Center),
        };

        private bool _inited = false;
        public async void Init()
        {
            if (_inited)
                return;

            for (int i = 0; i < _caruselBitmaps.Length; i++)
            {
                try
                {
                    using Stream stream = await FileSystem.OpenAppPackageFileAsync(_caruselBitmaps[i].ResourcePath);
                    using SKData data = SKData.Create(stream);
                    _caruselBitmaps[i].Bitmap = SKImage.FromEncodedData(data);
                }
                catch { }
            }
            _inited = true; 
        }

        public void ShutDown()
        {
            if (_inited)
            {
                Stop();
                for (int i = 0; i < _caruselBitmaps.Length; i++)
                {
                    if (_caruselBitmaps[i].Bitmap != null)
                    {
                        _caruselBitmaps[i].Bitmap?.Dispose();
                        _caruselBitmaps[i].Bitmap = null;
                    }
                }
                _inited = false;
            }
        }

        public readonly long _refreshFrequency = 60; // 60fps
        public const long _slideDurationInMilliseconds = 5000;
        public const long _transitionDurationInMilliseconds = 5000;

        private long _counterValue;
        public long CounterValue { get { return Interlocked.CompareExchange(ref _counterValue, 0L, 0L); } set { Interlocked.Exchange(ref _counterValue, value); } }

        private int _timerIsOn = 0;
        public bool CheckTimerOnAndSetTrue { get { return Interlocked.Exchange(ref _timerIsOn, 1) != 0; } } 
        public bool TimerIsOn { get { return Interlocked.CompareExchange(ref _timerIsOn, 0, 0) != 0; } set { Interlocked.Exchange(ref _timerIsOn, value ? 1 : 0); } }
        
        public void Play()
        {
            if(!CheckTimerOnAndSetTrue)
            {
                if (Microsoft.Maui.Controls.Application.Current != null && Microsoft.Maui.Controls.Application.Current.Dispatcher != null)
                {
                    var timer = Microsoft.Maui.Controls.Application.Current.Dispatcher.CreateTimer();
                    timer.Interval = TimeSpan.FromSeconds(1.0 / _refreshFrequency);
                    timer.IsRepeating = true;
                    timer.Tick += (s, e) => { DoIncrementCounter(); if (!TimerIsOn) timer.Stop(); };
                    timer.Start();
                }
            }
        }

        private void DoIncrementCounter()
        {
            if (MainThread.IsMainThread)
            {
                long counter = Interlocked.Increment(ref _counterValue);
                if (counter == long.MaxValue)
                {
                    Interlocked.Exchange(ref _counterValue, 0);
                    counter = 0;
                }
                byte alpha = GetSecondBitmapAlpha(counter);
                byte prevalpha = GetSecondBitmapAlpha(counter - 1);
                if (alpha > 0 || prevalpha > 0)
                    InvalidateSurface();
            }
            else
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    long counter = Interlocked.Increment(ref _counterValue);
                    if (counter == long.MaxValue)
                    {
                        Interlocked.Exchange(ref _counterValue, 0);
                        counter = 0;
                    }
                    byte alpha = GetSecondBitmapAlpha(counter);
                    byte prevalpha = GetSecondBitmapAlpha(counter - 1);
                    if (alpha > 0 || prevalpha > 0)
                        InvalidateSurface();
                });
            }
        }

        public void Stop()
        {
            TimerIsOn = false;
        }

        private double _currentWidth = 0;
        private double _currentHeight = 0;
        protected override void OnSizeAllocated(double width, double height)
        {
            base.OnSizeAllocated(width, height);
            if (width != _currentWidth || height != _currentHeight)
            {
                _currentWidth = width;
                _currentHeight = height;
                InvalidateSurface();
            }
        }

        private bool _initialDraw = true;

        private void Base_PaintSurface(object? sender, SKPaintSurfaceEventArgs e)
        {
            if (!_initialDraw && !TimerIsOn)
                return;

            SKImageInfo info = e.Info;
            SKSurface surface = e.Surface;
            SKCanvas canvas = surface.Canvas;
            float canvaswidth = info.Width;
            float canvasheight = info.Height;

            canvas.Clear();

            int cnt = _caruselBitmaps.Length;
            if (cnt > 0)
            {
                SKRect sourceRect, targetRect;
                float source_width, source_height;
                float target_x, target_y, target_width, target_height;
                float scale_x, scale_y, scale;
                TextAlignment halign;
                TextAlignment valign;
                using (SKPaint bmpPaint = new SKPaint())
                {
                    int idx = GetFirstBitmapIndex();
                    if(_caruselBitmaps[idx].Bitmap != null)
                    {
                        source_width = (float)Math.Max(1, _caruselBitmaps[idx].Bitmap!.Width);
                        source_height = (float)Math.Max(1, _caruselBitmaps[idx].Bitmap!.Height);
                        sourceRect = new SKRect(0, 0, source_width, source_height);
                        scale_x = canvaswidth / source_width;
                        scale_y = canvasheight / source_height;
                        scale = Math.Max(scale_x, scale_y);
                        target_width = scale * source_width;
                        target_height = scale * source_height;
                        halign = _caruselBitmaps[idx].HorizontalFitAlignment;
                        valign = _caruselBitmaps[idx].VerticalFitAlignment;
                        
                        target_x = halign == TextAlignment.Start ? 0 : halign == TextAlignment.End ? (canvaswidth - target_width) : (canvaswidth - target_width) / 2;
                        target_y = valign == TextAlignment.Start ? 0 : valign == TextAlignment.End ? (canvasheight - target_height) : (canvasheight - target_height) / 2;
                        
                        targetRect = new SKRect(target_x, target_y, target_x + target_width, target_y + target_height);
                        canvas.DrawImage(_caruselBitmaps[idx].Bitmap, sourceRect, targetRect, SKSamplingOptions.Default, bmpPaint);

                        if (cnt > 1)
                        {
                            byte alpha = GetSecondBitmapAlpha(CounterValue);
                            if (alpha > 0)
                            {
                                int idx2 = GetSecondBitmapIndex();
                                if (_caruselBitmaps[idx2].Bitmap != null)
                                {
                                    source_width = (float)Math.Max(1, _caruselBitmaps[idx2].Bitmap!.Width);
                                    source_height = (float)Math.Max(1, _caruselBitmaps[idx2].Bitmap!.Height);
                                    sourceRect = new SKRect(0, 0, source_width, source_height);
                                    scale_x = canvaswidth / source_width;
                                    scale_y = canvasheight / source_height;
                                    scale = Math.Max(scale_x, scale_y);
                                    target_width = scale * source_width;
                                    target_height = scale * source_height;
                                    halign = _caruselBitmaps[idx2].HorizontalFitAlignment;
                                    valign = _caruselBitmaps[idx2].VerticalFitAlignment;
                                    
                                    target_x = halign == TextAlignment.Start ? 0 : halign == TextAlignment.End ? (canvaswidth - target_width) : (canvaswidth - target_width) / 2;
                                    target_y = valign == TextAlignment.Start ? 0 : valign == TextAlignment.End ? (canvasheight - target_height) : (canvasheight - target_height) / 2;
                                    
                                    targetRect = new SKRect(target_x, target_y, target_x + target_width, target_y + target_height);

                                    byte oldalpha = bmpPaint.Color.Alpha;
                                    bmpPaint.Color = bmpPaint.Color.WithAlpha(alpha);
                                    canvas.DrawImage(_caruselBitmaps[idx2].Bitmap, sourceRect, targetRect, SKSamplingOptions.Default, bmpPaint);
                                    bmpPaint.Color = bmpPaint.Color.WithAlpha(oldalpha);
                                }
                            }
                        }
                    }
                }
            }
            _initialDraw = false;
        }

        private int GetFirstBitmapIndex()
        {
            int cnt = _caruselBitmaps.Length;
            if(cnt == 0)
                return 0;
            long idx = CounterValue * 1000 / (_slideDurationInMilliseconds * _refreshFrequency);
            return (int)idx % cnt;
        }

        private int GetSecondBitmapIndex()
        {
            int cnt = _caruselBitmaps.Length;
            if (cnt == 0)
                return 0;
            long firstidx = GetFirstBitmapIndex();
            return (int)(firstidx + 1) % cnt;
        }

        private byte GetSecondBitmapAlpha(long counter)
        {
            long slidedurationinticks = (_slideDurationInMilliseconds * _refreshFrequency) / 1000;
            long transitiondurationinticks = (_transitionDurationInMilliseconds * _refreshFrequency) / 1000;
            long countercycleticks = counter % slidedurationinticks;
            double opacity = Math.Min(1.0, (double)Math.Max(0, transitiondurationinticks - (slidedurationinticks - countercycleticks)) / (double)transitiondurationinticks);
            return (byte)(255.0 * opacity);
        }
    }
}
