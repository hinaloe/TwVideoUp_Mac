using System;
using System.IO;
using System.Threading.Tasks;
using AppKit;
using AVFoundation;
using CoreTweet;
using Foundation;
using TwVideoUp_Mac.Twitter;

namespace TwVideoUp_Mac
{
    public partial class ViewController : NSViewController
    {
        private string _selectedUrl;

        public ViewController(IntPtr handle) : base(handle)
        {
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            // Do any additional setup after loading the view.
            //PerformSegue("SheetSegue", this);
        }

        public override NSObject RepresentedObject
        {
            get { return base.RepresentedObject; }
            set
            {
                base.RepresentedObject = value;
                // Update the view, if already loaded.
            }
        }

        public override void ViewWillAppear()
        {
            base.ViewWillAppear();

            var userDefaults = NSUserDefaults.StandardUserDefaults;
            if (userDefaults.StringForKey(Config.CONFIG_TOKEN) == null)
            {
                PerformSegue("SheetSegue", this);
            }
            else
            {
                TokenStore.Tokens = Tokens.Create(Config.CONSUMER_KEY, Config.CONSUMER_SECRET,
                    userDefaults.StringForKey(Config.CONFIG_TOKEN), userDefaults.StringForKey(Config.CONFIG_SECRET));
            }
        }

        public override void PrepareForSegue(NSStoryboardSegue segue, NSObject sender)
        {
            base.PrepareForSegue(segue, sender);

            switch (segue.Identifier)
            {
                case "SheetSegue":
                    var sheet = segue.DestinationController as AuthSheetViewController;
                    sheet.Presentor = this;
                    sheet.SheetCanceled += (o, args) =>
                    {
                        var ac = o as AuthSheetViewController;
                        var userDefaults = NSUserDefaults.StandardUserDefaults;
                        // if token not have
                        if (userDefaults.StringForKey(Config.CONFIG_TOKEN) == null)
                        {
                            var alert = new NSAlert {MessageText = "アカウントの設定が出来ていません。", InformativeText = "終了しますか？"};
                            alert.AddButton("Yes").Tag = 1;
                            alert.AddButton("No").Tag = 0;

                            var quit = alert.RunSheetModal(ac.View.Window);
                            if (quit != 0)
                            {
                                ac.WillClose = ac.WillQuit = true;
                            }
                            else
                            {
                                ac.WillClose = false;
                            }
                        }
                    };
                    break;
            }
        }

        [Export("openDocument:")]
        void OpenDialog(NSObject sender)
        {
            ClickPick(sender);
        }


        partial void ClickPick(NSObject sender)
        {
            NSOpenPanel openPanel = new NSOpenPanel {AllowedFileTypes = new[] {"mp4"}};
            var result = openPanel.RunModal();
            if (result == 0)
            {
                Console.WriteLine("Canceled");
                return;
            }
            _selectedUrl = openPanel.Filename;
            var player = new AVPlayer(openPanel.Url);
            playerView.Player = player;
        }

        partial void ClickSubmit(NSObject sender)
        {
            if (_selectedUrl == null)
            {
                new NSAlert {MessageText = "Error", InformativeText = "Media not selected"}.RunModal();
                return;
            }
            TweetWithField().ConfigureAwait(true);
        }

        [Export("logout:")]
        void LogoutClick(NSMenuItem sender)
        {
            Console.WriteLine("Logout requested.");
            var userDefaults = NSUserDefaults.StandardUserDefaults;
            userDefaults.RemoveObject(Config.CONFIG_TOKEN);
            userDefaults.RemoveObject(Config.CONFIG_SECRET);
            userDefaults.Synchronize();

            PerformSegue("SheetSegue", this);
        }

        private async Task<StatusResponse> UploadMedia(string url, string tweet,
            IProgress<UploadChunkedProgressInfo> progress)
        {
            if (url == null) throw new ArgumentNullException(nameof(url));
            var token = TokenStore.Tokens;
            if (token == null)
            {
                throw new ApplicationException("Unauthorized!");
            }
            var media =
                await token.Media.UploadChunkedAsync(File.OpenRead(url), UploadMediaType.Video,
                    media_category: "tweet_video", progress: progress);
            return await token.Statuses.UpdateAsync(tweet, media_ids: new[] {media.MediaId});
        }

        private async Task TweetWithField()
        {
            try
            {
                progressIndicator.Hidden = false;
                submitButton.Enabled = false;
                var res =
                    await UploadMedia(_selectedUrl, tweetField.StringValue, new Progress<UploadChunkedProgressInfo>(e =>
                    {
                        Console.WriteLine("{0}/{1}", e.BytesSent, e.TotalBytesToSend);
                        progressIndicator.MaxValue = e.TotalBytesToSend;
                        progressIndicator.DoubleValue = e.BytesSent;
                    }));
                Console.WriteLine(res.Text);
                tweetField.StringValue = string.Empty;
                _selectedUrl = null;
            }
            catch (TwitterException ex)
            {
                new NSAlert {MessageText = ex.Status.ToString(), InformativeText = ex.Message}.RunModal();
            }
            catch (ParsingException ex)
            {
                Console.WriteLine(ex.Json);
                Console.Error.WriteLine(ex);
                new NSAlert {MessageText = "ParseError", InformativeText = ex.Message}.RunModal();
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
                new NSAlert {MessageText = "Error", InformativeText = ex.Message}.RunModal();
            }
            finally
            {
                submitButton.Enabled = true;
                progressIndicator.Hidden = true;
            }
        }
    }
}