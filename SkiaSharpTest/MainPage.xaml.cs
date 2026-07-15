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

        private int _clickState = 0;

        private void OnCarouselTapped(object sender, EventArgs e)
        {
            _clickState++;
            if (_clickState > 5)
                _clickState = 1;

            switch (_clickState)
            {
                case 1:
                    CenterLogo.IsVisible = false;
                    TopLeftLogo.IsVisible = false;
                    SettingsContainer.IsVisible = false;
                    ModesContainer.IsVisible = false;
                    PlayGameContainer.IsVisible = false;
                    break;
                case 2:
                    PlayGameContainer.IsVisible = true;
                    break;
                case 3:
                    ModesContainer.IsVisible = true;
                    break;
                case 4:
                    SettingsContainer.IsVisible = true;
                    break;
                case 5:
                    CenterLogo.IsVisible = true;
                    TopLeftLogo.IsVisible = true;
                    break;
            }
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
