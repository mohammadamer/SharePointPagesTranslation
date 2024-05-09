using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharePointPagesTranslation.Models
{
    public class PageDetailsModel
    {
        public string SiteUrl { get; set; }
        public Guid LibraryId { get; set; }
        public string TranslatedItemId { get; set; }
        public int TranslatedPageId { get; set; }
        public string SourceItemId { get; set; }
        public string TranslatedItemLanguage { get; set; }
    }
}