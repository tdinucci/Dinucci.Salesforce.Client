# Dinucci.Salesforce.Client
This is a general (.Net Standard 2.0) client library for the assorted Salesforce API's.

It does not currently integrate with all API's but I will continue to add support for additional API's when the need arises.  

PR's which integrate with additional API's are very welcome - so long as they include integration tests which are easy to run on a generic Salesforce Org.

## Current API Support

#### Data API
* Describe 
* Query
* Create
* Update
* Delete 

#### Tooling API
* Execute anonymous Apex
* Query metadata

#### Custom API  (REST Only)

* Get
* Post
* Put
* Patch
* Delete

See - https://developer.salesforce.com/docs/atlas.en-us.apexcode.meta/apexcode/apex_rest.htm

## Samples
The _Sample.Dinucci.Salesforce.Client_ project contains basic examples which show how all supported API's can be called. 

## Authentication
At present only the password grant OAuth 2 flow is supported and the sample project demonstrates this.

The implemented IAuthenticator (PasswordFlowAuthenticator) will automatically re-authenticate with Salesforce if/when an access token expires.

## NuGet
The current NuGet package is:

https://www.nuget.org/packages/Dinucci.Salesforce.Client/1.0.2
