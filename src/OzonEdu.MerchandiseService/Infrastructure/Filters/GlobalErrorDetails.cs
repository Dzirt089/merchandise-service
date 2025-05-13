namespace OzonEdu.MerchandiseService.Infrastructure.Filters
{
	internal record GlobalErrorDetails
	{
		public string? ExceptionType { get; }
		public string Message { get; }
		public string? StackTrace { get; }
		public int Status { get; }

		public GlobalErrorDetails(string? exceptionType, string message, string? stackTrace, int status)
		{
			ExceptionType = exceptionType;
			Message = message;
			StackTrace = stackTrace;
			Status = status;
		}
	}
}
