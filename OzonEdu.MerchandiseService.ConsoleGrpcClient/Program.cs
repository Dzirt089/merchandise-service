// See https://aka.ms/new-console-template for more information

using Grpc.Net.Client;
using OzonEdu.MerchandiseService.Grpc;


var channel = GrpcChannel.ForAddress("http://localhost:5222");
var client = new MerchandiseServiceGrpc.MerchandiseServiceGrpcClient(channel);

var response = await client.GetAllMerchandiseITemsAsync(new GetAllMerchandiseItemsRequest());
//TODO: Если включить логгирование в HostBuilderExtensions, то всё валится. Разобраться. Так как в стокАпи всё работает.
foreach (var item in response.MerchItems)
{
    Console.WriteLine($"Item id {item.ItemId} - quantity {item.ItemQuantity}");
}
Console.ReadKey();