
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ToolLib.Http
{
    public interface IHttpHelper
    {
        HttpResponse Get(string url);
        HttpResponse Get(string url, Dictionary<string, string> headers);
        HttpResponse Get(string url, Dictionary<string, object> requestParam);
        HttpResponse Get(string url, Dictionary<string, object> requestParam, Dictionary<string, string> headers);
        HttpResponse Post(string url, Dictionary<string, object> postParam);
        HttpResponse Post(string url, Dictionary<string, object> postParam, Dictionary<string, string> headers);
        HttpResponse Post(string url, Dictionary<string, string> headers, Dictionary<string, object> postParam, string name, byte[] fileAsByte, string fileName, string contentType);

        
        HttpResponse Put(string url, Dictionary<string, object> putParam);
        HttpResponse Put(string url, Dictionary<string, object> putParam, Dictionary<string, string> headers);
        HttpResponse Delete(string url);
        HttpResponse Delete(string url, Dictionary<string, string> headers);
    }
    public class HttpHelper : IHttpHelper
    {
        protected IRestClient _client;
        
        public HttpHelper()
        {
            _client = new RestClient();
        }
        public HttpResponse Delete(string url)
        {
            return buildAndExecute(url, Method.DELETE);
        }

        public HttpResponse Delete(string url, Dictionary<string, string> headers)
        {
            return buildAndExecute(url, Method.DELETE, headers);
        }

        public HttpResponse Get(string url)
        {
            return buildAndExecute(url, Method.GET);
        }

        public HttpResponse Get(string url, Dictionary<string, string> headers)
        {
            return buildAndExecute(url, Method.GET,headers);
        }

        public HttpResponse Get(string url, Dictionary<string, object> requestParam)
        {
            return buildAndExecute(url, Method.GET, null,requestParam);
        }

        public HttpResponse Get(string url, Dictionary<string, object> requestParam, Dictionary<string, string> headers)
        {
            return buildAndExecute(url, Method.GET, headers, requestParam);
        }

        public HttpResponse Post(string url, Dictionary<string, object> postParam)
        {
            return buildAndExecute(url, Method.POST, null, postParam);
        }

        public HttpResponse Post(string url, Dictionary<string, object> postParam, Dictionary<string, string> headers)
        {
            return buildAndExecute(url, Method.POST, headers, postParam);
        }

        public HttpResponse Post(string url, Dictionary<string, string> headers, Dictionary<string, object> postParam, string name, byte[] fileAsByte, string fileName, string contentType)
        {
            return buildAndExecuteAttachedFile(url, Method.POST, headers, postParam, name, fileAsByte, fileName, contentType);
        }

      

        public HttpResponse Put(string url, Dictionary<string, object> putParam)
        {
            return buildAndExecute(url, Method.PUT, null, putParam);
        }

        public HttpResponse Put(string url, Dictionary<string, object> putParam, Dictionary<string, string> headers)
        {
            return buildAndExecute(url, Method.POST, headers, putParam);
        }
        protected HttpResponse buildAndExecuteAttachedFile(string url, Method method, Dictionary<string, string> headers = null, Dictionary<string, object> requestParam = null, string name = "", byte[] fileAsByte = null, string fileName = "", string contentType = "image/jpeg")
        {
            var request = buildRequestAttachedFile(url, method, headers, requestParam, name, fileAsByte, fileName, contentType);
            var response = _client.Execute(request);
            //ErrorMesssage = (response.ErrorException == null ? "" : response.ErrorException.InnerException.Message);
            return new HttpResponse(response);
        }
        protected IRestRequest buildRequestAttachedFile(string url, Method method, Dictionary<string, string> headers = null, Dictionary<string, object> requestParam = null, string name = "", byte[] fileAsByte = null, string fileName = "", string contentType = "image/jpeg")
        {
            var request = new RestRequest(url, method);
            if (headers != null)
            {
                putHeader(headers, request);
            }



            foreach (var p in requestParam)
            {

                if (p.Key == "fileupload")
                {

                    request.AddFile(p.Key, (byte[])p.Value, fileName, contentType);

                }
                else
                {
                    request.AddParameter(p.Key, p.Value + "");
                }
            }
            return request;
        }
        protected HttpResponse buildAndExecute(string url, Method method, Dictionary<string, string> headers = null, Dictionary<string, object> requestParam = null)
        {
            var request = buildRequest(url, method, headers, requestParam);
            var response = _client.Execute(request);
          //  ErrorMesssage = (response.ErrorException == null ? "" : response.ErrorException.Message);

            return new HttpResponse(response);
        }
        void putHeader(Dictionary<string, string> headers, IRestRequest restRequest)
        {
            foreach (var headerItem in headers)
            {
                restRequest.AddHeader(headerItem.Key, headerItem.Value);
            }
        }

        protected IRestRequest buildRequest(string url, Method method, Dictionary<string, string> headers = null, Dictionary<string, object> requestParam = null)
        {


            var request = new RestRequest(url, method);
            if (headers != null)
            {
                putHeader(headers, request);
            }
            if (requestParam != null)
            {
                if (method == Method.GET)
                {
                    foreach (var p in requestParam)
                    {
                        request.AddParameter(p.Key, p.Value + "");

                    }
                }
                else
                {
                    request.AddBody(requestParam);

                }
            }
            return request;
        }
    }
    public class HttpResponse
    {

        protected IRestResponse _reponse;
        protected IRestRequest _request;
        protected string content;
        protected HttpStatusCode _statusCode;
        protected string _statusDesc;
        public HttpResponse()
        {

        }
        public HttpResponse(IRestResponse response)
        {
            _reponse = response;
            content = response.Content;
            _statusCode = response.StatusCode;
            _statusDesc = response.StatusDescription;
        }
        public HttpResponse(IRestRequest request, IRestResponse response)
        {
            _reponse = response;
            _request = request;
            content = response.Content;
            _statusCode = response.StatusCode;
            _statusDesc = response.StatusDescription;
        }

        public HttpStatusCode StatusCode
        {
            get
            {
                return _statusCode;
            }
        }
        public string StatusDescription
        {
            get
            {
                return _statusDesc;
            }
        }
        public string RawResponse
        {
            get
            {
                return content;
            }
        }
        public T GetResponse<T>()
        {
            return JsonConvert.DeserializeObject<T>(content);
        }
    }
}
