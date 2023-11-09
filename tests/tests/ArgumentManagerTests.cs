namespace Natech.MassTransit.Swagger.Tests.tests;

using FluentAssertions;
using Xunit;

public class ArgumentManagerTests
{
    [Fact(DisplayName = "Application exits when no arguments have been provided")]
    public void AppShould_Exit_WhenArgumentsAreMissing()
    {
        var args = Array.Empty<string>();
        _ = new ArgumentManager(args);
    }

    [Theory(DisplayName = "Returns the argument when the provided path starts with whitespace")]
    [InlineData("--dllPath= ..\\DllPath", "..\\DllPath")]
    public void GetDLLPath_Should_Return(string arg, string expected)
    {
        var args = new string[]{ arg };
        var argManager = new ArgumentManager(args);

        var argValue = argManager.GetDllPath();

        argValue.Should().NotBeNull();
        argValue.Should().Be(expected);
    }

    [Theory(DisplayName = "Does not return the argument when the provided path does not start with a whitespace")]
    [InlineData("--dllPath=..\\DllPath", "")]
    public void GetDLLPath_Should_ReturnEmpty(string arg, string expected)
    {
        var args = new string[]{ arg };
        var argManager = new ArgumentManager(args);

        var argValue = argManager.GetDllPath();

        argValue.Should().NotBeNull();
        argValue.Should().Be(expected);
    }

    [Theory(DisplayName = "Returns the argument when the provided path starts with a whitespace")]
    [InlineData("-p ..\\DllPath", "..\\DllPath")]
    public void GetDLLPath_WithProjectArgument_Should_Return(string arg, string expected)
    {
        var args = new string[]{ arg };
        var argManager = new ArgumentManager(args);

        var argValue = argManager.GetDllPath();

        argValue.Should().NotBeNull();
        argValue.Should().Be(expected);
    }

    [Theory(DisplayName = "Does not return the argument when the provided path does not start with a whitespace")]
    [InlineData("-p..\\DllPath", "")]
    public void GetDLLPath_WithProjectArgument_Should_ReturnEmpty(string arg, string expected)
    {
        var args = new string[]{ arg };
        var argManager = new ArgumentManager(args);

        var argValue = argManager.GetDllPath();

        argValue.Should().NotBeNull();
        argValue.Should().Be(expected);
    }


    [Theory(DisplayName = "Returns the argument when the provided path starts with whitespace")]
    [InlineData("--sbConnectionString= Endpoint=sb://test.servicebus.windows.net/;SharedAccessKeyName=AccessKey;SharedAccessKey=xx/someKey=",
        "Endpoint=sb://test.servicebus.windows.net/;SharedAccessKeyName=AccessKey;SharedAccessKey=xx/someKey=")]
    public void GetAzureServiceBusConnectionString_Should_Return(string arg, string expected)
    {
        var args = new string[]{ arg };
        var argManager = new ArgumentManager(args);

        var argValue = argManager.GetAzureServiceBusConnectionString();

        argValue.Should().NotBeNull();
        argValue.Should().Be(expected);
    }

    [Theory(DisplayName = "Does not return the argument when the provided path does not start with a whitespace")]
    [InlineData("--sbConnectionString=Endpoint=sb://test.servicebus.windows.net/;SharedAccessKeyName=AccessKey;SharedAccessKey=xx/someKey=",
        "")]
    public void GetAzureServiceBusConnectionString_Should_ReturnEmpty(string arg, string expected)
    {
        var args = new string[]{ arg };
        var argManager = new ArgumentManager(args);

        var argValue = argManager.GetAzureServiceBusConnectionString();

        argValue.Should().NotBeNull();
        argValue.Should().Be(expected);
    }

    [Theory(DisplayName = "Returns the argument when the provided path starts with whitespace")]
    [InlineData("-c Endpoint=sb://test.servicebus.windows.net/;SharedAccessKeyName=AccessKey;SharedAccessKey=xx/someKey=",
       "Endpoint=sb://test.servicebus.windows.net/;SharedAccessKeyName=AccessKey;SharedAccessKey=xx/someKey=")]
    public void GetAzureServiceBusConnectionString_WithSbConnArgument_Should_Return(string arg, string expected)
    {
        var args = new string[]{ arg };
        var argManager = new ArgumentManager(args);

        var argValue = argManager.GetAzureServiceBusConnectionString();

        argValue.Should().NotBeNull();
        argValue.Should().Be(expected);
    }

