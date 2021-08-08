using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Octokit;
using TacarEZGithubAPI;

[assembly: FunctionsStartup(typeof(Startup))]
namespace TacarEZGithubAPI
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            string cosmosDbConnString = System.Environment.GetEnvironmentVariable("CosmosDbConnString", EnvironmentVariableTarget.Process);
            builder.Services.AddSingleton((s) =>
            {
                GitHubClient gitHubclient = new GitHubClient(new ProductHeaderValue("tacarez"));
                Credentials tokenAuth = new Credentials(Utility.GetEnvironmentVariable("GithubPAT")); 
                gitHubclient.Credentials = tokenAuth;

                return gitHubclient;
            });
        }
    }
}
