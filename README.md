# Virto Commerce Elastic App Search Module 

## Overview
VirtoCommerce.ElasticAppSearch module implements ISearchProvider defined in the VirtoCommerce Search module to provide integration with Elastic App Search API.

Elastic App search provides search, aggregation, and analytics capabilities as a Service, on top of ElasticSearch. It provides tools that cat help tune a search result sets without development:
* [Relevance Tuning](https://www.elastic.co/guide/en/app-search/current/precision-tuning.html/)
* [Synonyms](https://www.elastic.co/guide/en/app-search/current/synonyms-guide.html/)
* [Curations](https://www.elastic.co/guide/en/app-search/current/curations-guide.html/)

[Read more about how to deploy Elastic App Search here](https://www.elastic.co/guide/en/app-search/current/installation.html)

## Configuration
Elastic App Search provider are configurable by these configuration keys:

* **Search.Provider** is the name of the search provider and must be **ElasticAppSearch**
* **Search.ElasticAppSearch.Endpoint** is a network address and port of the ElasticAppSearch server.
* **Search.ElasticAppSearch.PrivateApiKey** is a API access key can read and write against all available API endpoints. Prefixed with private-.
* **Search.Scope** is a common name (prefix) of all indexes. Each document type is stored in a separate index. Full index name is `scope-{documenttype}`. One search service can serve multiple indexes. **Optional**.  Default value is **default**.

[Read more about configuration here](https://virtocommerce.com/docs/user-guide/configuration-settings/)

```json
    "Search": {
        "Provider": "ElasticAppSearch",
        "Scope": "default",
        "ElasticAppSearch": {
			"Endpoint": "https://localhost:3002",
        	"PrivateApiKey": "private-key"
        }
    }
```

API keys can be managed on the Credential menu on the App Search Dashboard panel.

## Documentation

* [Search Fundamentals](https://virtocommerce.com/docs/fundamentals/search/)
* [Elastic App Search Documentation](https://www.elastic.co/guide/en/app-search/8.1/index.html)

## References

* Deploy: https://virtocommerce.com/docs/latest/developer-guide/deploy-module-from-source-code/
* Installation: https://www.virtocommerce.com/docs/latest/user-guide/modules/
* Home: https://virtocommerce.com
* Community: https://www.virtocommerce.org
* [Download Latest Release](https://github.com/VirtoCommerce/vc-module-catalog/releases/latest)

## License

Copyright (c) Virto Solutions LTD.  All rights reserved.

Licensed under the Virto Commerce Open Software License (the "License"); you
may not use this file except in compliance with the License. You may
obtain a copy of the License at

<http://virtocommerce.com/opensourcelicense>

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or
implied.
