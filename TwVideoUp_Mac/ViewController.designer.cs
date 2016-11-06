// WARNING
//
// This file has been generated automatically by Xamarin Studio to store outlets and
// actions made in the UI designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using Foundation;
using System.CodeDom.Compiler;

namespace TwVideoUp_Mac
{
	[Register ("ViewController")]
	partial class ViewController
	{
		[Outlet]
		AVKit.AVPlayerView playerView { get; set; }

		[Outlet]
		AppKit.NSProgressIndicator progressIndicator { get; set; }

		[Outlet]
		AppKit.NSButton submitButton { get; set; }

		[Outlet]
		AppKit.NSTextField tweetField { get; set; }

		[Action ("ClickPick:")]
		partial void ClickPick (Foundation.NSObject sender);

		[Action ("ClickSubmit:")]
		partial void ClickSubmit (Foundation.NSObject sender);
		
		void ReleaseDesignerOutlets ()
		{
			if (playerView != null) {
				playerView.Dispose ();
				playerView = null;
			}

			if (progressIndicator != null) {
				progressIndicator.Dispose ();
				progressIndicator = null;
			}

			if (submitButton != null) {
				submitButton.Dispose ();
				submitButton = null;
			}

			if (tweetField != null) {
				tweetField.Dispose ();
				tweetField = null;
			}
		}
	}
}
