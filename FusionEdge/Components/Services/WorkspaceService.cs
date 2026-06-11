using FusionEdge.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using FusionEdge.Data.Models;

namespace FusionEdge.Components.Services
{
    internal class WorkspaceService : IWorkspaceService
    {

        private readonly string _opcUrl = "https://primavera-au1.oraclecloud.com";
        private readonly string _opcUsername = "demo@projectpro-ph.com";
        private readonly string _opcPassword = "#OraclePrimavera2026";

        private readonly string _eppmUrl = "http://192.168.8.128:8206";
        private readonly string _eppmUsername = "cimadmin1";
        private readonly string _eppmPassword = "cimp@ssw0rd";
        private readonly string _eppmDbName = "CIMEPPM";

        private Dictionary<string, long> _epsNameToId = new();

        private readonly HttpClient _httpClient;

        public WorkspaceService()
        {
            _httpClient = new HttpClient();
        }

        private readonly Dictionary<string, List<string>> _workspaceProjects = new()
        {
            { "Workspace 1", new List<string> { "Project 1A", "Project 1B" } },
            { "Workspace 2", new List<string> { "Project 2A", "Project 2B", "Project 2C" } },
            { "Workspace 3", new List<string> { "Project 3A" } }
        };

        private long ParseObjectId(JsonElement element, string propertyName)
        {
            if (!element.TryGetProperty(propertyName, out var prop))
                return 0;
            return prop.ValueKind == JsonValueKind.String
                ? long.TryParse(prop.GetString(), out var parsed) ? parsed : 0
                : prop.GetInt64();
        }
        public async Task<List<Workspace>> GetWorkspaceAsync(string apiSource)
        {
            try
            {
                if (apiSource == "OPC")
                {
                    var tt = apiSource;
                    var client = await BuildOpcClientAsync();
                    var response = await client.GetAsync(
                        $"{_opcUrl}/api/restapi/workspace?Fields=ObjectId,Name,Id"
                    );
                    var body = await response.Content.ReadAsStringAsync();
                    using var doc = JsonDocument.Parse(body);
                    return EnumerateItems(doc.RootElement)
                        .Select(ws => new Workspace
                        {
                            ObjectId = ws.TryGetProperty("ObjectId", out var oid)
                                ? oid.GetString()
                                : null,

                            Name = ws.TryGetProperty("Name", out var name)
                                ? name.GetString()
                                : ws.TryGetProperty("workspaceName", out var wname)
                                    ? wname.GetString()
                                    : ws.TryGetProperty("Id", out var id)
                                        ? id.GetString()
                                        : null
                        })
                        .Where(w => !string.IsNullOrEmpty(w.ObjectId))
                        .ToList();
                }
                else 
                {
                    var client = await BuildEppmClientAsync();
                    var response = await client.GetAsync(
                        $"{_eppmUrl}/p6ws/restapi/eps?Fields=ObjectId,Id,Name"
                    );
                    var body = await response.Content.ReadAsStringAsync();
                    using var doc = JsonDocument.Parse(body);
                    _epsNameToId.Clear();

                    var workspaces = new List<Workspace>();
                    foreach (var eps in EnumerateItems(doc.RootElement))
                    {
                        var name = eps.TryGetProperty("Name", out var n)
                            ? n.GetString()
                            : null;
                        var objectId = ParseObjectId(eps, "ObjectId");

                        if (!string.IsNullOrEmpty(name))
                        {
                            if (objectId > 0)
                                _epsNameToId[name] = objectId;
                            workspaces.Add(new Workspace
                            {
                                ObjectId = objectId > 0 ? objectId.ToString() : null,
                                Name = name
                            });
                        }
                    }
                    return workspaces;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GetWorkspaceAsync error: {ex.Message}");
                return new List<Workspace>();

            }

            //var db = new AppDbContext();

            //var user = await db.User.OrderByDescending(u => u.Id).FirstOrDefaultAsync();

            //var authToken = user.AccessToken ?? "";
            //_httpClient.DefaultRequestHeaders.Authorization =
            //        new AuthenticationHeaderValue("Basic", authToken);


            //var response = await _httpClient.GetAsync(
            //     "http://192.168.8.128:8206/p6ws/restapi/eps?DatabaseName=CIMEPPM&Fields=ObjectId,Id,Name"
            // );

            //var result = await response.Content.ReadAsStringAsync();

            //return new List<string> { result };
            //var baseUrl = "http://192.168.8.128:8206";


            //var raw = $"{username}:{password}";

            //var authToken = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(raw));


            //var cookieContainer = new CookieContainer();


            //var handler = new HttpClientHandler
            //{

            //    CookieContainer = cookieContainer,

            //    ServerCertificateCustomValidationCallback = (msg, cert, chain, errors) => true
            //};


            //var client = new HttpClient(handler);


            // 1. LOGIN FIRST (required to establish session)client.DefaultRequestHeaders.Clear();

            //client.DefaultRequestHeaders.Add("authToken", authToken);


            //var loginResponse = await client.PostAsync(

            //    $"{baseUrl}/p6ws/restapi/login?DatabaseName=CIMEPPM",

            //    null);


            //var loginResult = await loginResponse.Content.ReadAsStringAsync();


            //// 2. GET WORKSPACE (EPS)
            //var epsResponse = await client.GetAsync(
            //    $"{baseUrl}/p6ws/restapi/eps?DatabaseName=CIMEPPM&Fields=ObjectId,Id,Name"            
            //);


            //var epsResult = await epsResponse.Content.ReadAsStringAsync();
            //var workspaces = JsonSerializer.Deserialize<List<Workspace>>(epsResult);

            //return workspaces ?? new List<Workspace>();

            //return workspaces?
            //    .Select(w => w.Name)
            //    .ToList()
            //    ?? new List<string>();

        }



        private async Task<HttpClient> BuildEppmClientAsync()

        {

            var authToken = Convert.ToBase64String(

                System.Text.Encoding.UTF8.GetBytes($"{_eppmUsername}:{_eppmPassword}")

            );

            var cookieContainer = new CookieContainer();

            var handler = new HttpClientHandler

            {

                CookieContainer = cookieContainer,

                ServerCertificateCustomValidationCallback = (_, _, _, _) => true

            };

            var client = new HttpClient(handler);

            client.DefaultRequestHeaders.Clear();

            client.DefaultRequestHeaders.TryAddWithoutValidation("authToken", authToken);

            // Must login first to get session cookie

            await client.PostAsync(

                $"{_eppmUrl}/p6ws/restapi/login?DatabaseName={_eppmDbName}",

                null

            );

            return client;

        }
        private async Task<HttpClient> BuildOpcClientAsync()

        {

            var client = new HttpClient();

            // Step 1: Get access token

            var basicToken = Convert.ToBase64String(

                System.Text.Encoding.UTF8.GetBytes($"{_opcUsername}:{_opcPassword}")

            );

            client.DefaultRequestHeaders.Authorization =

                new AuthenticationHeaderValue("Basic", basicToken);

            var tokenResponse = await client.PostAsync(

                $"{_opcUrl}/primediscovery/apitoken/request?scope=http://primavera-au1.oraclecloud.com/api",

                null

            );

            var tokenBody = await tokenResponse.Content.ReadAsStringAsync();

            using var doc = JsonDocument.Parse(tokenBody);

            var accessToken = doc.RootElement.GetProperty("accessToken").GetString() ?? "";

            // Step 2: Use access token for API calls

            client.DefaultRequestHeaders.Clear();

            client.DefaultRequestHeaders.Authorization =

                new AuthenticationHeaderValue("Bearer", accessToken);

            return client;

        }

        private IEnumerable<JsonElement> EnumerateItems(JsonElement root)
        {
            if (root.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in root.EnumerateArray())
                    yield return item;
            }

            else if (root.ValueKind == JsonValueKind.Object)
            {
                foreach (var key in new[] { "data", "Data", "results", "Results", "items", "Items" })
                {
                    if (root.TryGetProperty(key, out var nested) &&

                        nested.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var item in nested.EnumerateArray())
                            yield return item;
                        yield break;
                    }
                }
                yield return root;
            }
        }

