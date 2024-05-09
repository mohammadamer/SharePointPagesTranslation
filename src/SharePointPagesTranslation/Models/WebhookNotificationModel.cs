using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SharePointPagesTranslation.Models
{
    public class WebhookNotificationModel
    {
        [JsonPropertyName("subscriptionId")]
        public string SubscriptionId { get; set; }

        [JsonPropertyName("clientState")]
        public string ClientState { get; set; }

        [JsonPropertyName("expirationDateTime")]
        public DateTime ExpirationDateTime { get; set; }

        [JsonPropertyName("resource")]
        public string Resource { get; set; }

        [JsonPropertyName("tenantId")]
        public string TenantId { get; set; }

        [JsonPropertyName("siteUrl")]
        public string SiteUrl { get; set; }

        [JsonPropertyName("webId")]
        public string WebId { get; set; }
    }
}
