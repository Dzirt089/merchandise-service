﻿using OzonEdu.MerchandiseService.Http.Services;

class Program
{
	private static IHttpService _service = new HttpService(new HttpClient());

	static async Task Main(string[] args)
	{
		List<ItemPosition> items = await _service.GetAll(CancellationToken.None);
		ItemPosition item = await _service.GetById(2, CancellationToken.None);

		//Для визуализации результата
		foreach (var temp in items)
			await Console.Out.WriteLineAsync(temp.ToString());
		await Console.Out.WriteLineAsync(item.ToString());
		Console.ReadLine();
	}
}