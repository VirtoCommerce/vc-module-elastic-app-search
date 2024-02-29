# Virto Commerce Elastic App Search Module 

## Overview
VirtoCommerce.ElasticAppSearch module implements `ISearchProvider` defined in the VirtoCommerce Search module to provide integration with Elastic App Search API.

Elastic App search provides search, aggregation, and analytic capabilities as a service, on top of ElasticSearch. It also supplies tools that can help you tune search result sets without development:

* [Relevance Tuning](https://www.elastic.co/guide/en/app-search/current/precision-tuning.html)
* [Synonyms](https://www.elastic.co/guide/en/app-search/current/synonyms-guide.html)
* [Curations](https://www.elastic.co/guide/en/app-search/current/curations-guide.html)

For more information on deploying Elastic App Search, refer to the official documentation [here](https://www.elastic.co/guide/en/app-search/current/installation.html).

## Features
1. Fulltext Search Provider compatible with Elastic App Search version 8.12 and above.
1. Boosting Profile functionality.
1. Dynamic Boosting Concatenation: The provider combines dynamic boosting with query and static boosting from the Search Relevance Tuning panel.

## Known limitations

## Configuration
Elastic App Search provider can be configured through these configuration keys:

+ **Search.Provider**: Name of the search provider, must be **ElasticAppSearch**
+ **Search.Scope**: Common name (prefix) of all indexes. Each document type is stored in a separate index. Full index name is `scope-{documenttype}`. One search service can serve multiple indexes. This is an **optional** key, and its default value is set to **default**.
+ **Search.ElasticAppSearch.Endpoint**: Network address and port of the ElasticAppSearch server
+ **Search.ElasticAppSearch.PrivateApiKey**: API access key that can read and write against all available API endpoints. Prefixed with `private-`.
+ **Search.ElasticAppSearch.KibanaBaseUrl**: Kibana base URL for accessing the Kibana Dashboard from the application menu. 
+ **Search.ElasticAppSearch.KibanaPath**: Path to the App Search engine in the Kibana Dashboard. Default value is `/app/enterprise_search/app_search/engines/`.

[Read more about configuration [here](https://docs.virtocommerce.org/platform/developer-guide/Configuration-Reference/appsettingsjson/).

```json
	"Search": {
		"Provider": "ElasticAppSearch",
		"Scope": "default",
		"ElasticAppSearch": {
				"Endpoint": "https://localhost:3002",
			"PrivateApiKey": "private-key",
		  "KibanaBaseUrl": "https://localhost:5601"
		}
	}
```

Endpoint and API key can be managed in the Credential menu within the App Search Dashboard panel.

## App Menu 

## Dynamic Boosting
The Elastic App Search provider combines static boosting from the Search Relevance Tuning panel with dynamic boosting that can be passed at runtime.

Dynamic Boosting supports both Value Boost and Functional Boosting.

1. Define Boost Presets in App Settings:

```json
	"Search": {
		"Provider": "ElasticAppSearch",
		"Scope": "default",
		"ElasticAppSearch": {
		  "Endpoint": "https://localhost:3002",
		  "PrivateApiKey": "private-key",
		  "KibanaBaseUrl": "https://localhost:5601",

		  "BoostPresets": [
			{
			  "Name": "High",
			  "Type": "value",
			  "Operation": "add",
			  "Factor": 5,
			  "IsDefault": true
			},
			{
			  "Name": "Medium",
			  "Type": "value",
			  "Operation": "add",
			  "Factor": 3
			},
			{
			  "Name": "LOw",
			  "Type": "value",
			  "Operation": "add",
			  "Factor": 3
			}
		  ]
		}
	  }
```

2. Pass SearchBoost with Search Request

```cs
	searchRequest.Boosts = [new SearchBoost
			{
				FieldName = "brand",
				Value = "Apple",
				Preset = "Medium",
			}];
```


As a result, the Elastic App Search provider applies Dynamic Value Boosting for the Apple brand with a factor of 3 on top of static boosting that you configured.

## Diagnostic
The module offers a diagnostic API accessible from the Swagger panel for the following purposes:

1. Search API: This endpoint allows you to make direct Search API requests to the Elastic App Search service and view the response.
  ```
  GET /api/elastic-app-search/diagnotic/{engineName}/search?query={query}
  ```
1. Search Explain API: This endpoint enables direct Search Explain API requests to the Elastic App Search service, displaying the response.
  ```
  GET /api/elastic-app-search/diagnotic/{engineName}/search_explain?query={query}
  ```
1. Search Settings API: This endpoint facilitates direct Search Settings API requests to the Elastic App Search service, providing the response.
  ```
  GET /api/elastic-app-search/diagnotic/{engineName}/search_settings?query={query}
  ```


## Documentation

* [Search Fundamentals](https://docs.virtocommerce.org/platform/developer-guide/Fundamentals/Indexed-Search/overview/)
* [Elastic App Search Documentation](https://www.elastic.co/guide/en/app-search/current/getting-started.html)
* [Elastic App Search Boosting](https://www.elastic.co/guide/en/app-search/current/boosts.html)
* [Elastic App Search Guide](./docs/eas-setup-guide.md)

## References

* Documentation: https://docs.virtocommerce.org
* Home: https://virtocommerce.com
* Community: https://www.virtocommerce.org

## License

Copyright (c) Virto Solutions LTD.  All rights reserved.

Licensed under the Virto Commerce Open Software License (the "License"); you
may not use this file except in compliance with the License. You may
obtain a copy of the License at <http://virtocommerce.com/opensourcelicense>.

Unless required by the applicable laws and regulations or agreed to in written form, the software
distributed under the License is provided on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or
implied.
