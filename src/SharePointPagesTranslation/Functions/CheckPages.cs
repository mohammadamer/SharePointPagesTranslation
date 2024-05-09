using System.Text.Json;
using SharePointPagesTranslation.Interfaces;
using SharePointPagesTranslation.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.VisualBasic;
using PnP.Core.Model.SharePoint;

namespace SharePointPagesTranslation.Functions
{
    public class CheckPagesOutput
    {
        [QueueOutput("translate-pages")]
        public IList<string> TranslatedPages { get; set; }
    }

    public class CheckPages
    {
        private readonly ILogger<CheckPages> _logger;
        private readonly ISharePointOnlineProvider _sharePointOnlineProvider;
        private readonly ICamlQueries _camlQueries;

        private IEnumerable<IListItem> recentlyCreatedPages;
        private IEnumerable<IListItem> recentlyCreatedTranslatedItems;

        public CheckPages(ILogger<CheckPages> logger, ISharePointOnlineProvider sharePointOnlineProvider, ICamlQueries camlQueries)
        {
            _logger = logger;
            _sharePointOnlineProvider = sharePointOnlineProvider;
            _camlQueries = camlQueries;
        }

        [Function(nameof(CheckPages))]
        public async Task<CheckPagesOutput> Run([QueueTrigger("events", Connection = "AzureWebJobsStorage")] string message)
        {
            _logger.LogInformation($"CheckPages Func: Queue trigger function processed: {message}");
            var translatedPages = new List<string>();

            WebhookNotificationModel notification = JsonSerializer.Deserialize<WebhookNotificationModel>(message);
            await _sharePointOnlineProvider.SetUpProvider(notification.SiteUrl);

            var pageDetailsModel = new PageDetailsModel();
            if (await IsRecentlyCreatedPages(notification))
            {
                foreach (IListItem itm in recentlyCreatedPages)
                {
                    _logger.LogInformation($"CheckPages Func: The item with id {itm.Id} was just recently approved.");
                    _logger.LogInformation($"CheckPages Func: Create translated pages for item {itm.Id}.");
                    await _sharePointOnlineProvider.CreateTranslatedPages(itm.UniqueId);

                    var recentlyCreatedTranslatedItems = await GetTranslatedPagesBySourcePage(notification, itm.UniqueId);
                    foreach (IListItem item in recentlyCreatedTranslatedItems)
                    {
                        _logger.LogInformation($"CheckPages Func: The item with id: {item.Id} was just created as a translation of source item Id: {item.Values["_SPTranslationSourceItemId"]}.");
                        var sourceItemId = item.Values["_SPTranslationSourceItemId"].ToString();
                        var translatedItemLanguage = item.Values["_SPTranslationLanguage"].ToString().Split('-')[0];
                        var libraryId = GetListGUIDFromItem(item);

                        pageDetailsModel = new PageDetailsModel
                        {
                            SiteUrl = notification.SiteUrl,
                            LibraryId = libraryId,
                            TranslatedPageId = item.Id,
                            TranslatedItemId = item.UniqueId.ToString(),

                            SourceItemId = sourceItemId,
                            TranslatedItemLanguage = translatedItemLanguage
                        };

                        var pageDetailsMessage = JsonSerializer.Serialize(pageDetailsModel);
                        translatedPages.Add(pageDetailsMessage);
                    }

                    //await UpdateSourcePageTranslationStatus(pageDetailsModel);

                }
            }

            return new CheckPagesOutput
            {
                TranslatedPages = translatedPages
            };
        }

        private async Task<bool> IsRecentlyCreatedPages(WebhookNotificationModel notification)
        {
            bool IsRecentlyCreatedPage = false;
            string query = string.Format(_camlQueries.RecentlyCreatedPages, DateTime.UtcNow.AddHours(-1).ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'ff'Z'"));
            recentlyCreatedPages = await _sharePointOnlineProvider.GetListItemsByCamlQuery(query, new Guid(notification.Resource));
            if (recentlyCreatedPages.Any())
            {
                IsRecentlyCreatedPage = true;
            }
            return IsRecentlyCreatedPage;
        }

        private async Task<bool> IsTranslatedPages(WebhookNotificationModel notification)
        {
            bool IsRecentlyCreatedItemTranslationPage = false;
            string query = string.Format(_camlQueries.RecentlyCreatedTranslatedPages, DateTime.UtcNow.AddHours(-1).ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'ff'Z'"));
            recentlyCreatedTranslatedItems = await _sharePointOnlineProvider.GetListItemsByCamlQuery(query, new Guid(notification.Resource));
            if (recentlyCreatedTranslatedItems.Any())
            {
                IsRecentlyCreatedItemTranslationPage = true;
            }
            return IsRecentlyCreatedItemTranslationPage;
        }

        private async Task<IEnumerable<IListItem>> GetTranslatedPagesBySourcePage(WebhookNotificationModel notification, Guid guid)
        {
            string query = string.Format(_camlQueries.TranslationPagesBySourcePageGuid, guid);
            IEnumerable<IListItem> translationPagesItems = await _sharePointOnlineProvider.GetListItemsByCamlQuery(query, new Guid(notification.Resource));

            return translationPagesItems;
        }

        private async Task UpdateSourcePageTranslationStatus(PageDetailsModel pageDetailsModel)
        {
            //Update the translation status field value of the source page in order to avoid triggering the translation process again.
            _logger.LogInformation($"CheckPages Func: Updating {Constants.AutomateTranslation} field value for page {pageDetailsModel.SourceItemId} after page translation.");
            var keyValues = new Dictionary<string, object> { { Constants.AutomateTranslation, false } };

            string query = string.Format(_camlQueries.ItemByGuid, pageDetailsModel.SourceItemId);
            var items = await _sharePointOnlineProvider.GetListItemsByCamlQuery(query, pageDetailsModel.LibraryId);
            var itemId = items.FirstOrDefault().Id;

            await _sharePointOnlineProvider.ChangeItemFieldValues(pageDetailsModel.LibraryId, itemId, keyValues);
        }

        private Guid GetListGUIDFromItem(IListItem item) => (item.Parent.Parent as IList).Id;
    }

}
