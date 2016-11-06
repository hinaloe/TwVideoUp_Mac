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
	[Register ("AuthSheetViewController")]
	partial class AuthSheetViewController
	{
		[Outlet]
		WebKit.WebView authWebView { get; set; }

		[Action ("CancelSheet:")]
		partial void CancelSheet (Foundation.NSObject sender);

		[Action ("ReloadAuthView:")]
		partial void ReloadAuthView (Foundation.NSObject sender);
		
		void ReleaseDesignerOutlets ()
		{
			if (authWebView != null) {
				authWebView.Dispose ();
				authWebView = null;
			}
		}
	}
}
