using SharePointPagesTranslation.Models;
using System.Security.Cryptography.X509Certificates;

namespace SharePointPagesTranslation
{
    public class CertificateLoader
    {
        public enum CertificateType
        {
            SharePointOnlineCertificate,
            AADReaderCertificate
        }

        public static X509Certificate2 LoadCertificate(AzureFunctionAppSettingsModel appSettings, CertificateType certificateType)
        {
            string certBase64Encoded = certificateType == CertificateType.SharePointOnlineCertificate ?
             appSettings.SharePointOnlineCertificate : appSettings.AADReaderCertificate;

            Console.WriteLine("Loading certificate.");

            if (!string.IsNullOrEmpty(certBase64Encoded))
            {
                Console.WriteLine($"Using Azure Function flow. '{certBase64Encoded}'");
                return new X509Certificate2(Convert.FromBase64String(certBase64Encoded),
                    "",
                    X509KeyStorageFlags.Exportable |
                    X509KeyStorageFlags.MachineKeySet |
                    X509KeyStorageFlags.EphemeralKeySet);
            }
            else
            {
                Console.WriteLine("Using local flow.");
                var store = new X509Store(StoreName.My, StoreLocation.CurrentUser);
                store.Open(OpenFlags.ReadOnly | OpenFlags.OpenExistingOnly);
                var thumbprint = certificateType == CertificateType.SharePointOnlineCertificate ?
                 appSettings.SharePointOnlineCertificateThumbPrint : appSettings.AADReaderCertificateThumbPrint;

                var certificateCollection = store.Certificates.Find(X509FindType.FindByThumbprint, thumbprint, false);
                store.Close();

                return certificateCollection.First();
            }
        }
    }
}