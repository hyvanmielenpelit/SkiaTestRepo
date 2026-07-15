using Microsoft.Maui.Controls;
using SkiaSharpTest.Pages;
using System;

namespace SkiaSharpTest
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();

            this.HandlerChanged += (sender, e) =>
            {
#if WINDOWS
                var p = this.Handler?.PlatformView as Microsoft.UI.Xaml.Controls.Panel;
                if (p != null)
                {
                    p.Transitions = new Microsoft.UI.Xaml.Media.Animation.TransitionCollection()
                    {
                        new Microsoft.UI.Xaml.Media.Animation.EntranceThemeTransition()
                    };
                }
#endif
            };
        }

        private async void OnOpenModalClicked(object? sender, EventArgs e)
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();
            var page = new SnapshotPage();
            await Navigation.PushModalAsync(page, true);
            sw.Stop();
            page.SetOpenTime(sw.Elapsed);
        }
    }
}
