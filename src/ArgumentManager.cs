using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace Natech.MassTransit.Swagger;

/// <summary>
///     This class should be cleared up and refactored ideally using a CLI Argument Parser library like CommandLineParser, Spectre.Console or something similar
/// </summary>
using System;
using System.Linq;
using System.Text.RegularExpressions;

public class ArgumentManager
{
    private readonly string[] args;
    private readonly string[] requiredArgs = { "--dllPath=" };

    public ArgumentManager(string[] args)
    {
        this.args = args;
        ValidateRequiredArgs();
    }

    public void ValidateRequiredArgs()
    {
        foreach (var requiredArg in requiredArgs)
        {
            if (!args.Any(arg => arg.StartsWith(requiredArg)))
            {
                MissingArgument(requiredArg);
            }
        }
    }

    private string? GetArgumentValue(string argName)
    {
        var argument = args.FirstOrDefault(arg => arg.StartsWith(argName));
        return argument != null ? Regex.Match(argument, $@"{argName}(.*)").Groups[1].Value : null;
    }

    private static void MissingArgument(string argName)
    {
        Console.WriteLine($"Missing {argName} argument");
        PrintUsageGuidelines();
        Environment.Exit(0);
    }

    private bool GetUseArgument(string argName)
    {
        return args.Any(arg => arg.StartsWith(argName));
    }

    public string? GetDllPath()
    {
        var dllPathValue = GetArgumentValue("--dllPath=");

        Console.WriteLine($"Using dllPath: {dllPathValue}");

        return dllPathValue;
    }

    public (bool useKeyvault, string keyvaultUri, string keyvaultBusKey) GetKeyvaultArgs()
    {
        var useKeyvault = GetUseArgument("--keyvaultUri=") && GetUseArgument("--keyvaultBusKey=");
        var keyvaultUri = useKeyvault ? GetArgumentValue("--keyvaultUri=") : null;
        var keyVaultBusKey = useKeyvault ? GetArgumentValue("--keyvaultBusKey=") : null;

        Console.WriteLine($"Using keyvault: {useKeyvault}");

        if (useKeyvault && (keyvaultUri is null || keyVaultBusKey is null))
        {
            useKeyvault = false;
        }

        if (useKeyvault)
        {
            Console.WriteLine($"Using keyvaultUri: {keyvaultUri}");
            Console.WriteLine($"Using keyvaultBusKey: {keyVaultBusKey}");
        }

        return (useKeyvault, keyvaultUri, keyVaultBusKey);
    }

    //get azure service bus connection string
    public string? GetAzureServiceBusConnectionString()
    {
        var keyvaultUri = GetArgumentValue("--sbConnectionString=");
        return keyvaultUri;
    }

    public (bool useRabbit, string rabbitAddress, string rabbitUsername, string rabbitPassword, string rabbitUri, string rabbitVhost) GetRabbitArgs()
    {
        var useRabbit = GetUseArgument("--rabbitAddress=");
        var rabbitAddress = useRabbit ? GetArgumentValue("--rabbitAddress=") : null;
        var rabbitUsername = useRabbit ? GetArgumentValue("--rabbitUsername=") : null;
        var rabbitPassword = useRabbit ? GetArgumentValue("--rabbitPassword=") : null;
        var rabbitUri = useRabbit ? GetArgumentValue("--rabbitUri=") : null;
        var rabbitVhost = useRabbit ? GetArgumentValue("--rabbitVhost=") ?? "/" : "/";

        if (useRabbit && (rabbitAddress is null || rabbitUsername is null || rabbitPassword is null || rabbitUri is null))
        {
            useRabbit = false;
        }

        if (useRabbit)
        {
            Console.WriteLine($"Using rabbitAddress: {rabbitAddress}");
            Console.WriteLine($"Using rabbitUsername: {rabbitUsername}");
            Console.WriteLine($"Using rabbitPassword: {rabbitPassword}");
            Console.WriteLine($"Using rabbitUri: {rabbitUri}");
            Console.WriteLine($"Using rabbitVhost: {rabbitVhost}");
        }

        return (useRabbit, rabbitAddress, rabbitUsername, rabbitPassword, rabbitUri, rabbitVhost);
    }

    public static void PrintUsageGuidelines()
    {
        Console.WriteLine("**************************");
        Console.WriteLine("Usage:");
        Console.WriteLine("**************************");
        Console.WriteLine("Required arguments:");
        Console.WriteLine("--dllPath=<dll path>");
        Console.WriteLine("**************************");
        Console.WriteLine("Optional arguments:");
        Console.WriteLine("--keyvaultUri=<keyvault uri>");
        Console.WriteLine("--keyvaultBusKey=<keyvault bus key>");
        Console.WriteLine("--rabbitAddress=<rabbit address>");
        Console.WriteLine("--rabbitUsername=<rabbit username>");
        Console.WriteLine("--rabbitPassword=<rabbit password>");
        Console.WriteLine("--rabbitUri=<rabbit uri>");
        Console.WriteLine("--rabbitVhost=<rabbit vhost>");
        Console.WriteLine("**************************");
    }
}
