# ![crest](https://assets.publishing.service.gov.uk/government/assets/crests/org_crest_27px-916806dcf065e7273830577de490d5c7c42f36ddec83e907efe62086785f24fb.png) Digital Apprenticeships Service

##  RoATP Provider Moderation UI

<img src="https://avatars.githubusercontent.com/u/9841374?s=200&v=4" align="right" alt="UK Government logo">

[![Build Status](https://dev.azure.com/sfa-gov-uk/Digital%20Apprenticeship%20Service/_apis/build/status%2FApprenticeships%20Providers%2Fdas-roatp-providermoderation-web?repoName=SkillsFundingAgency%2Fdas-roatp-providermoderation-web&branchName=main)](https://dev.azure.com/sfa-gov-uk/Digital%20Apprenticeship%20Service/_build/latest?definitionId=2977&repoName=SkillsFundingAgency%2Fdas-roatp-providermoderation-web&branchName=main)
[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=SkillsFundingAgency_das-roatp-providermoderation-web&metric=alert_status)](https://sonarcloud.io/dashboard?id=SkillsFundingAgency_das-roatp-providermoderation-web)
[![License](https://img.shields.io/badge/license-MIT-lightgrey.svg?longCache=true&style=flat-square)](https://en.wikipedia.org/wiki/MIT_License)


## About

The front end for tribal users to view and administer the provider description.

It interacts with an outer api (https://github.com/SkillsFundingAgency/das-apim-endpoints/tree/master/src/RoatpProviderModeration), which in turn interacts with several other sources of data, but most significantly the inner api for roatp data (https://github.com/SkillsFundingAgency/das-roatp-api)


### Developer Setup

### Pre-Requisites

* A clone of this repository
* A storage emulator like Azurite
* Visual studio or similar IDE 

### Dependencies

* DfE Signin for user authentication
* The Outer API [das-apim-endpoints](https://github.com/SkillsFundingAgency/das-apim-endpoints/tree/master/src/RoatpProviderModeration) should be available either running locally or accessible in an Azure tenancy.

#### Setup

- Create a Configuration table in your (Development) local storage account.
- Obtain the local config json from the das-employer-config for das-roatp-providermoderation-web repo (https://github.com/SkillsFundingAgency/das-employer-config/blob/master/das-roatp-providermoderation-web/SFA.DAS.Roatp.ProviderModeration.Web.json) 
  - PartitionKey: LOCAL
  - RowKey: SFA.DAS.Roatp.ProviderModeration.Web_1.0
  - Data: {The contents of the local config json file}
  
- In the web project, if not exist already, add `AppSettings.Development.json` file with following content:
```json  
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "ConfigurationStorageConnectionString": "UseDevelopmentStorage=true;",
  "ConfigNames": "SFA.DAS.Roatp.ProviderModeration.Web,SFA.DAS.Provider.DfeSignIn",
  "EnvironmentName": "LOCAL",
  "cdn": {
    "url": "https://das-prd-frnt-end.azureedge.net"
  }
}
```
  
You will also need to setup the roatp outer api and have it running (see https://github.com/SkillsFundingAgency/das-apim-endpoints/ and go to the section for 'Provider Moderation')

Open the solution with Visual Studio, and run the project SFA.DAS.Roatp.ProviderModeration.Web, running under process 'SFA.DAS.Roatp.ProviderModeration.Web' (not IIS)

## Technologies
* .NetCore 10.0
* NUnit
* Moq
* FluentAssertions


  