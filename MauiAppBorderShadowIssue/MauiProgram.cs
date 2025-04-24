using Microsoft.Extensions.Logging;
using Microsoft.Maui.Handlers;

#if ANDROID
using Android.Util;
#endif

namespace MauiAppBorderShadowIssue;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

#if DEBUG
		builder.Logging.AddDebug();
#endif

        BorderHandler.Mapper.AppendToMapping("TouchableBackground", (handler, view) =>
        {
#if ANDROID
            var outValue = new TypedValue();
            var gestureRecognizers = ((Border)view.Handler.VirtualView).GestureRecognizers;
            if (gestureRecognizers.Any())
            {
                handler.PlatformView.Context!.Theme!.ResolveAttribute(Android.Resource.Attribute.SelectableItemBackground, outValue, true);
                handler.PlatformView.Foreground = handler.PlatformView.Context.GetDrawable(outValue.ResourceId);

                //Setting the Clickable Property to False makes it work as well, but it doesn't remove the 'clicked' effect anymore
                handler.PlatformView.Clickable = true;//false;
                handler.PlatformView.Focusable = true;

                handler.PlatformView.Touch += (sender, e) =>
                {
                    var nativeControl = (Android.Views.View?)sender;
                    if (nativeControl != null && e.Event != null)
                    {
                        switch (e.Event.Action)
                        {
                            case Android.Views.MotionEventActions.Down:
                                nativeControl.Foreground.SetHotspot(e.Event.GetX(), e.Event.GetY());
                                nativeControl.Pressed = true;
                                break;
                            case Android.Views.MotionEventActions.Up:
                                //Uncomment this to make the gesturerecognizer work with the workaround i made
                                //var tapGestureRecognizer =
                                //    gestureRecognizers.OfType<TapGestureRecognizer>().FirstOrDefault();
                                //tapGestureRecognizer?.Command?.Execute(tapGestureRecognizer.CommandParameter);
                                nativeControl.Pressed = false;
                                break;
                            case Android.Views.MotionEventActions.Cancel:
                            case Android.Views.MotionEventActions.Outside:
                                nativeControl.Pressed = false;
                                break;
                        }

                        e.Handled = false;
                    }
                };
            }
            ;
#endif
        });

        return builder.Build();
	}
}
