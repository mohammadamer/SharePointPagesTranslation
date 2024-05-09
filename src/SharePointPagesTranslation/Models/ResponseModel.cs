using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;

namespace SharePointPagesTranslation.Models
{
    public class ResponseModel<T>
    {
        [JsonPropertyName("value")]
        public List<T> Value { get; set; }
    }
}
