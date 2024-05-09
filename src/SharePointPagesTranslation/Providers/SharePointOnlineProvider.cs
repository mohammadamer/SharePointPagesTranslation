using SharePointPagesTranslation.Interfaces;
using SharePointPagesTranslation.Models;
using Microsoft.Extensions.Logging;
using PnP.Core.Model.SharePoint;
using PnP.Core.QueryModel;
using PnP.Core.Services;
using static SharePointPagesTranslation.Models.PageStructureModel;

namespace SharePointPagesTranslation.Providers
{
    public class SharePointOnlineProvider : ISharePointOnlineProvider
    {
        private readonly IPnPContextFactory pnpContextFactory;
        private PnPContext ctx;
        private ILogger log;
        private readonly ICamlQueries camlQueries;

        public SharePointOnlineProvider(
            IPnPContextFactory pnpContextFactory,
            ILoggerFactory loggerFactory,
            ICamlQueries camlQueries
            )
        {
            this.pnpContextFactory = pnpContextFactory;
            log = loggerFactory.CreateLogger<SharePointOnlineProvider>();
            this.camlQueries = camlQueries;
        }

        public async Task SetUpProvider(string siteURL)
        {
            log.LogDebug(nameof(SetUpProvider));
            ctx = await pnpContextFactory.CreateAsync(new Uri(siteURL));
            log.LogInformation($"SharePointOnlineProvider: Set up provider with site '{siteURL}'");
        }

        public async Task ChangeItemFieldValues(Guid listGUID, int itemID, Dictionary<string, object> keyValues)
        {
            log.LogDebug(nameof(ChangeItemFieldValues));
            log.LogInformation($"SharePointOnlineProvider: Getting list by ID: {listGUID}");
            IList list = await ctx.Web.Lists.GetByIdAsync(listGUID);

            IListItem item = await list.Items.GetByIdAsync(itemID);

            foreach (var key in keyValues.Keys)
            {
                log.LogDebug($"SharePointOnlineProvider: Setting item {key} value to {keyValues[key]}");
                item[key] = keyValues[key];
            }
            log.LogInformation($"SharePointOnlineProvider: Executing system update on item {item.Id}");
            await item.SystemUpdateAsync();
        }

        public async Task<IEnumerable<IListItem>> GetListItemsByCamlQuery(string camlQuery, Guid listGuid)
        {
            log.LogDebug(nameof(GetListItemsByCamlQuery));
            log.LogInformation($"SharePointOnlineProvider: Getting list for {listGuid}");
            IList list = await ctx.Web.Lists.GetByIdAsync(listGuid);
            await list.LoadListDataAsStreamAsync(new RenderListDataOptions()
            {
                ViewXml = camlQuery,
                RenderOptions = RenderListDataOptionsFlags.ListData
            });
            log.LogInformation($"SharePointOnlineProvider: Got {list.Items.AsRequested().Count()} items");
            return list.Items.AsRequested();
        }

        /// <summary>
        /// Get the page structure 'Sections, Cloumns, Controls and Text controls' and build a page structure model
        /// </summary>
        /// <param name="itemUniqueId"></param>
        /// <returns></returns>
        public async Task<PageStructureModel> GetPageContentStructure(Guid itemUniqueId)
        {
            IFile file = await ctx.Web.GetFileByIdAsync(itemUniqueId);
            var pages = await ctx.Web.GetPagesAsync(file.Name);
            var page = pages.First();

            var pageStructureModel = new PageStructureModel();
            if (page != null)
            {
                foreach (var section in page.Sections)
                {
                    var sectionModel = new Section { SectionOrder = section.Order };
                    foreach (var column in section.Columns)
                    {
                        var columnModel = new Column { ColumnOrder = (int)column.Order };
                        foreach (IPageText control in column.Controls)
                        {
                            var controlModel = new TextControl
                            {
                                ControlOrder = control.Order,
                                Text = control.Text
                            };
                            columnModel.Controls.Add(controlModel);
                        }
                        sectionModel.Columns.Add(columnModel);
                    }
                    pageStructureModel.Sections.Add(sectionModel);
                }
            }
            return pageStructureModel;
        }

