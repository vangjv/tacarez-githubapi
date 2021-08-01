using System;
using System.Collections.Generic;
using System.Text;

namespace TacarEZGithubAPI
{
    public class Utility
    {
        public static string GetEnvironmentVariable(string key)
        {
            return System.Environment.GetEnvironmentVariable(key, EnvironmentVariableTarget.Process);
        }

        public static string JsonToBase64(string jsonString)
        {
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(jsonString));
        }
    }
}
