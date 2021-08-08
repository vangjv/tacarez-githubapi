using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Octokit;
using TacarEZGithubAPI.Models;

namespace TacarEZGithubAPI
{

    public class ReposAPI
    {
        private GitHubClient _gitHubClient;

        public ReposAPI(GitHubClient gitHubClient)
        {
            _gitHubClient = gitHubClient;
        }

        [FunctionName("GetRepo")]
        public async Task<IActionResult> GetRepo(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "repo/{repoName}/{branch}")] HttpRequest req, string repoName,
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
            Branch ghBranch = await _gitHubClient.Repository.Branch.Get(gitHubAccount, repoName, branch);
            GitReference gitRef = ghBranch.Commit;
            string gitRefSha = gitRef.Sha;
            string gitRefURL = gitRef.Url;
            Console.WriteLine("GitRef Sha: " + gitRefSha);
            Console.WriteLine("GitRef URL: " + gitRefURL);
            var commits = await _gitHubClient.Repository.Commit.GetAll(gitHubAccount, repoName);
            for (int i = 0; i < commits.Count; i++)
            {
                Console.WriteLine("Commit " + i + " Label: " + commits[i].Label);
                Console.WriteLine("Commit " + i + " HtmlURL: " + commits[i].HtmlUrl);
                Console.WriteLine("Commit " + i + " CommentsUrl: " + commits[i].CommentsUrl);
                Console.WriteLine("Commit " + i + " Author.Login: " + commits[i].Author.Login);
                Console.WriteLine("Commit " + i + " Sha: " + commits[i].Sha);
            }
            return new OkObjectResult("test");
        }

        [FunctionName("CreateRepo")]
        public async Task<IActionResult> CreateRepoWithContent(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "repo")] HttpRequest req,
            ILogger log)
        {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            NewRepoRequest newRepo = JsonConvert.DeserializeObject<NewRepoRequest>(requestBody);

            NewRepository newRepository = new NewRepository(newRepo.name);
            newRepository.HasWiki = false;
            newRepository.HasIssues = false;
            newRepository.Private = false;
            newRepository.Description = newRepo.description;
            Repository createdRepo;
            try
            {
                createdRepo = await _gitHubClient.Repository.Create(newRepository);
                if (createdRepo == null)
                {
                    return new BadRequestObjectResult("Unable to create repository");
                }
            } catch (Exception e)
            {
                return new BadRequestObjectResult("Unable to create repository");
            }
            string gitHubAccount = Utility.GetEnvironmentVariable("GitHubAccount");
            RepositoryContentChangeSet repoChangeSet = await _gitHubClient.Repository.Content.CreateFile(gitHubAccount, createdRepo.Name, "data.geojson", new CreateFileRequest(newRepo.message, newRepo.content, "main", false));
            return new OkObjectResult(repoChangeSet);
        }

        [FunctionName("UpdateRepo")]
        public async Task<IActionResult> UpdateRepo(
            [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "repo/{repoName}/{branch}")] HttpRequest req, string repoName,
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

        [FunctionName("DeleteRepo")]
        public async Task<IActionResult> DeleteRepo(
        [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "repo/{repoName}")] HttpRequest req, string repoName,
            ILogger log)
        {
            if (repoName == null)
            {
                return new BadRequestObjectResult("Invalid repository name");
            }
            await _gitHubClient.Repository.Delete(Utility.GetEnvironmentVariable("GitHubAccount"), repoName);
            return new OkObjectResult("Success");
        }
    }
}
