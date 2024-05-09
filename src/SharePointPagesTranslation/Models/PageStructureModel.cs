using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharePointPagesTranslation.Models
{
    public class PageStructureModel
    {
        public List<Section> Sections { get; set; } = new List<Section>();

        public class Section
        {
            public float SectionOrder { get; set; }
            public List<Column> Columns { get; set; } = new List<Column>();
        }

        public class Column
        {
            public int ColumnOrder { get; set; }
            public List<TextControl> Controls { get; set; } = new List<TextControl>();
        }

        public class TextControl
        {
            public int ControlOrder { get; set; }
            public string Text { get; set; }
        }
    }
}