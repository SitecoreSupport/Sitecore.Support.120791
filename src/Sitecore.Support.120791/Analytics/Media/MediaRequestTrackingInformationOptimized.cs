using System.Web;
using System.Xml.Linq;
using Sitecore.Analytics.Configuration;
using Sitecore.Analytics.Media;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Resources.Media;

namespace Sitecore.Support.Analytics.Media
{
    public class MediaRequestTrackingInformationOptimized : MediaRequestTrackingInformation
    {
        private readonly MediaRequest request;

        public MediaRequestTrackingInformationOptimized(MediaRequest request) : base(request)
        {
            this.request = request;
        }

        protected Field FindTrackingField(Item item)
        {
            Assert.ArgumentNotNull(item, "item");
            Field trackingField = this.GetTrackingField(item);
            if (trackingField != null)
            {
                return trackingField;
            }
            TemplateItem template = item.Template;
            if (template == null)
            {
                return null;
            }
            return this.GetTrackingField(template.InnerItem);
        }

        protected Field GetTrackingField(Item item)
        {
            Assert.ArgumentNotNull(item, "item");
            return item.Fields["__Tracking"];
        }

        public override bool IsTrackedRequest()
        {
            bool flag;
            Item mediaItem = this.GetMediaItem();
            if ((mediaItem != null) && this.MediaItemHasTracking(mediaItem, out flag))
            {
                return !flag;
            }
            HttpRequest innerRequest = this.request.InnerRequest;
            if (innerRequest == null)
            {
                return false;
            }
            string str = innerRequest.QueryString[AnalyticsSettings.CampaignQueryStringKey];
            string str2 = innerRequest.QueryString[AnalyticsSettings.EventQueryStringKey];
            if (string.IsNullOrEmpty(str))
            {
                return !string.IsNullOrEmpty(str2);
            }
            return true;
        }

        protected virtual bool MediaItemHasTracking(Item mediaItem, out bool isIgnored)
        {
            Field field = this.FindTrackingField(mediaItem);
            isIgnored = false;
            if (field == null)
            {
                return false;
            }
            string str = field.Value;
            if (string.IsNullOrWhiteSpace(str))
            {
                return false;
            }
            XElement element = XDocument.Parse(str).Element("tracking");
            if (element == null)
            {
                return false;
            }
            if (!element.HasElements)
            {
                if (!element.HasAttributes)
                {
                    return false;
                }
                XAttribute attribute = element.Attribute("ignore");
                if (attribute == null)
                {
                    return true;
                }
                isIgnored = attribute.Value == "1";
            }
            return true;
        }
    }
}
