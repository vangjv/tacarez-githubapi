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
using Newtonsoft.Json.Linq;

namespace TacarEZGithubAPI
{
    public static class CreateRepo
    {
        [FunctionName("CreateRepo")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            NewRepoRequest newRepo = JsonConvert.DeserializeObject<NewRepoRequest>(requestBody);

            var client = new RestClient("https://api.github.com/user/repos");
            client.Timeout = -1;
            var request = new RestRequest(Method.POST);
            request.AddHeader("Content-Type", "application/json");
            request.AddHeader("Authorization", "token " + Utility.GetEnvironmentVariable("GithubPAT"));
            request.AddJsonBody(newRepo);
            IRestResponse response = client.Execute(request);
            dynamic data = JsonConvert.DeserializeObject(response.Content);
            if (response.IsSuccessful)
            {
                //add content
                client.BaseUrl = new Uri("https://api.github.com/repos/" + data.full_name + "/contents/" + newRepo.fileName);
                var newContentRequest = new RestRequest(Method.PUT);
                GithubContent newContent = new GithubContent
                {
                    content = newRepo.content,
                    message = newRepo.message
                };
                newContentRequest.AddJsonBody(newContent);
                newContentRequest.AddHeader("Content-Type", "application/json");
                newContentRequest.AddHeader("Authorization", "token " + Utility.GetEnvironmentVariable("GithubPAT"));
                IRestResponse newContentResponse = client.Execute(newContentRequest);
                return new OkObjectResult(JObject.Parse(newContentResponse.Content));
            } else
            {
                return new BadRequestObjectResult(JObject.Parse(response.Content));
            }
        }
    }
}
