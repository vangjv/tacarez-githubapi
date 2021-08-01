using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using TacarEZGithubAPI.Models;
using RestSharp;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace TacarEZGithubAPI
{
    public static class UpdateFile
    {
        [FunctionName("UpdateFile")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "updatefile/{repoName}")] HttpRequest req, string repoName,
            ILogger log)
        {
            if (repoName == null)
            {
                return new BadRequestObjectResult("Invalid repository name");
            }

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            GithubContent updatedContent = JsonConvert.DeserializeObject<GithubContent>(requestBody);

            //get files from repo
            var client = new RestClient("https://api.github.com/repos/" + Utility.GetEnvironmentVariable("GitHubAccount") + "/" + repoName + "/contents");
            client.Timeout = -1;
            var request = new RestRequest(Method.GET);
            request.AddHeader("Authorization", "token " + Utility.GetEnvironmentVariable("GithubPAT"));
            IRestResponse response = client.Execute(request);
            if (!response.IsSuccessful)
            {
                return new BadRequestObjectResult("Failed to get repository contents");
            }

            List<GetContentsResponse> contentsResponse = JsonConvert.DeserializeObject<List<GetContentsResponse>>(response.Content);
            if (contentsResponse.Count <= 0)
            {
                return new BadRequestObjectResult("No files found in repository");
            }
            string fileSha = null;
            contentsResponse.ForEach(content =>
            {
                if (content.name == "data.geojson")
                {
                    fileSha = content.sha;
                    return;
                }
            });
            if (fileSha == null)
            {
                return new BadRequestObjectResult("No file found in repository");
            }

            //update file by hash
            client.BaseUrl = new Uri("https://api.github.com/repos/" + Utility.GetEnvironmentVariable("GitHubAccount") + "/" + repoName + "/contents/data.geojson");
            var updateContentRequest = new RestRequest(Method.PUT);
            GithubContent newContent = new GithubContent
            {
                content = updatedContent.content,
                message = updatedContent.message,
                sha = fileSha,
                committer = new Committer
                {
                    name = "DSHackathon",
                    email = "hackathon@meliority.solutions",
                    author = new Author
                    {
                        name = "Jonathan Vang",
                        email = "jonathan.vang@gmail.com"
                    }
                }
            };
            updateContentRequest.AddJsonBody(newContent);
            updateContentRequest.AddHeader("Content-Type", "application/json");
            updateContentRequest.AddHeader("Authorization", "token " + Utility.GetEnvironmentVariable("GithubPAT"));
            IRestResponse updateContentResponse = client.Execute(updateContentRequest);
            return new OkObjectResult(JObject.Parse(updateContentResponse.Content));
        }
    }
}
