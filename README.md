# Virto Commerce Elastic App Search Module 

## Overview
VirtoCommerce.ElasticAppSearch module implements `ISearchProvider` defined in the VirtoCommerce Search module to provide integration with Elastic App Search API.

Elastic App search provides search, aggregation, and analytic capabilities as a service, on top of ElasticSearch. It also supplies tools that can help you tune search result sets without development:

* [Relevance Tuning](https://www.elastic.co/guide/en/app-search/current/precision-tuning.html)
* [Synonyms](https://www.elastic.co/guide/en/app-search/current/synonyms-guide.html)
* [Curations](https://www.elastic.co/guide/en/app-search/current/curations-guide.html)

Read more about how to deploy Elastic App Search [here](https://www.elastic.co/guide/en/app-search/current/installation.html).

## Configuration
Elastic App Search provider can be configured through these configuration keys:

+ **Search.Provider**: Name of the search provider, must be **ElasticAppSearch**
+ **Search.Scope**: Common name (prefix) of all indexes. Each document type is stored in a separate index. Full index name is `scope-{documenttype}`. One search service can serve multiple indexes. This is an **optional** key, and its default value is set to **default**.
+ **Search.ElasticAppSearch.Endpoint**: Network address and port of the ElasticAppSearch server
+ **Search.ElasticAppSearch.PrivateApiKey**: API access key that can read and write against all available API endpoints. Prefixed with `private-`.
+ **Search.ElasticAppSearch.KibanaBaseUrl**: Kibana base URL for accessing the Kibana Dashboard from the application menu. 
+ **Search.ElasticAppSearch.KibanaPath**: Path to the App Search engine in the Kibana Dashboard. Default value: /app/enterprise_search/app_search/engines/.

[Read more about configuration [here](https://virtocommerce.com/docs/user-guide/configuration-settings/).

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

API keys can be managed in the Credential menu within the App Search Dashboard panel.

## Documentation

* [Search Fundamentals](https://virtocommerce.com/docs/fundamentals/search/)
* [Elastic App Search Documentation](https://www.elastic.co/guide/en/app-search/current/getting-started.html)
* [Elastic App Search Guide](./docs/eas-setup-guide.md)

## References

* Deploy: https://virtocommerce.com/docs/latest/developer-guide/deploy-module-from-source-code/
* Installation: https://www.virtocommerce.com/docs/latest/user-guide/modules/
* Home: https://virtocommerce.com
* Community: https://www.virtocommerce.org
* Download Latest Release: https://github.com/VirtoCommerce/vc-module-catalog/releases/latest

## License

Copyright (c) Virto Solutions LTD.  All rights reserved.

Licensed under the Virto Commerce Open Software License (the "License"); you
may not use this file except in compliance with the License. You may
obtain a copy of the License at <http://virtocommerce.com/opensourcelicense>.

Unless required by the applicable laws and regulations or agreed to in written form, the software
distributed under the License is provided on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or
implied.
