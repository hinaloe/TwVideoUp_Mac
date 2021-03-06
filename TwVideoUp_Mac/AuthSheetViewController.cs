using System;
using System.Collections.Specialized;
using Foundation;
using AppKit;
using CoreTweet;
using TwVideoUp_Mac.Twitter;

namespace TwVideoUp_Mac
{
    public partial class AuthSheetViewController : NSViewController
    {
        #region Private Variables

        private OAuth.OAuthSession _session;

        #endregion

        #region Computed Properties

        public NSViewController Presentor { get; set; }

        public bool WillClose { get; set; } = true;
        public bool WillQuit { get; set; } = false;

        #endregion

        public AuthSheetViewController(IntPtr handle) : base(handle)
        {
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            // Do any additional setup after loading the view.
            BrowseAuthorizeView();
            authWebView.WillPerformClientRedirect += (sender, args) =>
            {
                Console.WriteLine(args.ToUrl);
                if (args.ToUrl.AbsoluteString.StartsWith(Config.CALLBACK))
                {
                    Console.WriteLine(args.ToUrl.Query);
                    var query = System.Web.HttpUtility.ParseQueryString(args.ToUrl.Query);
                    GetTokens(query);
                }
            };
        }

        private async void BrowseAuthorizeView()
        {
            _session = await OAuth.AuthorizeAsync(Config.CONSUMER_KEY, Config.CONSUMER_SECRET, Config.CALLBACK);
            authWebView.MainFrameUrl = _session.AuthorizeUri.AbsoluteUri;
        }

        private async void GetTokens(NameValueCollection query)
        {
            try
            {
                TokenStore.Tokens = await _session.GetTokensAsync(query["oauth_verifier"]);
                var userDefaults = NSUserDefaults.StandardUserDefaults;
                userDefaults.SetString(TokenStore.Tokens.AccessToken, Config.CONFIG_TOKEN);
                userDefaults.SetString(TokenStore.Tokens.AccessTokenSecret, Config.CONFIG_SECRET);
                Console.WriteLine(userDefaults.Synchronize());

                CloseSheet();
            }
            catch (Exception ex)
            {
                new NSAlert {MessageText = ex.Message}.RunSheetModal(View.Window);
                BrowseAuthorizeView();
            }
        }

        private void CloseSheet()
        {
            Presentor.DismissViewController(this);
        }

        partial void CancelSheet(NSObject sender)
        {
            RaiseSheetCanceled();
            if (WillClose)
            {
                CloseSheet();
            }
            if (WillQuit)
            {
                NSApplication.SharedApplication.Terminate(this);
            }
        }

        partial void ReloadAuthView(NSObject sender)
        {
            BrowseAuthorizeView();
        }


        public EventHandler SheetCanceled;

        internal void RaiseSheetCanceled()
        {
            SheetCanceled?.Invoke(this, EventArgs.Empty);
        }
    }
}