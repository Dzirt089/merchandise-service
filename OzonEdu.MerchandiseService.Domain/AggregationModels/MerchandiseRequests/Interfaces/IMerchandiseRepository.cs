namespace OzonEdu.MerchandiseService.Domain.AggregationModels.MerchandiseRequests.Interfaces
{
	public interface IMerchandiseRepository
	{
		/// <summary>
		/// Создание заявки на выдачу мерча
		/// </summary>
		/// <param name="merchandiseRequest"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		Task<int> CreateAsync(MerchandiseRequest merchandiseRequest, CancellationToken cancellationToken);

		/// <summary>
		/// Обновление заявки на выдачу мерча
		/// </summary>
		/// <param name="merchandiseRequest"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		Task UpdateAsync(MerchandiseRequest merchandiseRequest, CancellationToken cancellationToken);

		/// <summary>
		/// Получение заявки на выдачу мерча по идентификатору
		/// </summary>
		/// <param name="id"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		Task<MerchandiseRequest> GetByIdAsync(long id, CancellationToken cancellationToken);

		/// <summary>
		/// Получение заявки на выдачу мерча по Email сотрудника
		/// </summary>
		/// <param name="email"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		Task<IReadOnlyCollection<MerchandiseRequest>> GetByEmployeeEmailAsync(Email email, CancellationToken cancellationToken);

		/// <summary>
		/// Получение всех заявок на выдачу мерча со статусом "В обработке"
		/// </summary>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		Task<IReadOnlyCollection<MerchandiseRequest>> GetAllProcessingRequestsAsync(CancellationToken cancellationToken);
	}
}
