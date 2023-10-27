using System;
using System.Collections.Generic;
using System.Linq;

namespace Natech.MassTransit.Swagger
{
    public class ArgumentManager
    {
        private readonly string[] args;
        private readonly Dictionary<string, string> argumentMap;

        public ArgumentManager(string[] args)
        {
            this.args = args;
            argumentMap = ParseArguments();
        }

        public string GetArgumentValue(string argumentName)
        {
            if (argumentMap.TryGetValue(argumentName, out var value))
            {
                return value;
            }
            return null;
        }

        private Dictionary<string, string> ParseArguments()
        {
            var argDictionary = new Dictionary<string, string>();

            foreach (var arg in args)
            {
                var parts = arg.Split('=');
                if (parts.Length == 2)
                {
                    argDictionary[parts[0]] = parts[1];
                }
            }

            return argDictionary;
        }
    }
}
