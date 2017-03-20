using System.Web;
using Sitecore.Resources.Media;
using Sitecore.Support.Analytics.Media;

namespace Sitecore.Support.Analytics.RobotDetection.Media
{
    public class MediaRequestSessionModule : Sitecore.Support.Analytics.Media.MediaRequestSessionModule
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
            return optimized.IsTrackedRequest();
        }
    }
}
