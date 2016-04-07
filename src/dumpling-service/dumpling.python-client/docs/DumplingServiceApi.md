# swagger_client.DumplingServiceApi

All URIs are relative to *http://dotnetrp.azurewebsites.net*

Method | HTTP request | Description
------------- | ------------- | -------------
[**dumpling_service_get_dump_zip**](DumplingServiceApi.md#dumpling_service_get_dump_zip) | **GET** /dumpling/store/get/{owner}/{dumplingid} | todo: Retrieve the originally uploaded dump container, and return it.
[**dumpling_service_get_status**](DumplingServiceApi.md#dumpling_service_get_status) | **GET** /dumpling/status/{owner}/{dumplingid} | returns the current status of a dumpling.
[**dumpling_service_post_dump_chunk**](DumplingServiceApi.md#dumpling_service_post_dump_chunk) | **POST** /dumpling/store/chunk/{owner}/{targetos}/{index}/{filesize} | 
[**dumpling_service_say_hi**](DumplingServiceApi.md#dumpling_service_say_hi) | **GET** /dumpling/test/hi/im/{name} | This is just here to test service availability. \r\n            \r\n            \r\n            curl http://[dotnetrp].net/dumpling/test/hi/im/(yourname)


# **dumpling_service_get_dump_zip**
> list[AzureBlobInfo] dumpling_service_get_dump_zip(owner, dumplingid)

todo: Retrieve the originally uploaded dump container, and return it.

### Example 
```python
import time
import swagger_client
from swagger_client.rest import ApiException
from pprint import pprint

# create an instance of the API class
api_instance = swagger_client.DumplingServiceApi()
owner = 'owner_example' # str | user identifier
dumplingid = 'dumplingid_example' # str | the dumpling id that was returned from /dumpling/storage/(owner)/(targetos)

try: 
    # todo: Retrieve the originally uploaded dump container, and return it.
    api_response = api_instance.dumpling_service_get_dump_zip(owner, dumplingid)
    pprint(api_response)
except ApiException as e:
    print "Exception when calling DumplingServiceApi->dumpling_service_get_dump_zip: %s\n" % e
```

### Parameters

Name | Type | Description  | Notes
------------- | ------------- | ------------- | -------------
 **owner** | **str**| user identifier | 
 **dumplingid** | **str**| the dumpling id that was returned from /dumpling/storage/(owner)/(targetos) | 

### Return type

[**list[AzureBlobInfo]**](AzureBlobInfo.md)

### Authorization

No authorization required

### HTTP reuqest headers

 - **Content-Type**: Not defined
 - **Accept**: application/json, text/json, application/xml, text/xml

[[Back to top]](#) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to Model list]](../README.md#documentation-for-models) [[Back to README]](../README.md)

# **dumpling_service_get_status**
> str dumpling_service_get_status(owner, dumplingid)

returns the current status of a dumpling.

### Example 
```python
import time
import swagger_client
from swagger_client.rest import ApiException
from pprint import pprint

# create an instance of the API class
api_instance = swagger_client.DumplingServiceApi()
owner = 'owner_example' # str | 
dumplingid = 'dumplingid_example' # str | 

try: 
    # returns the current status of a dumpling.
    api_response = api_instance.dumpling_service_get_status(owner, dumplingid)
    pprint(api_response)
except ApiException as e:
    print "Exception when calling DumplingServiceApi->dumpling_service_get_status: %s\n" % e
```

### Parameters

Name | Type | Description  | Notes
------------- | ------------- | ------------- | -------------
 **owner** | **str**|  | 
 **dumplingid** | **str**|  | 

### Return type

**str**

### Authorization

No authorization required

### HTTP reuqest headers

 - **Content-Type**: Not defined
 - **Accept**: application/json, text/json, application/xml, text/xml

[[Back to top]](#) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to Model list]](../README.md#documentation-for-models) [[Back to README]](../README.md)

# **dumpling_service_post_dump_chunk**
> str dumpling_service_post_dump_chunk(owner, targetos, index, filesize)



### Example 
```python
import time
import swagger_client
from swagger_client.rest import ApiException
from pprint import pprint

# create an instance of the API class
api_instance = swagger_client.DumplingServiceApi()
owner = 'owner_example' # str | 
targetos = 'targetos_example' # str | 
index = 56 # int | 
filesize = 789 # int | 

try: 
    api_response = api_instance.dumpling_service_post_dump_chunk(owner, targetos, index, filesize)
    pprint(api_response)
except ApiException as e:
    print "Exception when calling DumplingServiceApi->dumpling_service_post_dump_chunk: %s\n" % e
```

### Parameters

Name | Type | Description  | Notes
------------- | ------------- | ------------- | -------------
 **owner** | **str**|  | 
 **targetos** | **str**|  | 
 **index** | **int**|  | 
 **filesize** | **int**|  | 

### Return type

**str**

### Authorization

No authorization required

### HTTP reuqest headers

 - **Content-Type**: Not defined
 - **Accept**: application/json, text/json, application/xml, text/xml

[[Back to top]](#) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to Model list]](../README.md#documentation-for-models) [[Back to README]](../README.md)

# **dumpling_service_say_hi**
> str dumpling_service_say_hi(name)

This is just here to test service availability. \r\n            \r\n            \r\n            curl http://[dotnetrp].net/dumpling/test/hi/im/(yourname)

### Example 
```python
import time
import swagger_client
from swagger_client.rest import ApiException
from pprint import pprint

# create an instance of the API class
api_instance = swagger_client.DumplingServiceApi()
name = 'name_example' # str | 

try: 
    # This is just here to test service availability. \r\n            \r\n            \r\n            curl http://[dotnetrp].net/dumpling/test/hi/im/(yourname)
    api_response = api_instance.dumpling_service_say_hi(name)
    pprint(api_response)
except ApiException as e:
    print "Exception when calling DumplingServiceApi->dumpling_service_say_hi: %s\n" % e
```

### Parameters

Name | Type | Description  | Notes
------------- | ------------- | ------------- | -------------
 **name** | **str**|  | 

### Return type

**str**

### Authorization

No authorization required

### HTTP reuqest headers

 - **Content-Type**: Not defined
 - **Accept**: application/json, text/json, application/xml, text/xml

[[Back to top]](#) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to Model list]](../README.md#documentation-for-models) [[Back to README]](../README.md)

