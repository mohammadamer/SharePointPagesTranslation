using PnP.Core.Model.SharePoint;
using SharePointPagesTranslation.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharePointPagesTranslation.Interfaces
{
    public interface ISharePointOnlineProvider
    {
        public Task<IEnumerable<IListItem>> GetListItemsByCamlQuery(string camlQuery, Guid listGuid);
        public Task ChangeItemFieldValues(Guid listGUID, int itemID, Dictionary<string, object> keyValues);
        public Task SetUpProvider(string siteURL);
        public Task CreateTranslatedPages(Guid itemUniqueId);
        public Task<PageStructureModel> GetPageContentStructure(Guid itemUniqueId);
        public Task UpdatePageContentStructure(PageStructureModel pageContentStructure, Guid itemUniqueId);
        public Task<List<string>> GetTranslationLanguages(Guid itemUniqueId);
    }
}
