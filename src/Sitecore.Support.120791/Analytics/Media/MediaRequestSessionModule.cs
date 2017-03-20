using System.Web;
using Sitecore.Analytics.Web;
using Sitecore.Resources.Media;

namespace Sitecore.Support.Analytics.Media
{
    public class MediaRequestSessionModule : Sitecore.Analytics.Media.MediaRequestSessionModule
    {
        protected override bool IsSessionRequired()
        {
            if (!this.IsTrackerEnabled())
            {
                return false;
            }
            MediaRequest request = MediaManager.ParseMediaRequest(HttpContext.Current.Request);
            if (request == null)
            {
                return false;
            }
            MediaRequestTrackingInformationOptimized optimized = new MediaRequestTrackingInformationOptimized(request);
            if (optimized.IsTrackedRequest())
            {
                return true;
            }
            ContactKeyCookie cookie = new ContactKeyCookie("");
            return !cookie.IsClassificationGuessed;
        }
    }
}
