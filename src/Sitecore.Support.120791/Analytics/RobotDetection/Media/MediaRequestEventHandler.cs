using System;
using System.Collections.Generic;
using System.Linq;
using Sitecore.Analytics;
using Sitecore.Analytics.Configuration;
using Sitecore.Analytics.Media;
using Sitecore.Analytics.Tracking;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Events;
using Sitecore.Resources.Media;
using Sitecore.Sites;

namespace Sitecore.Support.Analytics.RobotDetection.Media
{
    [UsedImplicitly]
    public class MediaRequestEventHandler : Sitecore.Support.Analytics.Media.MediaRequestEventHandler
    {
        [UsedImplicitly]
        public override void OnMediaRequest(object sender, EventArgs args)
        {
            Assert.ArgumentNotNull(sender, "sender");
            Assert.ArgumentNotNull(args, "args");
            if (AnalyticsSettings.Enabled)
            {
                SiteContext site = Context.Site;
                if ((site != null) && site.EnableAnalytics)
                {
                    MediaRequest request = Event.ExtractParameter(args, 0) as MediaRequest;
                    if (request != null)
                    {
                        MediaRequestTrackingInformation information = new MediaRequestTrackingInformation(request);
                        Item mediaItem = information.GetMediaItem();
                        if ((mediaItem != null) && information.IsTrackedRequest())
                        {
                            ContextItemSwitcher switcher = new ContextItemSwitcher(mediaItem);
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
            }
        }
    }
}