        public async Task<List<Project>> GetProjectsAsync(string workspace, string apiSource)
        {
            try
            {

                if (apiSource == "OPC")
                {
                    var client = await BuildOpcClientAsync();
                    var response = await client.GetAsync(

                        $"{_opcUrl}/api/restapi/project" +
                        $"?Filter=WorkspaceName:eq:'{Uri.EscapeDataString(workspace)}'" +
                        $"&Fields=ObjectId,Id,Name,Status"
                    );

                    var body = await response.Content.ReadAsStringAsync();
                    using var doc = JsonDocument.Parse(body);

                    return EnumerateItems(doc.RootElement)
                        .Select(p => new Project
                        {
                            ObjectId = p.TryGetProperty("ObjectId", out var oid)
                                ? oid.GetString()
                                : null,

                            Name = p.TryGetProperty("Name", out var name)
                                ? name.GetString()
                                : p.TryGetProperty("Id", out var id)
                                    ? id.GetString()
                                    : null
                        })
                        .Where(p => !string.IsNullOrEmpty(p.ObjectId))
                        .ToList();
                }
                else // EPPM
                {
                    var client = await BuildEppmClientAsync();
                    //var url = _epsNameToId.TryGetValue(workspace, out var epsObjectId)

                    //    ? $"{_eppmUrl}/p6ws/restapi/project" +
                    //      $"?Filter=ParentEPSObjectId:eq:{epsObjectId}" +
                    //      $"&Fields=ObjectId,Id,Name,Status"
                    //    : $"{_eppmUrl}/p6ws/restapi/project" +
                    //      $"?Fields=ObjectId,Id,Name,Status";

                    var url = $"{_eppmUrl}/p6ws/restapi/project" +
                          $"?Filter=ParentEPSObjectId:eq:{workspace}" +
                          $"&Fields=ObjectId,Id,Name,Status";

                    var response = await client.GetAsync(url);
                    var body = await response.Content.ReadAsStringAsync();
                    using var doc = JsonDocument.Parse(body);

                    return EnumerateItems(doc.RootElement)
                        .Select(p => new Project
                        {
                            ObjectId = p.TryGetProperty("ObjectId", out var oid)
                                ? oid.GetString()
                                : null,

                            Name = p.TryGetProperty("Name", out var name)
                                ? name.GetString()
                                : p.TryGetProperty("Id", out var id)
                                    ? id.GetString()
                                    : null
                        })
                        .Where(p => !string.IsNullOrEmpty(p.ObjectId))
                        .ToList();

                }

            }
            catch (Exception ex)
            {
                Console.WriteLine($"GetProjectsAsync error: {ex.Message}");

                return new List<Project>();

            }

        }

