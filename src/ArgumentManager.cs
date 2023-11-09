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
    private static readonly string DllPathArg = "--dllPath=";
    private static readonly string KeyvaultUriArg = "--keyvaultUri=";
    private static readonly string KeyvaultBusKeyArg = "--keyvaultBusKey=";
    private static readonly string SbConnectionStringArg = "--sbConnectionString=";
    private static readonly string RabbitMqUsernameArg = "--rabbitMqUsername=";
    private static readonly string RabbitMqPasswordArg = "--rabbitMqPassword=";
    private static readonly string RabbitMqUriArg = "--rabbitMqUri=";
    private static readonly string RabbitMqVhostArg = "--rabbitMqVhost=";

    private const string UsageGuidelines = @"**************************
Usage:
**************************
Required arguments:
{0} or -p <dll path>
**************************
Optional arguments:
{1} or -k <keyvault uri>
{2} <keyvault bus key>
{3} or -c <sb connection string>
{4} <rabbitMq username>
{5} <rabbitMq password>
{6} <rabbitMq uri>
{7} <rabbitMq vhost>
**************************";

    /// <summary>
    /// Initializes a new instance of the ArgumentManager class.
    /// </summary>
    /// <param name="args">The command-line arguments.</param>
    public ArgumentManager(string[] args)
    {
        this.args = args;
        ValidateRequiredArgs();
    }

    /// <summary>
    /// Validates the required arguments.
    /// </summary>
    public void ValidateRequiredArgs()
    {
        if (!args.Any(arg => arg.StartsWith(DllPathArg) || arg.StartsWith("-p")))
        {
            MissingArgument(DllPathArg);
        }
    }

    public string? GetDllPath()
    {
        return GetArgumentValue(DllPathArg) ?? GetArgumentValue("-p");
    }

    public (bool useKeyvault, string keyvaultUri, string keyvaultBusKey) GetKeyvaultArgs()
    {
        var useKeyvault = args.Any(arg => arg.StartsWith(KeyvaultUriArg) || arg.StartsWith("-k"));
        var keyvaultUri = useKeyvault ? GetArgumentValue(KeyvaultUriArg) ?? GetArgumentValue("-k") : null;
        var keyvaultBusKey = useKeyvault ? GetArgumentValue(KeyvaultBusKeyArg) : null;
        return (useKeyvault, keyvaultUri, keyvaultBusKey);
    }

    public string? GetAzureServiceBusConnectionString()
    {
        return GetArgumentValue(SbConnectionStringArg) ?? GetArgumentValue("-c");
    }

    public (bool useRabbitMq, string rabbitMqUsername, string rabbitMqPassword, string rabbitMqUri, string rabbitMqVhost) GetRabbitArgs()
    {
        var useRabbitMq = args.Any(arg => arg.StartsWith(RabbitMqUsernameArg) || arg.StartsWith(RabbitMqPasswordArg) || arg.StartsWith(RabbitMqUriArg) || arg.StartsWith("-r"));
        var rabbitMqUsername = GetArgumentValue(RabbitMqUsernameArg);
        var rabbitMqPassword = GetArgumentValue(RabbitMqPasswordArg);
        var rabbitMqUri = GetArgumentValue(RabbitMqUriArg) ?? GetArgumentValue("-r");
        var rabbitMqVhost = GetArgumentValue(RabbitMqVhostArg) ?? "/";
        return (useRabbitMq, rabbitMqUsername, rabbitMqPassword, rabbitMqUri, rabbitMqVhost);
    }

    /// <summary>
    /// Gets the value of a command-line argument.
    /// </summary>
    /// <param name="argName">The name of the argument to retrieve.</param>
    /// <returns>The value of the argument or null if not found.</returns>
    private string? GetArgumentValue(string argName)
    {
        var argument = args.FirstOrDefault(arg => arg.StartsWith(argName) || (arg.StartsWith("-" + argName[2]) && arg[3] == ' '));
        return argument != null ? Regex.Match(argument, $@"{argName}(?:=|\s+)(.*)").Groups[1].Value : null;
    }

    /// <summary>
    /// Handles missing arguments by displaying an error message and exiting the application.
    /// </summary>
    /// <param name="argName">The name of the missing argument.</param>
    private static void MissingArgument(string argName)
    {
        Console.WriteLine($"Missing {argName} argument");
        PrintUsageGuidelines();
    }

    // ... other methods ...

    /// <summary>
    /// Displays usage guidelines for the command-line arguments.
    /// </summary>
    public static void PrintUsageGuidelines()
    {
        Console.WriteLine(string.Format(UsageGuidelines, DllPathArg, KeyvaultUriArg, KeyvaultBusKeyArg, SbConnectionStringArg, RabbitMqUsernameArg, RabbitMqPasswordArg, RabbitMqUriArg, RabbitMqVhostArg));
    }
}