using MediatR;

namespace OzonEdu.MerchandiseService.Application.Commands.ProcessEmployeeNotification
{
    public sealed record ProcessEmployeeNotificationCommand : IRequest<bool>
    {
        public string Email { get; init; } = string.Empty;

        public string ClothingSize { get; init; } = string.Empty;

        public string Type { get; init; } = string.Empty;
    }
}
