using System.Text.Json;
using SharePointPagesTranslation.Interfaces;
using SharePointPagesTranslation.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace SharePointPagesTranslation.Functions
{
    public class TranslatePages
    {
        private readonly ILogger<TranslatePages> _logger;
        private readonly ISharePointOnlineProvider _sharePointOnlineProvider;
        private readonly ITextTranslationProvider _textTranslationProvider;

        public TranslatePages(ISharePointOnlineProvider sharePointOnlineProvider, ILogger<TranslatePages> logger, ITextTranslationProvider documentTranslationProvider)
        {
            _sharePointOnlineProvider = sharePointOnlineProvider;
            _logger = logger;
            _textTranslationProvider = documentTranslationProvider;
        }

        [Function(nameof(TranslatePages))]
        public async Task Run([QueueTrigger("translate-pages", Connection = "AzureWebJobsStorage")] string TargetPagesMessage)
        {
            _logger.LogInformation($"TranslatePages: Queue trigger function processed: {TargetPagesMessage}");
            _logger.LogInformation($"TranslatePages Func: Translating target pages using AI...");

            PageDetailsModel pageDetailsModel = JsonSerializer.Deserialize<PageDetailsModel>(TargetPagesMessage);
            await _sharePointOnlineProvider.SetUpProvider(pageDetailsModel.SiteUrl);

            //Get the target page content structure and detect the language of it
            _logger.LogInformation($"Getting target page {pageDetailsModel.TranslatedItemId} content for language detection.");
            var targetPageContentStructure = await _sharePointOnlineProvider.GetPageContentStructure(new Guid(pageDetailsModel.TranslatedItemId));

            _logger.LogInformation($"Detect target page content. if it's the same language like the source, then skipe translation it.");
            var targetPageLanguage = await _textTranslationProvider.DetectContent(targetPageContentStructure);

            //Get the source page content structure and detect the language of the source page
            _logger.LogInformation($"Praparing source page {pageDetailsModel.SourceItemId} content for translation.");
            var sourcePageContentStructure = await _sharePointOnlineProvider.GetPageContentStructure(new Guid(pageDetailsModel.SourceItemId));

            //Translate the target page content if it's not in the same language as the source page
            if (targetPageLanguage == pageDetailsModel.TranslatedItemLanguage)
            {
                _logger.LogInformation($"TranslateTargetPages Func: Target page is already in the same language as the source page, No need to Translate it.");
                //ToDO - Add logic to update the translation status field
                //ToDO - Add logic to update the page to be published.
            }
            else
            {
                _logger.LogInformation($"getting source page translation for page {pageDetailsModel.SourceItemId}.");
                var languageToTranslate = pageDetailsModel.TranslatedItemLanguage;
                var translatedPageStructureModel = await _textTranslationProvider.TranslateContent(sourcePageContentStructure, languageToTranslate);

                _logger.LogInformation($"Updating translation of page content for page {pageDetailsModel.TranslatedPageId}.");
                await _sharePointOnlineProvider.UpdatePageContentStructure(translatedPageStructureModel, new Guid(pageDetailsModel.TranslatedItemId));
            }

            //Update the translation status field value of the page
            _logger.LogInformation($"Updating {Constants.AutomateTranslation} field value for page {pageDetailsModel.TranslatedPageId} after page translation.");
            var keyValues = new Dictionary<string, object> { { Constants.AutomateTranslation, false } };
            await _sharePointOnlineProvider.ChangeItemFieldValues(pageDetailsModel.LibraryId, pageDetailsModel.TranslatedPageId, keyValues);

        }
    }
}
