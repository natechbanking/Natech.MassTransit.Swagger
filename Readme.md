![Nuget](https://img.shields.io/nuget/dt/Natech.MassTransit.Swagger)


## What is this?

You've probably found yourself writing `MassTransit` consumers and wanting an easy way to test them. While the best practice is thorough unit testing, we've developed a small tool to expose all your consumers in a Swagger UI for easy testing!

## Installation

To install, run:

```shell
dotnet tool install -g Natech.MassTransit.Swagger
```

If you face issues authenticating with the NuGet source, you can try the following command:

```shell
iex "& { $(irm https://aka.ms/install-artifacts-credprovider.ps1) }" | dotnet tool install -g Natech.MassTransit.Swagger
```

## Supported arguments

| Argument                  | Description                                                 | Required |
| ------------------------- | ----------------------------------------------------------- | -------- |
| --dllPath (-p)            | The path to the DLL containing your consumers               | Yes ✅   |
| --sbConnectionString (-c) | The connection string to your Azure Service Bus             | No ❌    |
| --keyvaultUri (-k)        | The URI of the KeyVault containing your secrets             | No ❌    |
| --keyvaultBusKey          | The name of the secret containing the bus connection string | No ❌    |
| --rabbitUsername          | The username of the RabbitMQ server                         | No ❌    |
| --rabbitPassword          | The password of the RabbitMQ server                         | No ❌    |
| --rabbitUri (-r)          | The URI of the RabbitMQ server                              | No ❌    |
| --rabbitVhost             | The vhost of the RabbitMQ server                            | No ❌    |

## Examples

#### Using with Azure Service Bus and KeyVault

```shell
mtswagger --keyvaultUri=https://kv-test.vault.azure.net/ --keyvaultBusKey=BusConnStr --dllPath=C:\git\Service.dll
```

#### Using with RabbitMQ

```shell
mtswagger --rabbitUri=amqp://localhost --rabbitUsername=guest --rabbitPassword=guest --rabbitVhost=/ --dllPath=C:\git\Service.dll
```

#### Using with no KeyVault and Azure Service Bus

```shell
mtswagger --sbConnectionString=Endpoint=sb://test.servicebus.windows.net --dllPath=C:\git\Service.dll
```

## Update

To update the tool, run:

```shell
dotnet tool update -g Natech.MassTransit.Swagger
```

## Uninstall

To uninstall, run:

```shell
dotnet tool uninstall -g Natech.MassTransit.Swagger
```

## Install a Specific Version

To install a specific version, use the `--version` flag:

```shell
dotnet tool install -g Natech.MassTransit.Swagger --version 1.0.0
```
