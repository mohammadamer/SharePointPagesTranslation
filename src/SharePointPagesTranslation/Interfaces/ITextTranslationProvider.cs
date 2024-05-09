using SharePointPagesTranslation.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharePointPagesTranslation.Interfaces
{
    public interface ITextTranslationProvider
    {
        public Task<PageStructureModel> TranslateContent(PageStructureModel sourcePageContentStructure, string targetLanguage);
        public Task<string> DetectContent(PageStructureModel sourcePageContentStructure);
    }
}
