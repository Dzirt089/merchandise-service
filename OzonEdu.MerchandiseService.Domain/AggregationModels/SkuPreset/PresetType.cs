using OzonEdu.MerchandiseService.Domain.Root;
using OzonEdu.MerchandiseService.Domain.Root.Exceptions;

namespace OzonEdu.MerchandiseService.Domain.AggregationModels.SkuPreset
{

	/// <summary>
	/// Наборы мерча, выдаваемые сотруднику
	/// </summary>
	public class PresetType : Enumeration
	{
		/// <summary>
		/// Мерч новому сотруднику
		/// </summary>
		public static PresetType WelcomePack = new(1, "Welcome_pack");

		/// <summary>
		/// Мерч для слушателя конференции
		/// </summary>
		public static PresetType ConferenceListenerPack = new(2, "Conference_listener_pack");

		/// <summary>
		/// Мерч для спикера конференции
		/// </summary>
		public static PresetType ConferenceSpeakerPack = new(3, "Conference_speaker_pack");

		/// <summary>
		/// Мерч, выдаваемый после завершения испытательного срока
		/// </summary>
		public static PresetType ProbationPeriodEndingPack = new(4, "Probation_period_ending_pack");

		/// <summary>
		/// Мерч, выдаваемый сотруднику долго проработавшему в компании
		/// </summary>
		public static PresetType VeteranPack = new(5, "Veteran_pack");


		/// <summary>
		/// Преобразование строки в тип пресета
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		/// <exception cref="DomainException"></exception>
		public static PresetType Parse(string name) => name?.ToUpper() switch
		{
			"Welcome_pack" => WelcomePack,
			"Conference_listener_pack" => ConferenceListenerPack,
			"Conference_speaker_pack" => ConferenceSpeakerPack,
			"Probation_period_ending_pack" => ProbationPeriodEndingPack,
			"Veteran_pack" => VeteranPack,
			_ => throw new DomainException("Unknown preset type name")
		};


		/// <summary>
		/// Конструктор
		/// </summary>
		/// <param name="id"></param>
		/// <param name="name"></param>
		public PresetType(int id, string name) : base(id, name) { }
	}
}
