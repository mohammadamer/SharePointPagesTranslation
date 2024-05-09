using System.Security.Cryptography.X509Certificates;

namespace SharePointPagesTranslation.Models
{
    public class AzureFunctionAppSettingsModel
    {
        public string TenantID { get; set; }
        public string SharePointOnlineClientID { get; set; }
        public string SiteURL { get; set; }
        public string SharePointOnlineCertificateThumbPrint { get; set; }
        public string SharePointOnlineCertificate { get; set; }
        public string AADReaderCertificate { get; set; }
        public string AADReaderCertificateThumbPrint { get; set; }
    }
}