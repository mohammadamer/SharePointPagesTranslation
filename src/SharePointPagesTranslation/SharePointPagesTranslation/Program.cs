using SharePointPagesTranslation.Models;
using SharePointPagesTranslation.Providers;
using SharePointPagesTranslation;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PnP.Core.Auth.Services.Builder.Configuration;
using SharePointPagesTranslation.Interfaces;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.Configuration;

AzureFunctionAppSettingsModel azureFunctionAppSettings = null;
var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices((context, services) =>
    {
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();

        services.AddSingleton(options =>
        {
            var configuration = context.Configuration;
            azureFunctionAppSettings = new AzureFunctionAppSettingsModel();
            configuration.Bind(azureFunctionAppSettings);
            return configuration;
        });

        services.AddSingleton(options => { return azureFunctionAppSettings; });
        services.AddPnPCore();
        services.AddPnPCoreAuthentication(options =>
        {
            X509Certificate2 cert = CertificateLoader.LoadCertificate(azureFunctionAppSettings, CertificateLoader.CertificateType.SharePointOnlineCertificate);
            options.Credentials.Configurations.Add("CertAuth", new PnPCoreAuthenticationCredentialConfigurationOptions
            {
                ClientId = azureFunctionAppSettings.SharePointOnlineClientID,
                TenantId = azureFunctionAppSettings.TenantID,
                X509Certificate = new PnPCoreAuthenticationX509CertificateOptions
                {
                    Certificate = cert,
                }
            });
            options.Credentials.DefaultConfiguration = "CertAuth";
        });

        services.AddHttpClient();
        services.AddSingleton<ICamlQueries, CamlQueries>();
        services.AddScoped<ISharePointOnlineProvider, SharePointOnlineProvider>();
        services.AddScoped<ITextTranslationProvider, TextTranslationProvider>();
    })
    .Build();

host.Run();

