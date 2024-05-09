using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharePointPagesTranslation.Interfaces
{
    public interface ICamlQueries
    {
        public string ItemByGuid { get; }
        public string RecentlyCreatedTranslatedPages { get; }
        public string RecentlyCreatedPages { get; }
        public string TranslationPagesBySourcePageGuid { get; }
    }
}
