using OzonEdu.MerchandiseService.Domain.AggregationModels.MerchandiseRequests.DomainEvents;
using OzonEdu.MerchandiseService.Domain.AggregationModels.SkuPresets;
using OzonEdu.MerchandiseService.Domain.Root;
using OzonEdu.MerchandiseService.Domain.Root.Exceptions;

namespace OzonEdu.MerchandiseService.Domain.AggregationModels.MerchandiseRequests
{
	public sealed class MerchandiseRequest : Entity
	{
		// Приватные поля для навигационных свойств
		private long _skuPresetId;
		private string _employeeEmail;
		private string _clothingSize;


		/// <summary>
		/// Конструктор для заполнения данными из БД при использовании Dapper или ADO.NET
		/// </summary>
		/// <param name="id"></param>
		/// <param name="skuPreset">Идентификатор набора мерча</param>
		/// <param name="employee">Информация о сотруднике</param>
		/// <param name="status">Статус заявки на выдачу мерча</param>
		/// <param name="createdAt">Дата создания запроса</param>
		/// <param name="giveOutAt">Дата выдачи мерча по запросу</param>
		private MerchandiseRequest(
			long id,
			long skuPresetId,
			string employeeEmail,
			string clothingSize,
			MerchandiseRequestStatus status,
			DateTimeOffset createdAt,
			DateTimeOffset? giveOutAt)
		{
			Id = id;
			_skuPresetId = skuPresetId;
			_employeeEmail = employeeEmail;
			_clothingSize = clothingSize;

			Status = status;
			CreatedAt = createdAt;
			GiveOutAt = giveOutAt;

			// Инициализация Value Objects
			Employee = new Employee(
				email: Email.Create(_employeeEmail),
				clothingSize: ClothingSize.Parse(_clothingSize));
		}

		/// <summary>
		/// Идентификатор набора мерча
		/// </summary>
		public SkuPreset SkuPreset { get; private set; }

		/// <summary>
		/// Информация о сотруднике
		/// </summary>
		public Employee Employee { get; private set; }

		/// <summary>
		/// Статус запроса 
		/// </summary>
		public MerchandiseRequestStatus Status { get; private set; }

		/// <summary>
		/// Дата создания запроса
		/// </summary>
		public DateTimeOffset CreatedAt { get; private set; }

		/// <summary>
		/// Дата выдачи мерча по запросу
		/// </summary>
		public DateTimeOffset? GiveOutAt { get; private set; }


		/// <summary>
		/// Создание нового запроса на выдачу мерча
		/// </summary>
		/// <param name="skuPreset"></param>
		/// <param name="employee"></param>
		/// <param name="alreadyExistedRequest"></param>
		/// <param name="createAt"></param>
		/// <returns></returns>
		public static MerchandiseRequest Create(
			SkuPreset skuPreset,
			Employee employee,
			IReadOnlyCollection<MerchandiseRequest> alreadyExistedRequest,
			DateTimeOffset createAt)
		{
			var newRequest = new MerchandiseRequest(
					id: 0,
					skuPresetId: skuPreset.Id,
					employeeEmail: employee.Email.Value,
					clothingSize: employee.ClothingSize.Name,
					status: MerchandiseRequestStatus.New,
					createdAt: createAt,
					giveOutAt: null)
			{
				SkuPreset = skuPreset
			};

			if (!newRequest.CheckAbilityToGiveOut(alreadyExistedRequest, createAt))
			{
				throw new DomainException("Merchandise is unable to gave out");
			}
			return newRequest;
		}

		/// <summary>
		/// Выдаем мерч
		/// </summary>
		/// <param name="isAvailable">Флаг от StockApi сервиса, обозначает возможность забронирования мерча на складе</param>
		/// <param name="giveOutAt">Дата предполагаемой выдачи</param>
		public MerchandiseRequestStatus GiveOut(bool isAvailable, DateTimeOffset giveOutAt)
		{
			if (Equals(Status, MerchandiseRequestStatus.New) || Equals(Status, MerchandiseRequestStatus.Processing))
			{
				if (isAvailable)
				{
					Status = MerchandiseRequestStatus.Done;
					GiveOutAt = giveOutAt;

					//Бросаем доменное событие, что выдали такой-то набор мерча определенному сотруднику
					AddDomainEvent(new MerchandiseRequestGiveOutDomainEvent
					{
						SkuPreset = SkuPreset,
						Employee = Employee,
					});
				}//Или переводим в статус в процессе
				else
				{
					Status = MerchandiseRequestStatus.Processing;
				}
				return Status;
			}
			else
			{
				throw new DomainException($"Unable to give out merchandise for request in '{Status}' status");
			}
		}

		/// <summary>
		/// Отклоняем выдачу мерча
		/// </summary>
		public void Decline()
		{
			//Проверяем, что пытаемся отклонить уже отклоненный или выполненный запрос
			if (Equals(Status, MerchandiseRequestStatus.Done) || Equals(Status, MerchandiseRequestStatus.Declined))
			{
				throw new DomainException($"Unable to decline request in '{Status}' status");
			}
			else
			{
				Status = MerchandiseRequestStatus.Declined;
				//Бросаем доменное событие, что отклонили выдачу мерча
				AddDomainEvent(new MerchandiseRequestDeclinedDomainEvent
				{
					SkuPreset = SkuPreset,
					Employee = Employee,
				});
			}
		}

		/// <summary>
		/// Проверяем возможность выдать мерч сотруднику
		/// </summary>
		/// <param name="alreadyExistedRequest">Список всех запросов выдачи мерча</param>
		/// <param name="createAt">Дата предполагаемой выдачи мерча</param>
		/// <returns></returns>
		private bool CheckAbilityToGiveOut(IReadOnlyCollection<MerchandiseRequest> alreadyExistedRequest, DateTimeOffset createAt)
		{
			//TODO: реализовать проверку на возможность выдачи мерча
			//Например, сотрудник проработал меньше срока, положенного на выдаваемый набор мерча.
			//Или повторно пытаемся что-то выдать

			return true;
		}
	}
}
