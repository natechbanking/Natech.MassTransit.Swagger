## What is this?
You probably have found yourself writing `MassTransit` consumers and wanting an easy way to actually test them! While the best way to do this is through thorough unit testing , we've actually developed a small tool to expose all of your consumers in a swagger UI for easy testing!

## Installation 

`dotnet tool install -g Natech.MassTransit.Swagger`

## Supported arguments
| Argument | Description | Required |
| --- | --- | --- |
| --dllPath | The path to the dll containing your consumers | Yes ✅|
| --sbConnectionString | The connection string to your azure service bus | No ❌ |
| --keyvaultUri | The uri of the keyvault containing your secrets | No ❌ |
| --keyvaultBusKey | The name of the secret containing the bus connection string | No ❌ |
| --rabbitAddress | The address of the rabbitmq server | No ❌ |
| --rabbitUsername | The username of the rabbitmq server | No ❌ |
| --rabbitPassword | The password of the rabbitmq server | No ❌ |
| --rabbitUri | The uri of the rabbitmq server | No ❌ |
| --rabbitVhost | The vhost of the rabbitmq server | No ❌ |

## Examples

#### Using with azure service bus and keyvault
`mtswagger --keyvaultUri=https://kv-test.vault.azure.net/ --keyvaultBusKey=BusConnStr --dllPath=C:\git\Service.dll`

#### Using with rabbitmq
`mtswagger --rabbitAddress=localhost --rabbitUri=amqp://localhost --rabbitUsername=guest --rabbitPassword=guest --rabbitVhost=/ --dllPath=C:\git\Service.dll`

#### Using with no keyvault and service bus
`mtswagger --sbConnectionString=Endpoint=sb://test.servicebus.windows.net --dllPath=C:\git\Service.dll`

## Update
`dotnet tool update -g Natech.MassTransit.Swagger`

## Uninstall
`dotnet tool uninstall -g Natech.MassTransit.Swagger`

## Install specific version
`dotnet tool install -g Natech.MassTransit.Swagger --version 1.0.0`