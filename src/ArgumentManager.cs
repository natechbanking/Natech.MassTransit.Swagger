using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace Natech.MassTransit.Swagger;

/// <summary>
///     This class should be cleared up and refactored ideally using a CLI Argument Parser library like CommandLineParser, Spectre.Console or something similar
/// </summary>
public class ArgumentManager
{
    private readonly string[] args;

    //array with all the required arguments
    private readonly string[] requiredArgs = ["--project=", "--dllPath="];

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
                PrintUsageGuideliness();
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

    public (bool useKeyvault, string keyvaultUri, string keyvaultBusKey) GetKeyvaultArgs()
    {
        var keyvaultUriArg = args.FirstOrDefault(arg => arg.StartsWith("--keyvaultUri="));
        var keyvaultBusKey = args.FirstOrDefault(arg => arg.StartsWith("--keyvaultBusKey="));
        var useKeyvault = false;
        useKeyvault = (keyvaultUriArg != null && keyvaultBusKey != null);
        var keyvaultUri = (useKeyvault) ? Regex.Match(keyvaultUriArg, @"--keyvaultUri=(.*)").Groups[1].Value : null;
        var keyVaultBusKey = (useKeyvault) ? Regex.Match(keyvaultBusKey, @"--keyvaultBusKey=(.*)").Groups[1].Value : null;

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

    public (bool useRabbit, string rabbitAddress, string rabbitUsername, string rabbitPassword, string rabbitUri, string rabbitVhost) GetRabbitArgs()
    {
        var rabbitAddressArg = args.FirstOrDefault(arg => arg.StartsWith("--rabbitAddress="));
        var rabbitUsernameArg = args.FirstOrDefault(arg => arg.StartsWith("--rabbitUsername="));
        var rabbitPasswordArg = args.FirstOrDefault(arg => arg.StartsWith("--rabbitPassword="));
        var rabbitUriArg = args.FirstOrDefault(arg => arg.StartsWith("--rabbitUri="));
        var rabbitVhostArg = args.FirstOrDefault(arg => arg.StartsWith("--rabbitVhost="));
        var useRabbit = rabbitAddressArg is not null;
        var rabbitAddress = (useRabbit) ? Regex.Match(rabbitAddressArg, @"--rabbitAddress=(.*)").Groups[1].Value : null;
        var rabbitUsername = (useRabbit) ? Regex.Match(rabbitUsernameArg, @"--rabbitUsername=(.*)").Groups[1].Value : null;
        var rabbitPassword = (useRabbit) ? Regex.Match(rabbitPasswordArg, @"--rabbitPassword=(.*)").Groups[1].Value : null;
        var rabbitUri = (useRabbit) ? Regex.Match(rabbitUriArg, @"--rabbitPort=(.*)").Groups[1].Value : null;
        var rabbitVhost = (useRabbit) ? Regex.Match(rabbitVhostArg, @"--rabbitVhost=(.*)").Groups[1].Value : "/"; //default vhost is /

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

    public static void PrintUsageGuideliness()
    {
        Console.WriteLine("**************************");
        Console.WriteLine("Usage:");
        Console.WriteLine("**************************");
        Console.WriteLine("Required arguments:");
        Console.WriteLine("--project=<project path>");
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