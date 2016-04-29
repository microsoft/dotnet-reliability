// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.IO;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using RestSharp;
using IO.Swagger.Client;

namespace IO.Swagger.Api
{
    /// <summary>
    /// Represents a collection of functions to interact with the API endpoints
    /// </summary>
    public interface IDumplingServiceApi
    {
        #region Synchronous Operations
        /// <summary>
        /// 
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="IO.Swagger.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="owner"></param>
        /// <param name="dumplingid"></param>
        /// <returns>string</returns>
        string DumplingServiceGetDumpUrl(string owner, string dumplingid);

        /// <summary>
        /// 
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="IO.Swagger.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="owner"></param>
        /// <param name="dumplingid"></param>
        /// <returns>ApiResponse of string</returns>
        ApiResponse<string> DumplingServiceGetDumpUrlWithHttpInfo(string owner, string dumplingid);
        /// <summary>
        /// returns the current status of a dumpling.
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="IO.Swagger.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="owner"></param>
        /// <param name="dumplingid"></param>
        /// <returns>string</returns>
        string DumplingServiceGetStatus(string owner, string dumplingid);

        /// <summary>
        /// returns the current status of a dumpling.
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="IO.Swagger.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="owner"></param>
        /// <param name="dumplingid"></param>
        /// <returns>ApiResponse of string</returns>
        ApiResponse<string> DumplingServiceGetStatusWithHttpInfo(string owner, string dumplingid);
        /// <summary>
        /// 
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="IO.Swagger.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="owner"></param>
        /// <param name="targetos"></param>
        /// <param name="index"></param>
        /// <param name="filesize"></param>
        /// <returns>string</returns>
        string DumplingServicePostDumpChunk(string owner, string targetos, int? index, long? filesize);

        /// <summary>
        /// 
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="IO.Swagger.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="owner"></param>
        /// <param name="targetos"></param>
        /// <param name="index"></param>
        /// <param name="filesize"></param>
        /// <returns>ApiResponse of string</returns>
        ApiResponse<string> DumplingServicePostDumpChunkWithHttpInfo(string owner, string targetos, int? index, long? filesize);
        /// <summary>
        /// This is just here to test service availability. \r\n            \r\n            \r\n            curl http://[dotnetrp].net/dumpling/test/hi/im/(yourname)
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="IO.Swagger.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="name"></param>
        /// <returns>string</returns>
        string DumplingServiceSayHi(string name);

        /// <summary>
        /// This is just here to test service availability. \r\n            \r\n            \r\n            curl http://[dotnetrp].net/dumpling/test/hi/im/(yourname)
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="IO.Swagger.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="name"></param>
        /// <returns>ApiResponse of string</returns>
        ApiResponse<string> DumplingServiceSayHiWithHttpInfo(string name);
        #endregion Synchronous Operations
        #region Asynchronous Operations
        /// <summary>
        /// 
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="IO.Swagger.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="owner"></param>
        /// <param name="dumplingid"></param>
        /// <returns>Task of string</returns>
        System.Threading.Tasks.Task<string> DumplingServiceGetDumpUrlAsync(string owner, string dumplingid);

        /// <summary>
        /// 
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="IO.Swagger.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="owner"></param>
        /// <param name="dumplingid"></param>
        /// <returns>Task of ApiResponse (string)</returns>
        System.Threading.Tasks.Task<ApiResponse<string>> DumplingServiceGetDumpUrlAsyncWithHttpInfo(string owner, string dumplingid);
        /// <summary>
        /// returns the current status of a dumpling.
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="IO.Swagger.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="owner"></param>
        /// <param name="dumplingid"></param>
        /// <returns>Task of string</returns>
        System.Threading.Tasks.Task<string> DumplingServiceGetStatusAsync(string owner, string dumplingid);

        /// <summary>
        /// returns the current status of a dumpling.
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="IO.Swagger.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="owner"></param>
        /// <param name="dumplingid"></param>
        /// <returns>Task of ApiResponse (string)</returns>
        System.Threading.Tasks.Task<ApiResponse<string>> DumplingServiceGetStatusAsyncWithHttpInfo(string owner, string dumplingid);
        /// <summary>
        /// 
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="IO.Swagger.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="owner"></param>
        /// <param name="targetos"></param>
        /// <param name="index"></param>
        /// <param name="filesize"></param>
        /// <returns>Task of string</returns>
        System.Threading.Tasks.Task<string> DumplingServicePostDumpChunkAsync(string owner, string targetos, int? index, long? filesize);

        /// <summary>
        /// 
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="IO.Swagger.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="owner"></param>
        /// <param name="targetos"></param>
        /// <param name="index"></param>
        /// <param name="filesize"></param>
        /// <returns>Task of ApiResponse (string)</returns>
        System.Threading.Tasks.Task<ApiResponse<string>> DumplingServicePostDumpChunkAsyncWithHttpInfo(string owner, string targetos, int? index, long? filesize);
        /// <summary>
        /// This is just here to test service availability. \r\n            \r\n            \r\n            curl http://[dotnetrp].net/dumpling/test/hi/im/(yourname)
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="IO.Swagger.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="name"></param>
        /// <returns>Task of string</returns>
        System.Threading.Tasks.Task<string> DumplingServiceSayHiAsync(string name);