        public async Task UpdatePageContentStructure(PageStructureModel pageContentStructure, Guid itemUniqueId)
        {
            IFile file = await ctx.Web.GetFileByIdAsync(itemUniqueId, s => s.ServerRelativeUrl);
            var filePath = file.ServerRelativeUrl.Split("/SitePages/")[1];
            var pages = await ctx.Web.GetPagesAsync(filePath);
            var page = pages.First();
            if (page != null)
            {
                int sectionOrder = 0;
                foreach (var sectionModel in pageContentStructure.Sections)
                {
                    //page section[0] order is 0.375 which is strange
                    var section = page.Sections.FirstOrDefault(s => s.Order == sectionModel.SectionOrder);
                    if (section != null)
                    {
                        foreach (var columnModel in sectionModel.Columns)
                        {
                            //Get Page section columns by column order
                            var column = section.Columns.FirstOrDefault(c => c.Order == columnModel.ColumnOrder);
                            if (column != null)
                            {
                                foreach (var controlModel in columnModel.Controls)
                                {
                                    //We've replaced the cast with a pattern match, which is generally considered more elegant and safer.
                                    //We've also added a check to make sure that the control is actually an IPageText before we try to set its Text property.
                                    var control = column.Controls.FirstOrDefault(c => c.Order == controlModel.ControlOrder);
                                    if (control is IPageText pageTextControl)
                                    {
                                        pageTextControl.Text = pageContentStructure.Sections[sectionOrder].Columns[columnModel.ColumnOrder - 1].Controls[controlModel.ControlOrder - 1].Text;
                                    }
                                    
                                }
                            }
                        }
                    }
                    sectionOrder++;
                }
                if (page.PageTitle.StartsWith("Translate into"))
                {
                    page.PageTitle = page.PageTitle.Split(": ")[1];
                }

                await page.SaveAsync();
                await page.PublishAsync();
            }
        }

        public async Task<List<string>> GetTranslationLanguages(Guid itemUniqueId)
        {
            List<string> languages = new List<string>();
            IFile file = await ctx.Web.GetFileByIdAsync(itemUniqueId);
            var pages = await ctx.Web.GetPagesAsync(file.Name);
            var page = pages.First();

            //get you a IPageTranslationStatusCollection object that contains a list of languages
            //for which the page was not yet translated and a collection of IPageTranslationStatus objects
            IPageTranslationStatusCollection translationStatusCollection = await page.GetPageTranslationsAsync();

            var translationLanguages = translationStatusCollection.TranslatedLanguages;
            var unTranslationLanguages = translationStatusCollection.UntranslatedLanguages;

            if (translationLanguages.Count > 0)
            {
                foreach (var translationLanguage in translationLanguages)
                {
                    languages.Add(translationLanguage.Culture.Split('-')[0]);
                }
            }

            if (unTranslationLanguages.Count > 0)
            {
                foreach (var translationLanguage in unTranslationLanguages)
                {
                    languages.Add(translationLanguage);
                }
            }
            return languages;
        }

        public async Task CreateTranslatedPages(Guid itemUniqueId)
        {
            IFile file = await ctx.Web.GetFileByIdAsync(itemUniqueId);
            var pages = await ctx.Web.GetPagesAsync(file.Name);
            var page = pages.First();

            //get you a IPageTranslationStatusCollection object that contains a list of languages
            //for which the page was not yet translated and a collection of IPageTranslationStatus objects
            var pageTranslations = await page.GetPageTranslationsAsync();

            //When you call this method without input it will automatically create page translations
            //for each site language for which there was not yet a translated page.
            IPageTranslationStatusCollection translatedPgaes = await page.TranslatePagesAsync();
        }
    }
}