        //public async Task<List<Project>> GetProjectsAsync(string workspace, string apiSource)
        //{
        //    try
        //    {
        //var baseUrl = "http://192.168.8.128:8206";

        //var raw = $"{username}:{password}";

        //var authToken = Convert.ToBase64String(
        //    Encoding.UTF8.GetBytes(raw)
        //);

        // STORE SESSION COOKIE
        //var cookieContainer = new CookieContainer();

        //var handler = new HttpClientHandler
        //{
        //    CookieContainer = cookieContainer,

        //    ServerCertificateCustomValidationCallback =
        //        (msg, cert, chain, errors) => true
        //};

        //var client = new HttpClient(handler);

        //client.DefaultRequestHeaders.Clear();

        //client.DefaultRequestHeaders.Add("authToken", authToken);

        // STEP 1: LOGIN FIRST
        //var loginResponse = await client.PostAsync(
        //    $"{baseUrl}/p6ws/restapi/login?DatabaseName=CIMEPPM",
        //    null
        //);

        //if (!loginResponse.IsSuccessStatusCode)
        //{
        //    return new List<Project>();
        //}

        // STEP 2: GET PROJECTS
        //var response = await client.GetAsync(
        //    $"{baseUrl}/p6ws/restapi/project?DatabaseName=CIMEPPM&Fields=ObjectId,Id,Name,ParentEPSObjectId"
        //);

        //var result = await response.Content.ReadAsStringAsync();
        //var projects = JsonSerializer.Deserialize<List<Project>>(result);

        //return projects?
        //    .Where(p => p.ParentEPSObjectId == workspace)
        //    .ToList()
        //    ?? new List<Project>();
        //return projects ?? new List<Project>();

        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine(ex.Message);
        //        return new List<Project>();
        //    }
        //}

    }

}
