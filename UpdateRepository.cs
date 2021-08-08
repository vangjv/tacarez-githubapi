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
using Octokit;
using System.Linq;

namespace TacarEZGithubAPI
{
    public class UpdateRepository
    {
        private GitHubClient _gitHubClient;

        public UpdateRepository(GitHubClient gitHubClient)
        {
            _gitHubClient = gitHubClient;
        }

        [FunctionName("UpdateRepository")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "updaterepo/{repoName}/{branch}")] HttpRequest req, string repoName,
            string branch, ILogger log)
        {
            if (repoName == null)
            {
                return new BadRequestObjectResult("Invalid repository name");
            }

            if (branch == null)
            {
                return new BadRequestObjectResult("Invalid branch name");
            }
            string gitHubAccount = Utility.GetEnvironmentVariable("GitHubAccount");
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            GithubContent updatedContent = JsonConvert.DeserializeObject<GithubContent>(requestBody);

            RepositoryContentChangeSet repoChangeSet;
            try
            {
                // try to get the file (and with the file the last commit sha)
                var existingFile = await _gitHubClient.Repository.Content.GetAllContentsByRef(gitHubAccount, repoName, "data.geojson", branch);

                // update the file
                repoChangeSet = await _gitHubClient.Repository.Content.UpdateFile(gitHubAccount, repoName, "data.geojson",
                    new UpdateFileRequest(updatedContent.message, updatedContent.content, existingFile[0].Sha, branch, false));           
            }
            catch (Octokit.NotFoundException)
            {
                // if file is not found, create it
                repoChangeSet = await _gitHubClient.Repository.Content.CreateFile(gitHubAccount, repoName, "data.geojson", new CreateFileRequest(updatedContent.message, updatedContent.content, branch, false));
            }

            return new OkObjectResult(repoChangeSet);
        }
    }
}
