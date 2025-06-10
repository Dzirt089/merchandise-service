// See https://aka.ms/new-console-template for more information

using Grpc.Net.Client;

using OzonEdu.MerchandiseService.Grpc;


var channel = GrpcChannel.ForAddress("http://localhost:5222");
var client = new MerchandiseServiceGrpc.MerchandiseServiceGrpcClient(channel);

//var responseGetAll = await client.GetAllMerchandiseITemsAsync(new GetAllMerchandiseItemsRequest());

//foreach (var item in responseGetAll.MerchItems)
//{
//	Console.WriteLine($"Item id {item.ItemId} - quantity {item.ItemQuantity}");
//}

//var responseGetAllV2 = await client.GetAllMerchandiseITemsV2Async(new Empty());
//foreach (var item in responseGetAllV2.MerchItems)
//{
//	Console.WriteLine($"Item2 id {item.ItemId} - quantity {item.ItemQuantity}");
//}

//var responseGetOne = await client.GetOneMerchaniseItemAsync(new GetOneMerchaniseItemRequest() { ItemId = 1 });
//Console.WriteLine($"Item3 id {responseGetOne.MerchItem.ItemId} - quantity {responseGetOne.MerchItem.ItemQuantity}");

Console.ReadKey();