        /// <summary>
        /// This is just here to test service availability. \r\n            \r\n            \r\n            curl http://[dotnetrp].net/dumpling/test/hi/im/(yourname)
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="IO.Swagger.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="name"></param>
        /// <returns>Task of ApiResponse (string)</returns>
        System.Threading.Tasks.Task<ApiResponse<string>> DumplingServiceSayHiAsyncWithHttpInfo(string name);
        #endregion Asynchronous Operations
    }

    /// <summary>
    /// Represents a collection of functions to interact with the API endpoints
    /// </summary>
    public class DumplingServiceApi : IDumplingServiceApi
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DumplingServiceApi"/> class.
        /// </summary>
        /// <returns></returns>
        public DumplingServiceApi(String basePath)
        {
            this.Configuration = new Configuration(new ApiClient(basePath));

            // ensure API client has configuration ready
            if (Configuration.ApiClient.Configuration == null)
            {
                this.Configuration.ApiClient.Configuration = this.Configuration;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DumplingServiceApi"/> class
        /// using Configuration object
        /// </summary>
        /// <param name="configuration">An instance of Configuration</param>
        /// <returns></returns>
        public DumplingServiceApi(Configuration configuration = null)
        {
            if (configuration == null) // use the default one in Configuration
                this.Configuration = Configuration.Default;
            else
                this.Configuration = configuration;

            // ensure API client has configuration ready
            if (Configuration.ApiClient.Configuration == null)
            {
                this.Configuration.ApiClient.Configuration = this.Configuration;
            }
        }

        /// <summary>
        /// Gets the base path of the API client.
        /// </summary>
        /// <value>The base path</value>
        public String GetBasePath()
        {
            return this.Configuration.ApiClient.RestClient.BaseUrl.ToString();
        }

        /// <summary>
        /// Sets the base path of the API client.
        /// </summary>
        /// <value>The base path</value>
        [Obsolete("SetBasePath is deprecated, please do 'Configuraiton.ApiClient = new ApiClient(\"http://new-path\")' instead.")]
        public void SetBasePath(String basePath)
        {
            // do nothing
        }

        /// <summary>
        /// Gets or sets the configuration object
        /// </summary>
        /// <value>An instance of the Configuration</value>
        public Configuration Configuration { get; set; }

        /// <summary>
        /// Gets the default header.
        /// </summary>
        /// <returns>Dictionary of HTTP header</returns>
        [Obsolete("DefaultHeader is deprecated, please use Configuration.DefaultHeader instead.")]
        public Dictionary<String, String> DefaultHeader()
        {
            return this.Configuration.DefaultHeader;
        }

        /// <summary>
        /// Add default header.
        /// </summary>
        /// <param name="key">Header field name.</param>
        /// <param name="value">Header field value.</param>
        /// <returns></returns>
        [Obsolete("AddDefaultHeader is deprecated, please use Configuration.AddDefaultHeader instead.")]
        public void AddDefaultHeader(string key, string value)
        {
            this.Configuration.AddDefaultHeader(key, value);
        }

        /// <summary>
        ///  
        /// </summary>
        /// <exception cref="IO.Swagger.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="owner"></param>
        /// <param name="dumplingid"></param>
        /// <returns>string</returns>
        public string DumplingServiceGetDumpUrl(string owner, string dumplingid)
        {
            ApiResponse<string> localVarResponse = DumplingServiceGetDumpUrlWithHttpInfo(owner, dumplingid);
            return localVarResponse.Data;
        }

        /// <summary>
        ///  
        /// </summary>
        /// <exception cref="IO.Swagger.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="owner"></param>
        /// <param name="dumplingid"></param>
        /// <returns>ApiResponse of string</returns>
        public ApiResponse<string> DumplingServiceGetDumpUrlWithHttpInfo(string owner, string dumplingid)
        {
            // verify the required parameter 'owner' is set
            if (owner == null)
                throw new ApiException(400, "Missing required parameter 'owner' when calling DumplingServiceApi->DumplingServiceGetDumpUrl");
            // verify the required parameter 'dumplingid' is set
            if (dumplingid == null)
                throw new ApiException(400, "Missing required parameter 'dumplingid' when calling DumplingServiceApi->DumplingServiceGetDumpUrl");

            var localVarPath = "/dumpling/store/geturl/{owner}/{dumplingid}";
            var localVarPathParams = new Dictionary<String, String>();
            var localVarQueryParams = new Dictionary<String, String>();
            var localVarHeaderParams = new Dictionary<String, String>(Configuration.DefaultHeader);
            var localVarFormParams = new Dictionary<String, String>();
            var localVarFileParams = new Dictionary<String, FileParameter>();
            Object localVarPostBody = null;

            // to determine the Content-Type header
            String[] localVarHttpContentTypes = new String[] {
            };
            String localVarHttpContentType = Configuration.ApiClient.SelectHeaderContentType(localVarHttpContentTypes);

            // to determine the Accept header
            String[] localVarHttpHeaderAccepts = new String[] {
                "application/json",
                "text/json",
                "application/xml",
                "text/xml"
            };
            String localVarHttpHeaderAccept = Configuration.ApiClient.SelectHeaderAccept(localVarHttpHeaderAccepts);
            if (localVarHttpHeaderAccept != null)
                localVarHeaderParams.Add("Accept", localVarHttpHeaderAccept);

            // set "format" to json by default
            // e.g. /pet/{petId}.{format} becomes /pet/{petId}.json
            localVarPathParams.Add("format", "json");
            if (owner != null) localVarPathParams.Add("owner", Configuration.ApiClient.ParameterToString(owner)); // path parameter
            if (dumplingid != null) localVarPathParams.Add("dumplingid", Configuration.ApiClient.ParameterToString(dumplingid)); // path parameter


            // make the HTTP request
            IRestResponse localVarResponse = (IRestResponse)Configuration.ApiClient.CallApi(localVarPath,
                Method.GET, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams, localVarFileParams,
                localVarPathParams, localVarHttpContentType);

            int localVarStatusCode = (int)localVarResponse.StatusCode;

            if (localVarStatusCode >= 400)
                throw new ApiException(localVarStatusCode, "Error calling DumplingServiceGetDumpUrl: " + localVarResponse.Content, localVarResponse.Content);
            else if (localVarStatusCode == 0)
                throw new ApiException(localVarStatusCode, "Error calling DumplingServiceGetDumpUrl: " + localVarResponse.ErrorMessage, localVarResponse.ErrorMessage);

            return new ApiResponse<string>(localVarStatusCode,
                localVarResponse.Headers.ToDictionary(x => x.Name, x => x.Value.ToString()),
                (string)Configuration.ApiClient.Deserialize(localVarResponse, typeof(string)));
        }

        /// <summary>
        ///  
        /// </summary>
        /// <exception cref="IO.Swagger.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="owner"></param>
        /// <param name="dumplingid"></param>
        /// <returns>Task of string</returns>
        public async System.Threading.Tasks.Task<string> DumplingServiceGetDumpUrlAsync(string owner, string dumplingid)
        {
            ApiResponse<string> localVarResponse = await DumplingServiceGetDumpUrlAsyncWithHttpInfo(owner, dumplingid);
            return localVarResponse.Data;
        }

        /// <summary>
        ///  
        /// </summary>
        /// <exception cref="IO.Swagger.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="owner"></param>
        /// <param name="dumplingid"></param>
        /// <returns>Task of ApiResponse (string)</returns>
        public async System.Threading.Tasks.Task<ApiResponse<string>> DumplingServiceGetDumpUrlAsyncWithHttpInfo(string owner, string dumplingid)
        {
            // verify the required parameter 'owner' is set
            if (owner == null)
                throw new ApiException(400, "Missing required parameter 'owner' when calling DumplingServiceApi->DumplingServiceGetDumpUrl");
            // verify the required parameter 'dumplingid' is set
            if (dumplingid == null)
                throw new ApiException(400, "Missing required parameter 'dumplingid' when calling DumplingServiceApi->DumplingServiceGetDumpUrl");

            var localVarPath = "/dumpling/store/geturl/{owner}/{dumplingid}";
            var localVarPathParams = new Dictionary<String, String>();
            var localVarQueryParams = new Dictionary<String, String>();
            var localVarHeaderParams = new Dictionary<String, String>(Configuration.DefaultHeader);
            var localVarFormParams = new Dictionary<String, String>();
            var localVarFileParams = new Dictionary<String, FileParameter>();
            Object localVarPostBody = null;

            // to determine the Content-Type header
            String[] localVarHttpContentTypes = new String[] {
            };
            String localVarHttpContentType = Configuration.ApiClient.SelectHeaderContentType(localVarHttpContentTypes);

            // to determine the Accept header
            String[] localVarHttpHeaderAccepts = new String[] {
                "application/json",
                "text/json",
                "application/xml",
                "text/xml"
            };
            String localVarHttpHeaderAccept = Configuration.ApiClient.SelectHeaderAccept(localVarHttpHeaderAccepts);
            if (localVarHttpHeaderAccept != null)
                localVarHeaderParams.Add("Accept", localVarHttpHeaderAccept);

            // set "format" to json by default
            // e.g. /pet/{petId}.{format} becomes /pet/{petId}.json
            localVarPathParams.Add("format", "json");
            if (owner != null) localVarPathParams.Add("owner", Configuration.ApiClient.ParameterToString(owner)); // path parameter
            if (dumplingid != null) localVarPathParams.Add("dumplingid", Configuration.ApiClient.ParameterToString(dumplingid)); // path parameter


            // make the HTTP request
            IRestResponse localVarResponse = (IRestResponse)await Configuration.ApiClient.CallApiAsync(localVarPath,
                Method.GET, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams, localVarFileParams,
                localVarPathParams, localVarHttpContentType);

            int localVarStatusCode = (int)localVarResponse.StatusCode;

            if (localVarStatusCode >= 400)
                throw new ApiException(localVarStatusCode, "Error calling DumplingServiceGetDumpUrl: " + localVarResponse.Content, localVarResponse.Content);
            else if (localVarStatusCode == 0)
                throw new ApiException(localVarStatusCode, "Error calling DumplingServiceGetDumpUrl: " + localVarResponse.ErrorMessage, localVarResponse.ErrorMessage);

            return new ApiResponse<string>(localVarStatusCode,
                localVarResponse.Headers.ToDictionary(x => x.Name, x => x.Value.ToString()),
                (string)Configuration.ApiClient.Deserialize(localVarResponse, typeof(string)));
        }

        /// <summary>
        /// returns the current status of a dumpling. 
        /// </summary>
        /// <exception cref="IO.Swagger.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="owner"></param>
        /// <param name="dumplingid"></param>
        /// <returns>string</returns>
        public string DumplingServiceGetStatus(string owner, string dumplingid)
        {
            ApiResponse<string> localVarResponse = DumplingServiceGetStatusWithHttpInfo(owner, dumplingid);
            return localVarResponse.Data;
        }

        /// <summary>
        /// returns the current status of a dumpling. 
        /// </summary>
        /// <exception cref="IO.Swagger.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="owner"></param>
        /// <param name="dumplingid"></param>
        /// <returns>ApiResponse of string</returns>
        public ApiResponse<string> DumplingServiceGetStatusWithHttpInfo(string owner, string dumplingid)
        {
            // verify the required parameter 'owner' is set
            if (owner == null)
                throw new ApiException(400, "Missing required parameter 'owner' when calling DumplingServiceApi->DumplingServiceGetStatus");
            // verify the required parameter 'dumplingid' is set
            if (dumplingid == null)
                throw new ApiException(400, "Missing required parameter 'dumplingid' when calling DumplingServiceApi->DumplingServiceGetStatus");

            var localVarPath = "/dumpling/status/{owner}/{dumplingid}";
            var localVarPathParams = new Dictionary<String, String>();
            var localVarQueryParams = new Dictionary<String, String>();
            var localVarHeaderParams = new Dictionary<String, String>(Configuration.DefaultHeader);
            var localVarFormParams = new Dictionary<String, String>();
            var localVarFileParams = new Dictionary<String, FileParameter>();
            Object localVarPostBody = null;

            // to determine the Content-Type header
            String[] localVarHttpContentTypes = new String[] {
            };
            String localVarHttpContentType = Configuration.ApiClient.SelectHeaderContentType(localVarHttpContentTypes);

            // to determine the Accept header
            String[] localVarHttpHeaderAccepts = new String[] {
                "application/json",
                "text/json",
                "application/xml",
                "text/xml"
            };
            String localVarHttpHeaderAccept = Configuration.ApiClient.SelectHeaderAccept(localVarHttpHeaderAccepts);
            if (localVarHttpHeaderAccept != null)
                localVarHeaderParams.Add("Accept", localVarHttpHeaderAccept);

            // set "format" to json by default
            // e.g. /pet/{petId}.{format} becomes /pet/{petId}.json
            localVarPathParams.Add("format", "json");
            if (owner != null) localVarPathParams.Add("owner", Configuration.ApiClient.ParameterToString(owner)); // path parameter
            if (dumplingid != null) localVarPathParams.Add("dumplingid", Configuration.ApiClient.ParameterToString(dumplingid)); // path parameter


            // make the HTTP request
            IRestResponse localVarResponse = (IRestResponse)Configuration.ApiClient.CallApi(localVarPath,
                Method.GET, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams, localVarFileParams,
                localVarPathParams, localVarHttpContentType);

            int localVarStatusCode = (int)localVarResponse.StatusCode;

            if (localVarStatusCode >= 400)
                throw new ApiException(localVarStatusCode, "Error calling DumplingServiceGetStatus: " + localVarResponse.Content, localVarResponse.Content);
            else if (localVarStatusCode == 0)
                throw new ApiException(localVarStatusCode, "Error calling DumplingServiceGetStatus: " + localVarResponse.ErrorMessage, localVarResponse.ErrorMessage);

            return new ApiResponse<string>(localVarStatusCode,
                localVarResponse.Headers.ToDictionary(x => x.Name, x => x.Value.ToString()),
                (string)Configuration.ApiClient.Deserialize(localVarResponse, typeof(string)));
        }

        /// <summary>
        /// returns the current status of a dumpling. 
        /// </summary>
        /// <exception cref="IO.Swagger.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="owner"></param>
        /// <param name="dumplingid"></param>
        /// <returns>Task of string</returns>
        public async System.Threading.Tasks.Task<string> DumplingServiceGetStatusAsync(string owner, string dumplingid)
        {
            ApiResponse<string> localVarResponse = await DumplingServiceGetStatusAsyncWithHttpInfo(owner, dumplingid);
            return localVarResponse.Data;
        }

        /// <summary>
        /// returns the current status of a dumpling. 
        /// </summary>
        /// <exception cref="IO.Swagger.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="owner"></param>
        /// <param name="dumplingid"></param>
        /// <returns>Task of ApiResponse (string)</returns>
        public async System.Threading.Tasks.Task<ApiResponse<string>> DumplingServiceGetStatusAsyncWithHttpInfo(string owner, string dumplingid)
        {
            // verify the required parameter 'owner' is set
            if (owner == null)
                throw new ApiException(400, "Missing required parameter 'owner' when calling DumplingServiceApi->DumplingServiceGetStatus");
            // verify the required parameter 'dumplingid' is set
            if (dumplingid == null)
                throw new ApiException(400, "Missing required parameter 'dumplingid' when calling DumplingServiceApi->DumplingServiceGetStatus");

            var localVarPath = "/dumpling/status/{owner}/{dumplingid}";
            var localVarPathParams = new Dictionary<String, String>();
            var localVarQueryParams = new Dictionary<String, String>();
            var localVarHeaderParams = new Dictionary<String, String>(Configuration.DefaultHeader);
            var localVarFormParams = new Dictionary<String, String>();
            var localVarFileParams = new Dictionary<String, FileParameter>();
            Object localVarPostBody = null;

            // to determine the Content-Type header
            String[] localVarHttpContentTypes = new String[] {
            };
            String localVarHttpContentType = Configuration.ApiClient.SelectHeaderContentType(localVarHttpContentTypes);

            // to determine the Accept header
            String[] localVarHttpHeaderAccepts = new String[] {
                "application/json",
                "text/json",
                "application/xml",
                "text/xml"
            };
            String localVarHttpHeaderAccept = Configuration.ApiClient.SelectHeaderAccept(localVarHttpHeaderAccepts);
            if (localVarHttpHeaderAccept != null)
                localVarHeaderParams.Add("Accept", localVarHttpHeaderAccept);

            // set "format" to json by default
            // e.g. /pet/{petId}.{format} becomes /pet/{petId}.json
            localVarPathParams.Add("format", "json");
            if (owner != null) localVarPathParams.Add("owner", Configuration.ApiClient.ParameterToString(owner)); // path parameter
            if (dumplingid != null) localVarPathParams.Add("dumplingid", Configuration.ApiClient.ParameterToString(dumplingid)); // path parameter


            // make the HTTP request
            IRestResponse localVarResponse = (IRestResponse)await Configuration.ApiClient.CallApiAsync(localVarPath,
                Method.GET, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams, localVarFileParams,
                localVarPathParams, localVarHttpContentType);

            int localVarStatusCode = (int)localVarResponse.StatusCode;

            if (localVarStatusCode >= 400)
                throw new ApiException(localVarStatusCode, "Error calling DumplingServiceGetStatus: " + localVarResponse.Content, localVarResponse.Content);
            else if (localVarStatusCode == 0)
                throw new ApiException(localVarStatusCode, "Error calling DumplingServiceGetStatus: " + localVarResponse.ErrorMessage, localVarResponse.ErrorMessage);

            return new ApiResponse<string>(localVarStatusCode,
                localVarResponse.Headers.ToDictionary(x => x.Name, x => x.Value.ToString()),
                (string)Configuration.ApiClient.Deserialize(localVarResponse, typeof(string)));
        }

        /// <summary>
        ///  
        /// </summary>
        /// <exception cref="IO.Swagger.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="owner"></param>
        /// <param name="targetos"></param>
        /// <param name="index"></param>
        /// <param name="filesize"></param>
        /// <returns>string</returns>
        public string DumplingServicePostDumpChunk(string owner, string targetos, int? index, long? filesize)
        {
            ApiResponse<string> localVarResponse = DumplingServicePostDumpChunkWithHttpInfo(owner, targetos, index, filesize);
            return localVarResponse.Data;
        }

        /// <summary>
        ///  
        /// </summary>
        /// <exception cref="IO.Swagger.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="owner"></param>
        /// <param name="targetos"></param>
        /// <param name="index"></param>
        /// <param name="filesize"></param>
        /// <returns>ApiResponse of string</returns>
        public ApiResponse<string> DumplingServicePostDumpChunkWithHttpInfo(string owner, string targetos, int? index, long? filesize)
        {
            // verify the required parameter 'owner' is set
            if (owner == null)
                throw new ApiException(400, "Missing required parameter 'owner' when calling DumplingServiceApi->DumplingServicePostDumpChunk");
            // verify the required parameter 'targetos' is set
            if (targetos == null)
                throw new ApiException(400, "Missing required parameter 'targetos' when calling DumplingServiceApi->DumplingServicePostDumpChunk");
            // verify the required parameter 'index' is set
            if (index == null)
                throw new ApiException(400, "Missing required parameter 'index' when calling DumplingServiceApi->DumplingServicePostDumpChunk");
            // verify the required parameter 'filesize' is set
            if (filesize == null)
                throw new ApiException(400, "Missing required parameter 'filesize' when calling DumplingServiceApi->DumplingServicePostDumpChunk");

            var localVarPath = "/dumpling/store/chunk/{owner}/{targetos}/{index}/{filesize}";
            var localVarPathParams = new Dictionary<String, String>();
            var localVarQueryParams = new Dictionary<String, String>();
            var localVarHeaderParams = new Dictionary<String, String>(Configuration.DefaultHeader);
            var localVarFormParams = new Dictionary<String, String>();
            var localVarFileParams = new Dictionary<String, FileParameter>();
            Object localVarPostBody = null;

            // to determine the Content-Type header
            String[] localVarHttpContentTypes = new String[] {
            };
            String localVarHttpContentType = Configuration.ApiClient.SelectHeaderContentType(localVarHttpContentTypes);

            // to determine the Accept header
            String[] localVarHttpHeaderAccepts = new String[] {
                "application/json",
                "text/json",
                "application/xml",
                "text/xml"
            };
            String localVarHttpHeaderAccept = Configuration.ApiClient.SelectHeaderAccept(localVarHttpHeaderAccepts);
            if (localVarHttpHeaderAccept != null)
                localVarHeaderParams.Add("Accept", localVarHttpHeaderAccept);

            // set "format" to json by default
            // e.g. /pet/{petId}.{format} becomes /pet/{petId}.json
            localVarPathParams.Add("format", "json");
            if (owner != null) localVarPathParams.Add("owner", Configuration.ApiClient.ParameterToString(owner)); // path parameter
            if (targetos != null) localVarPathParams.Add("targetos", Configuration.ApiClient.ParameterToString(targetos)); // path parameter
            if (index != null) localVarPathParams.Add("index", Configuration.ApiClient.ParameterToString(index)); // path parameter
            if (filesize != null) localVarPathParams.Add("filesize", Configuration.ApiClient.ParameterToString(filesize)); // path parameter


            // make the HTTP request
            IRestResponse localVarResponse = (IRestResponse)Configuration.ApiClient.CallApi(localVarPath,
                Method.POST, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams, localVarFileParams,
                localVarPathParams, localVarHttpContentType);

            int localVarStatusCode = (int)localVarResponse.StatusCode;

            if (localVarStatusCode >= 400)
                throw new ApiException(localVarStatusCode, "Error calling DumplingServicePostDumpChunk: " + localVarResponse.Content, localVarResponse.Content);
            else if (localVarStatusCode == 0)
                throw new ApiException(localVarStatusCode, "Error calling DumplingServicePostDumpChunk: " + localVarResponse.ErrorMessage, localVarResponse.ErrorMessage);

            return new ApiResponse<string>(localVarStatusCode,
                localVarResponse.Headers.ToDictionary(x => x.Name, x => x.Value.ToString()),
                (string)Configuration.ApiClient.Deserialize(localVarResponse, typeof(string)));
        }

        /// <summary>
        ///  
        /// </summary>
        /// <exception cref="IO.Swagger.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="owner"></param>
        /// <param name="targetos"></param>
        /// <param name="index"></param>
        /// <param name="filesize"></param>
        /// <returns>Task of string</returns>
        public async System.Threading.Tasks.Task<string> DumplingServicePostDumpChunkAsync(string owner, string targetos, int? index, long? filesize)
        {
            ApiResponse<string> localVarResponse = await DumplingServicePostDumpChunkAsyncWithHttpInfo(owner, targetos, index, filesize);
            return localVarResponse.Data;
        }

        /// <summary>
        ///  
        /// </summary>
        /// <exception cref="IO.Swagger.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="owner"></param>
        /// <param name="targetos"></param>
        /// <param name="index"></param>
        /// <param name="filesize"></param>
        /// <returns>Task of ApiResponse (string)</returns>
        public async System.Threading.Tasks.Task<ApiResponse<string>> DumplingServicePostDumpChunkAsyncWithHttpInfo(string owner, string targetos, int? index, long? filesize)
        {
            // verify the required parameter 'owner' is set
            if (owner == null)
                throw new ApiException(400, "Missing required parameter 'owner' when calling DumplingServiceApi->DumplingServicePostDumpChunk");
            // verify the required parameter 'targetos' is set
            if (targetos == null)
                throw new ApiException(400, "Missing required parameter 'targetos' when calling DumplingServiceApi->DumplingServicePostDumpChunk");
            // verify the required parameter 'index' is set
            if (index == null)
                throw new ApiException(400, "Missing required parameter 'index' when calling DumplingServiceApi->DumplingServicePostDumpChunk");
            // verify the required parameter 'filesize' is set
            if (filesize == null)
                throw new ApiException(400, "Missing required parameter 'filesize' when calling DumplingServiceApi->DumplingServicePostDumpChunk");

            var localVarPath = "/dumpling/store/chunk/{owner}/{targetos}/{index}/{filesize}";
            var localVarPathParams = new Dictionary<String, String>();
            var localVarQueryParams = new Dictionary<String, String>();
            var localVarHeaderParams = new Dictionary<String, String>(Configuration.DefaultHeader);
            var localVarFormParams = new Dictionary<String, String>();
            var localVarFileParams = new Dictionary<String, FileParameter>();
            Object localVarPostBody = null;

            // to determine the Content-Type header
            String[] localVarHttpContentTypes = new String[] {
            };
            String localVarHttpContentType = Configuration.ApiClient.SelectHeaderContentType(localVarHttpContentTypes);

            // to determine the Accept header
            String[] localVarHttpHeaderAccepts = new String[] {
                "application/json",
                "text/json",
                "application/xml",
                "text/xml"
            };
            String localVarHttpHeaderAccept = Configuration.ApiClient.SelectHeaderAccept(localVarHttpHeaderAccepts);
            if (localVarHttpHeaderAccept != null)
                localVarHeaderParams.Add("Accept", localVarHttpHeaderAccept);

            // set "format" to json by default
            // e.g. /pet/{petId}.{format} becomes /pet/{petId}.json
            localVarPathParams.Add("format", "json");
            if (owner != null) localVarPathParams.Add("owner", Configuration.ApiClient.ParameterToString(owner)); // path parameter
            if (targetos != null) localVarPathParams.Add("targetos", Configuration.ApiClient.ParameterToString(targetos)); // path parameter
            if (index != null) localVarPathParams.Add("index", Configuration.ApiClient.ParameterToString(index)); // path parameter
            if (filesize != null) localVarPathParams.Add("filesize", Configuration.ApiClient.ParameterToString(filesize)); // path parameter


            // make the HTTP request
            IRestResponse localVarResponse = (IRestResponse)await Configuration.ApiClient.CallApiAsync(localVarPath,
                Method.POST, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams, localVarFileParams,
                localVarPathParams, localVarHttpContentType);

            int localVarStatusCode = (int)localVarResponse.StatusCode;

            if (localVarStatusCode >= 400)
                throw new ApiException(localVarStatusCode, "Error calling DumplingServicePostDumpChunk: " + localVarResponse.Content, localVarResponse.Content);
            else if (localVarStatusCode == 0)
                throw new ApiException(localVarStatusCode, "Error calling DumplingServicePostDumpChunk: " + localVarResponse.ErrorMessage, localVarResponse.ErrorMessage);

            return new ApiResponse<string>(localVarStatusCode,
                localVarResponse.Headers.ToDictionary(x => x.Name, x => x.Value.ToString()),
                (string)Configuration.ApiClient.Deserialize(localVarResponse, typeof(string)));
        }

        /// <summary>
        /// This is just here to test service availability. \r\n            \r\n            \r\n            curl http://[dotnetrp].net/dumpling/test/hi/im/(yourname) 
        /// </summary>
        /// <exception cref="IO.Swagger.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="name"></param>
        /// <returns>string</returns>
        public string DumplingServiceSayHi(string name)
        {
            ApiResponse<string> localVarResponse = DumplingServiceSayHiWithHttpInfo(name);
            return localVarResponse.Data;
        }

        /// <summary>
        /// This is just here to test service availability. \r\n            \r\n            \r\n            curl http://[dotnetrp].net/dumpling/test/hi/im/(yourname) 
        /// </summary>
        /// <exception cref="IO.Swagger.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="name"></param>
        /// <returns>ApiResponse of string</returns>
        public ApiResponse<string> DumplingServiceSayHiWithHttpInfo(string name)
        {
            // verify the required parameter 'name' is set
            if (name == null)
                throw new ApiException(400, "Missing required parameter 'name' when calling DumplingServiceApi->DumplingServiceSayHi");

            var localVarPath = "/dumpling/test/hi/im/{name}";
            var localVarPathParams = new Dictionary<String, String>();
            var localVarQueryParams = new Dictionary<String, String>();
            var localVarHeaderParams = new Dictionary<String, String>(Configuration.DefaultHeader);
            var localVarFormParams = new Dictionary<String, String>();
            var localVarFileParams = new Dictionary<String, FileParameter>();
            Object localVarPostBody = null;

            // to determine the Content-Type header
            String[] localVarHttpContentTypes = new String[] {
            };
            String localVarHttpContentType = Configuration.ApiClient.SelectHeaderContentType(localVarHttpContentTypes);

            // to determine the Accept header
            String[] localVarHttpHeaderAccepts = new String[] {
                "application/json",
                "text/json",
                "application/xml",
                "text/xml"
            };
            String localVarHttpHeaderAccept = Configuration.ApiClient.SelectHeaderAccept(localVarHttpHeaderAccepts);
            if (localVarHttpHeaderAccept != null)
                localVarHeaderParams.Add("Accept", localVarHttpHeaderAccept);

            // set "format" to json by default
            // e.g. /pet/{petId}.{format} becomes /pet/{petId}.json
            localVarPathParams.Add("format", "json");
            if (name != null) localVarPathParams.Add("name", Configuration.ApiClient.ParameterToString(name)); // path parameter


            // make the HTTP request
            IRestResponse localVarResponse = (IRestResponse)Configuration.ApiClient.CallApi(localVarPath,
                Method.GET, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams, localVarFileParams,
                localVarPathParams, localVarHttpContentType);

            int localVarStatusCode = (int)localVarResponse.StatusCode;

            if (localVarStatusCode >= 400)
                throw new ApiException(localVarStatusCode, "Error calling DumplingServiceSayHi: " + localVarResponse.Content, localVarResponse.Content);
            else if (localVarStatusCode == 0)
                throw new ApiException(localVarStatusCode, "Error calling DumplingServiceSayHi: " + localVarResponse.ErrorMessage, localVarResponse.ErrorMessage);

            return new ApiResponse<string>(localVarStatusCode,
                localVarResponse.Headers.ToDictionary(x => x.Name, x => x.Value.ToString()),
                (string)Configuration.ApiClient.Deserialize(localVarResponse, typeof(string)));
        }

        /// <summary>
        /// This is just here to test service availability. \r\n            \r\n            \r\n            curl http://[dotnetrp].net/dumpling/test/hi/im/(yourname) 
        /// </summary>
        /// <exception cref="IO.Swagger.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="name"></param>
        /// <returns>Task of string</returns>
        public async System.Threading.Tasks.Task<string> DumplingServiceSayHiAsync(string name)
        {
            ApiResponse<string> localVarResponse = await DumplingServiceSayHiAsyncWithHttpInfo(name);
            return localVarResponse.Data;
        }

        /// <summary>
        /// This is just here to test service availability. \r\n            \r\n            \r\n            curl http://[dotnetrp].net/dumpling/test/hi/im/(yourname) 
        /// </summary>
        /// <exception cref="IO.Swagger.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="name"></param>
        /// <returns>Task of ApiResponse (string)</returns>
        public async System.Threading.Tasks.Task<ApiResponse<string>> DumplingServiceSayHiAsyncWithHttpInfo(string name)
        {
            // verify the required parameter 'name' is set
            if (name == null)
                throw new ApiException(400, "Missing required parameter 'name' when calling DumplingServiceApi->DumplingServiceSayHi");

            var localVarPath = "/dumpling/test/hi/im/{name}";
            var localVarPathParams = new Dictionary<String, String>();
            var localVarQueryParams = new Dictionary<String, String>();
            var localVarHeaderParams = new Dictionary<String, String>(Configuration.DefaultHeader);
            var localVarFormParams = new Dictionary<String, String>();
            var localVarFileParams = new Dictionary<String, FileParameter>();
            Object localVarPostBody = null;

            // to determine the Content-Type header
            String[] localVarHttpContentTypes = new String[] {
            };
            String localVarHttpContentType = Configuration.ApiClient.SelectHeaderContentType(localVarHttpContentTypes);

            // to determine the Accept header
            String[] localVarHttpHeaderAccepts = new String[] {
                "application/json",
                "text/json",
                "application/xml",
                "text/xml"
            };
            String localVarHttpHeaderAccept = Configuration.ApiClient.SelectHeaderAccept(localVarHttpHeaderAccepts);
            if (localVarHttpHeaderAccept != null)
                localVarHeaderParams.Add("Accept", localVarHttpHeaderAccept);

            // set "format" to json by default
            // e.g. /pet/{petId}.{format} becomes /pet/{petId}.json
            localVarPathParams.Add("format", "json");
            if (name != null) localVarPathParams.Add("name", Configuration.ApiClient.ParameterToString(name)); // path parameter


            // make the HTTP request
            IRestResponse localVarResponse = (IRestResponse)await Configuration.ApiClient.CallApiAsync(localVarPath,
                Method.GET, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams, localVarFileParams,
                localVarPathParams, localVarHttpContentType);

            int localVarStatusCode = (int)localVarResponse.StatusCode;

            if (localVarStatusCode >= 400)
                throw new ApiException(localVarStatusCode, "Error calling DumplingServiceSayHi: " + localVarResponse.Content, localVarResponse.Content);
            else if (localVarStatusCode == 0)
                throw new ApiException(localVarStatusCode, "Error calling DumplingServiceSayHi: " + localVarResponse.ErrorMessage, localVarResponse.ErrorMessage);

            return new ApiResponse<string>(localVarStatusCode,
                localVarResponse.Headers.ToDictionary(x => x.Name, x => x.Value.ToString()),
                (string)Configuration.ApiClient.Deserialize(localVarResponse, typeof(string)));
        }
    }
}
