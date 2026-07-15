using Microsoft.Maui.Controls;
using SkiaSharpTest.Controls;
using System;

namespace SkiaSharpTest.Pages
{
    public partial class SnapshotPage : ContentPage
    {
        public SnapshotPage()
        {
            InitializeComponent();
            LoadSnapshots();

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

        public void SetOpenTime(TimeSpan elapsed)
        {
            lblTime.Text = $"Opened in {elapsed.TotalMilliseconds:F0} ms";
        }

        private void LoadSnapshots()
        {
            lblSubtitle.Text = "10 snapshots taken";

            for (int i = 0; i < 10; i++)
            {
                var rib = new RowImageButton
                {
                    Title = $"Snapshot {i + 1}",
                    Subtitle = $"Dungeons of Doom Level {i + 1}",
                    HeightRequest = 140,
                    WidthRequest = 400,
                    HorizontalOptions = LayoutOptions.Center
                };

                SnapshotLayout.Children.Add(rib);
            }
        }

        private async void CloseButton_Clicked(object? sender, EventArgs e)
        {
            await Navigation.PopModalAsync();
        }
    }
}
