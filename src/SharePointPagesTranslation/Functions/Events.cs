using System.Net;
using System.Text.Json;
using SharePointPagesTranslation.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace SharePointPagesTranslation.Functions
{
    public class MultiResponse
    {
        [QueueOutput("events")]
        public IList<string> EventMessages { get; set; }
        public HttpResponseData HttpResponse { get; set; }
    }
    public class Events
    {
        private readonly ILogger _logger;
        public Events(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<Events>();
        }

        [Function("Events")]
        public async Task<MultiResponse> Run([HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequestData req)
        {
            _logger.LogInformation("Events Func: Events HttpTrigger function was triggered");
            var response = req.CreateResponse(HttpStatusCode.OK);
            var eventsOutput = new List<string>();

            string validationToken = req.Query["validationtoken"];
            if (validationToken != null)
            {
                _logger.LogInformation($"Validation token {validationToken} received");
                response.WriteString(validationToken);
                return new MultiResponse
                {
                    HttpResponse = response
                };
            }

            var content = await new StreamReader(req.Body).ReadToEndAsync();
            _logger.LogInformation($"Received payload: {content}");
            var notifications = JsonSerializer.Deserialize<ResponseModel<WebhookNotificationModel>>(content).Value;

            if (notifications.Count > 0)
            {
                _logger.LogInformation($"Processing {notifications.Count} notifications");
                foreach (var notification in notifications)
                {
                    notification.SiteUrl = Environment.GetEnvironmentVariable(Constants.TenantURL) + notification.SiteUrl;
                    string message = JsonSerializer.Serialize(notification);
                    eventsOutput.Add(message);
                }
            }
            return new MultiResponse
            {
                EventMessages = eventsOutput,
                HttpResponse = response
            };
        }
    }
}
