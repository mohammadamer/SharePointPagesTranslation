# SharePointPagesTranslation
SharePointPagesTranslation is an Azure Function app that streamlines the translation of SharePoint Pages by leveraging SharePoint Webhooks, Azure Functions, and Azure Translator Service for automation.


https://github.com/user-attachments/assets/1d6f8bb7-5572-460e-a6cf-c24062b36950


# Overview
In today's global landscape, the availability of multilingual content on SharePoint is increasingly important for effective communication. This solution offers a straightforward integration of SharePoint Webhooks with Azure Functions and Azure Translator Service to facilitate this.


# Prerequisites

## Azure Portal:
- Registering an Azure App
- Adding a Certificate for PnPCore Authentication to your App registration
- Setting up Azure AI Translator Service in the Azure Portal

## SharePoint Level:
- Setting up a SharePoint Webhook subscription
- Activating translation for multiple languages on your SharePoint site

# How it works!
With SharePoint Webhooks, SharePoint can activate an Azure Function when new pages are added to the sitepage library. Using PnPCore, it's possible to create translated pages based on the languages enabled in the SharePoint site's language settings. Another Azure Function then employs the Azure Translator Service to perform translations according to the site's language settings, allowing us to update the content of newly translated pages with the results from the Azure Translator Service.


