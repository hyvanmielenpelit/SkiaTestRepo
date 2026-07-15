namespace SafeAreaiOS;

public partial class MainPage : ContentPage
{
    private bool _insetsHack = false;

    public MainPage()
    {
        InitializeComponent();
        UpdateStatus();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (_insetsHack)
            ApplyInsetsHack(true);
    }

    protected override void OnSizeAllocated(double width, double height)
    {
        base.OnSizeAllocated(width, height);
        if (_insetsHack)
            ApplyInsetsHack(true);
    }

    private void OnToggleInsetsHack(object sender, EventArgs e)
    {
        _insetsHack = !_insetsHack;
        ApplyInsetsHack(_insetsHack);
        UpdateStatus();
    }

    private void OnDumpVCHierarchy(object sender, EventArgs e)
    {
#if IOS
        string dump = "";

        // 1. Handler-based VC
        UIKit.UIViewController handlerVc = null;
        if (this.Handler is Microsoft.Maui.IPlatformViewHandler pvh)
            handlerVc = pvh.ViewController;
        dump += $"Handler VC: {handlerVc?.GetType().Name ?? "null"}\n";
        if (handlerVc != null)
        {
            dump += $"  .NavigationController: {handlerVc.NavigationController?.GetType().Name ?? "null"}\n";
            dump += $"  .ParentViewController: {handlerVc.ParentViewController?.GetType().Name ?? "null"}\n";
        }

        // 2. Platform.GetCurrentUIViewController
        var currentVc = Microsoft.Maui.ApplicationModel.Platform.GetCurrentUIViewController();
        dump += $"\nGetCurrentUIVC: {currentVc?.GetType().Name ?? "null"}\n";
        if (currentVc != null)
        {
            dump += $"  .NavigationController: {currentVc.NavigationController?.GetType().Name ?? "null"}\n";
            dump += $"  .ParentViewController: {currentVc.ParentViewController?.GetType().Name ?? "null"}\n";
        }

        // 3. Walk the full parent chain from handler VC
        dump += "\n--- Full VC chain from Handler VC ---\n";
        var walker = handlerVc;
        int depth = 0;
        while (walker != null && depth < 10)
        {
            dump += $"  [{depth}] {walker.GetType().FullName}\n";
            dump += $"       AdditionalSafeAreaInsets: {walker.AdditionalSafeAreaInsets}\n";
            walker = walker.ParentViewController;
            depth++;
        }

        // 4. Window root VC
        var window = UIKit.UIApplication.SharedApplication.KeyWindow;
        dump += $"\nKeyWindow.RootViewController: {window?.RootViewController?.GetType().Name ?? "null"}\n";
        if (window?.RootViewController != null)
        {
            dump += $"  AdditionalSafeAreaInsets: {window.RootViewController.AdditionalSafeAreaInsets}\n";
            // Check children
            if (window.RootViewController.ChildViewControllers != null)
            {
                foreach (var child in window.RootViewController.ChildViewControllers)
                {
                    dump += $"  Child: {child.GetType().FullName}\n";
                    dump += $"    AdditionalSafeAreaInsets: {child.AdditionalSafeAreaInsets}\n";
                }
            }
        }

        // 5. Window safe area
        dump += $"\nWindow.SafeAreaInsets: {window?.SafeAreaInsets}\n";

        StatusLabel.Text = dump;
#endif
    }

    private void ApplyInsetsHack(bool enable)
    {
#if IOS
        UIKit.UIViewController vc = null;
        if (this.Handler is Microsoft.Maui.IPlatformViewHandler pvh)
            vc = pvh.ViewController;

        if (vc == null)
            return;

        if (vc.NavigationController != null)
            vc = vc.NavigationController;

        if (enable)
        {
            var window = vc.View?.Window
                ?? UIKit.UIApplication.SharedApplication.KeyWindow;
            var safeArea = window?.SafeAreaInsets ?? UIKit.UIEdgeInsets.Zero;
            vc.AdditionalSafeAreaInsets = new UIKit.UIEdgeInsets(
                -safeArea.Top, -safeArea.Left, -safeArea.Bottom, -safeArea.Right);
        }
        else
        {
            vc.AdditionalSafeAreaInsets = UIKit.UIEdgeInsets.Zero;
        }
#endif
    }

    private void UpdateStatus()
    {
        StatusLabel.Text = $"Insets Hack: {_insetsHack}\n\nTap 'Dump VC Hierarchy' to inspect";
    }
}
