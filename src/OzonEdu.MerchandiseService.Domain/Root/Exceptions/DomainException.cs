namespace OzonEdu.MerchandiseService.Domain.Root.Exceptions
{
	/// <summary>
	/// Общее исключение доменной модели
	/// TODO: лучше использовать определенные исключения для конкретных случаев
	/// </summary>
	[Serializable]
	public class DomainException : Exception
	{
		public DomainException()
		{
		}
		public DomainException(string message)
			: base(message)
		{
		}
		public DomainException(string message, Exception innerException)
			: base(message, innerException)
		{
		}
	}
}
