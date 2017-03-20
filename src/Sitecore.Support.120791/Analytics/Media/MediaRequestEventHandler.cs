using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Sitecore.Analytics;
using Sitecore.Analytics.Configuration;
using Sitecore.Analytics.Tracking;
using Sitecore.Analytics.Web;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Events;
using Sitecore.Resources.Media;
using Sitecore.Sites;

namespace Sitecore.Support.Analytics.Media
{
    [UsedImplicitly]
    public class MediaRequestEventHandler : Sitecore.Analytics.Media.MediaRequestEventHandler
    {
        private static Action SaveAndReleaseCurrentContactDelegate = (typeof(Sitecore.Analytics.Media.MediaRequestEventHandler).GetMethod("SaveAndReleaseCurrentContact", BindingFlags.NonPublic | BindingFlags.Static).CreateDelegate(typeof(Action)) as Action);

        [UsedImplicitly]
        public override void OnMediaRequest(object sender, EventArgs args)
        {
            ContextItemSwitcher switcher;
            Assert.ArgumentNotNull(sender, "sender");
            Assert.ArgumentNotNull(args, "args");
            if (!AnalyticsSettings.Enabled)
            {
                return;
            }
            SiteContext site = Context.Site;
            if ((site == null) || !site.EnableAnalytics)
            {
                return;
            }
            MediaRequest request = Event.ExtractParameter(args, 0) as MediaRequest;
            if (request == null)
            {
                return;
            }
            MediaRequestTrackingInformationOptimized optimized = new MediaRequestTrackingInformationOptimized(request);
            Item mediaItem = optimized.GetMediaItem();
            if (mediaItem == null)
            {
                return;
            }
            if (!optimized.IsTrackedRequest())
            {
                ContactKeyCookie cookie = new ContactKeyCookie("");
                if (!cookie.IsClassificationGuessed)
                {
                    Tracker.Initialize();
                    if (((Tracker.Current == null) || (Tracker.Current.Session == null)) || Tracker.Current.Session.Settings.IsNew)
                    {
                        goto Label_00CA;
                    }
                    Tracker.Current.Session.SetClassification(0, 0, true);
                    cookie.IsClassificationGuessed = true;
                    cookie.Update();
                    SaveAndReleaseCurrentContactDelegate();
                }
                return;
            }
            Label_00CA:
            switcher = new ContextItemSwitcher(mediaItem);
            try
            {
                this.StartTracking();
                Assert.IsNotNull(Tracker.Current, "Tracker.Current is not initialized");
                Assert.IsNotNull(Tracker.Current.Session, "Tracker.Current.Session is not initialized");
                Assert.IsNotNull(Tracker.Current.Session.Interaction, "Tracker.Current.Session.Interaction is not initialized");
                Assert.IsNotNull(Tracker.Current.Session.Interaction.CurrentPage, "Tracker.Current.Session.Interaction is not initialized");
                IPageContext previousPage = Tracker.Current.Session.Interaction.PreviousPage;
                if (previousPage != null)
                {
                    if (Tracker.Current.Session.Interaction.CurrentPage.PageEvents.Any<Sitecore.Analytics.Model.PageEventData>())
                    {
                        IEnumerable<Sitecore.Analytics.Data.PageEventData> pageEvents = from i in Tracker.Current.Session.Interaction.CurrentPage.PageEvents select new Sitecore.Analytics.Data.PageEventData(i);
                        previousPage.RegisterEvents(pageEvents);
                    }
                    Tracker.Current.Session.Interaction.CurrentPage.Cancel();
                    SaveAndReleaseCurrentContactDelegate();
                }
            }
            catch (Exception exception)
            {
                Log.Error("Media request analytics failed", exception, base.GetType());
            }
            finally
            {
                if (switcher != null)
                {
                    switcher.Dispose();
                }
            }
        }
    }
}
