using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Web;
using System.Web.Script.Serialization;

namespace EZB.PackServerEngine
{
    public class Server : IDisposable
    {
        public const short DefaultPort = 8710;

        internal Server(PackageManager packageManager, short port, string iface)
        {
            _packageManager = packageManager ?? throw new ArgumentException("Package manager must be provided");
            _restServer = new Common.RESTServer(OnRequest, port, "packages", iface);
            _restServer.Start();
        }

        public void Dispose()
        {
            if (_restServer != null)
            {
                _restServer.Stop();
                _restServer.Dispose();
                _restServer = null;
            }
        }

        private void OnRequest(HttpListenerRequest request, HttpListenerResponse response)
        {
            bool isGET = (request.HttpMethod == "GET");
            bool isPUT = (request.HttpMethod == "PUT");
            bool isDELETE = (request.HttpMethod == "DELETE");
            if (!isGET && !isPUT && !isDELETE)
            {
                response.StatusCode = (int)HttpStatusCode.MethodNotAllowed;
                return;
            }

            string name = null;
            string version = null;
            if (isGET || isDELETE)
            {
                try
                {
                    NameValueCollection query = HttpUtility.ParseQueryString(request.Url.Query);
                    name = query.Get("name");
                    version = query.Get("version");
                }
                catch
                {
                    response.StatusCode = (int)HttpStatusCode.BadRequest;
                    return;
                }
            }

            try
            {
                if (isGET)
                    response.StatusCode = (int)OnGETRequest(request, response, name, version);
                else if (isPUT)
                    response.StatusCode = (int)OnPUTRequest(request, response);
                else if (isDELETE)
                    response.StatusCode = (int)OnDELETERequest(request, response, name, version);
            }
            catch
            {
                response.StatusCode = (int)HttpStatusCode.InternalServerError;
            }
        }

        private HttpStatusCode OnGETRequest(HttpListenerRequest request, HttpListenerResponse response, string name, string version)
        {
            try
            {
                if (request.Url.PathAndQuery.StartsWith("/packages?"))
                {
                    response.ContentType = "application/x-zip-compressed";
                    response.Headers["Content-Disposition"] = $"attachment; filename={name}_{version}.zip";
                    _packageManager.GetPackage(name, version, response.OutputStream);
                }
                else if (request.Url.PathAndQuery.StartsWith("/packages/list?") ||
                         request.Url.PathAndQuery == "/packages/list" || request.Url.PathAndQuery == "/packages/list/")
                {
                    List<PackEngine.PackageInfo> packages = _packageManager.ListPackages(name, version);
                    if (packages == null)
                        packages = new List<PackEngine.PackageInfo>();

                    List<object> packagesRoot = new List<object>();
                    foreach (PackEngine.PackageInfo package in packages)
                    {
                        Dictionary<string, object> packageRoot = new Dictionary<string, object>();
                        packageRoot["name"] = package.Name;
                        packageRoot["version"] = package.Version.ToString(4);
                        packagesRoot.Add(packageRoot);
                    }
                    Dictionary<string, object> root = new Dictionary<string, object>();
                    root["packages"] = packagesRoot;

                    JavaScriptSerializer serializer = new JavaScriptSerializer();
                    string json = serializer.Serialize(root);

                    response.ContentType = "application/json";
                    using (StreamWriter writer = new StreamWriter(response.OutputStream))
                        writer.Write(json);
                }
                else
                {
                    return HttpStatusCode.BadRequest;
                }
            }
            catch
            {
                return HttpStatusCode.NotFound;
            }

            return HttpStatusCode.OK;
        }

        private HttpStatusCode OnPUTRequest(HttpListenerRequest request, HttpListenerResponse response)
        {
            try
            {
                if (request.Url.PathAndQuery== "/packages" || request.Url.PathAndQuery == "/packages/")
                {
                    if ((request.ContentLength64 == 0) ||
                        (!string.IsNullOrEmpty(request.ContentType) && !request.ContentType.Contains("zip")))
                    {
                        return HttpStatusCode.BadRequest;
                    }

                    if (!_packageManager.AddPackage(request.InputStream))
                        return HttpStatusCode.Conflict;
                }
                else
                {
                    return HttpStatusCode.BadRequest;
                }
            }
            catch
            {
                return HttpStatusCode.InternalServerError;
            }

            return HttpStatusCode.Created;
        }

        private HttpStatusCode OnDELETERequest(HttpListenerRequest request, HttpListenerResponse response, string name, string version)
        {
            try
            {
                if (request.Url.PathAndQuery.StartsWith("/packages?"))
                {
                    _packageManager.RemovePackage(name, Version.Parse(version));
                }
                else
                {
                    return HttpStatusCode.BadRequest;
                }
            }
            catch
            {
                return HttpStatusCode.NotFound;
            }

            return HttpStatusCode.OK;
        }

        private PackageManager _packageManager;
        private Common.RESTServer _restServer;
    }
}
