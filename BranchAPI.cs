using System.IO;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using Octokit;
using TacarEZGithubAPI.Models;

namespace TacarEZGithubAPI
{
    public class BranchAPI
    {
        private GitHubClient _gitHubClient;
        private string gitHubAccount = Utility.GetEnvironmentVariable("GitHubAccount");
        public BranchAPI(GitHubClient gitHubClient)
        {
            _gitHubClient = gitHubClient;
        }


        [FunctionName("CreateBranch")]
        public async Task<IActionResult> CreateBranch(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "branch")] HttpRequest req,
            ILogger log)
        {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            NewBranchRequest newBranch = JsonConvert.DeserializeObject< NewBranchRequest>(requestBody);
            if (newBranch.BranchName == null || newBranch.RepoName == null)
            {
                return new BadRequestObjectResult("Invalid branch name or repository name");
            }
            Branch ghBranch = await _gitHubClient.Repository.Branch.Get(gitHubAccount, newBranch.RepoName, "main");
            GitReference gitRef = ghBranch.Commit;
            string gitRefSha = gitRef.Sha;

            NewReference newRef = new NewReference("refs/heads/" + newBranch.BranchName, gitRefSha);

            Reference newRefResponse = await _gitHubClient.Git.Reference.Create(gitHubAccount, newBranch.RepoName, newRef);
            return new OkObjectResult(newRefResponse);
        }

        //used to create merges which are copies of branches
        [FunctionName("CopyBranch")]
        public async Task<IActionResult> CopyBranch(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "branch/{branchName}")] HttpRequest req,
            ILogger log, string branchName)
        {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            NewBranchRequest newBranch = JsonConvert.DeserializeObject<NewBranchRequest>(requestBody);
            if (branchName == null)
            {
                return new BadRequestObjectResult("No branch name provided to copy");
            }
            if (newBranch.BranchName == null || newBranch.RepoName == null)
            {
                return new BadRequestObjectResult("Invalid branch name or repository name");
            }
            Branch ghBranch = await _gitHubClient.Repository.Branch.Get(gitHubAccount, newBranch.RepoName, branchName);
            GitReference gitRef = ghBranch.Commit;
            string gitRefSha = gitRef.Sha;
            NewReference newRef = new NewReference("refs/heads/" + newBranch.BranchName, gitRefSha);
            Reference newRefResponse = await _gitHubClient.Git.Reference.Create(gitHubAccount, newBranch.RepoName, newRef);
            return new OkObjectResult(newRefResponse);
        }
    }
}

