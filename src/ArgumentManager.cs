using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Natech.MassTransit.Swagger
{
    public class ArgumentManager
    {
        private readonly string[] args;
        //array with all the required arguments
        private readonly string[] requiredArgs = new string[] { "--project=", "--dllPath=" };

        //array with all the optional arguments
        private readonly string[] optionalArgs = new string[] { "--useKeyvault", "--keyvaultUri=", "--keyvaultBusKey=" };

        public ArgumentManager(string[] args)
        {
            this.args = args;
            ValidateRequiredArgs();
        }

        public void ValidateRequiredArgs()
        {
            //check if all the required arguments are present
            foreach (var requiredArg in requiredArgs)
            {
                if (!args.Any(arg => arg.StartsWith(requiredArg)))
                {
                    Console.WriteLine($"Missing {requiredArg} argument");
                    Environment.Exit(0);
                }
            }
        }

        public (string projectArgValue, string dllPathValue) GetProjectAndDllPath()
        {
            var projectArg = args.FirstOrDefault(arg => arg.StartsWith("--project="));
            var dllPath = args.FirstOrDefault(arg => arg.StartsWith("--dllPath="));
            var projectArgValue = Regex.Match(projectArg, @"--project=(.*)").Groups[1].Value;
            var dllPathValue = Regex.Match(dllPath, @"--dllPath=(.*)").Groups[1].Value;

            Console.WriteLine($"Using project: {projectArgValue}");
            Console.WriteLine($"Using dllPath: {dllPathValue}");

            return (projectArgValue, dllPathValue);
        }
    }

}