    [Theory(DisplayName = "Does not return the argument when the provided path does not start with a whitespace")]
    [InlineData("-cEndpoint=sb://test.servicebus.windows.net/;SharedAccessKeyName=AccessKey;SharedAccessKey=xx/someKey=",
        "")]
    public void GetAzureServiceBusConnectionString_WithSbConnArgument_Should_ReturnEmpty(string arg, string expected)
    {
        var args = new string[]{ arg };
        var argManager = new ArgumentManager(args);

        var argValue = argManager.GetAzureServiceBusConnectionString();

        argValue.Should().NotBeNull();
        argValue.Should().Be(expected);
    }

    [Theory(DisplayName = "Returns the Azure Key Vault arguments")]
    [InlineData("--keyvaultUri= https://test.vault.azure.net/", "--keyvaultBusKey= Endpoint=sb://test.servicebus.windows.net/;SharedAccessKeyName=AccessKey;SharedAccessKey=xx/someKey=",
        "https://test.vault.azure.net/", "Endpoint=sb://test.servicebus.windows.net/;SharedAccessKeyName=AccessKey;SharedAccessKey=xx/someKey=")]
    public void GetKeyvaultArgs_Should_Return(string arg1, string arg2, string expectedKeyVaultUri, string expectedKeyVaultBusKey)
    {
        var args = new string[]{ arg1, arg2 };
        var argManager = new ArgumentManager(args);

        var argValue = argManager.GetKeyvaultArgs();

        argValue.Should().NotBeNull();
        argValue.useKeyvault.Should().BeTrue();
        argValue.keyvaultUri.Should().Be(expectedKeyVaultUri);
        argValue.keyvaultBusKey.Should().Be(expectedKeyVaultBusKey);
    }

    [Theory]
    [InlineData("-k https://test.vault.azure.net/", "--keyvaultBusKey= Endpoint=sb://test.servicebus.windows.net/;SharedAccessKeyName=AccessKey;SharedAccessKey=xx/someKey=",
       "https://test.vault.azure.net/", "Endpoint=sb://test.servicebus.windows.net/;SharedAccessKeyName=AccessKey;SharedAccessKey=xx/someKey=")]
    public void GetKeyvaultArgs_WithKeyVaultArg_ShouldReturn(string arg1, string arg2, string expectedKeyVaultUri, string expectedKeyVaultBusKey)
    {
        var args = new string[]{ arg1, arg2 };
        var argManager = new ArgumentManager(args);

        var argValue = argManager.GetKeyvaultArgs();

        argValue.Should().NotBeNull();
        argValue.useKeyvault.Should().BeTrue();
        argValue.keyvaultUri.Should().Be(expectedKeyVaultUri);
        argValue.keyvaultBusKey.Should().Be(expectedKeyVaultBusKey);
    }


    [Theory(DisplayName = "Returns the RabbitMQ arguments")]
    [InlineData("--rabbitMqUsername= guest", "--rabbitMqPassword= guest", "--rabbitMqUri= https://localhost", "guest", "guest", "https://localhost")]
    public void GetRabbitArgs_Should_Return(string arg1,string  arg2, string arg3, 
        string expectedRmqUsername, string expectedRmqPassword, string expectedRmqUri)
    {
        var args = new string[]{ arg1, arg2, arg3 };
        var argManager = new ArgumentManager(args);

        var argValue = argManager.GetRabbitArgs();

        argValue.Should().NotBeNull();
        argValue.rabbitMqUsername.Should().Be(expectedRmqUsername);
        argValue.rabbitMqPassword.Should().Be(expectedRmqPassword);
        argValue.rabbitMqUri.Should().Be(expectedRmqUri);
        argValue.rabbitMqVhost.Should().Be("/");
    }
}