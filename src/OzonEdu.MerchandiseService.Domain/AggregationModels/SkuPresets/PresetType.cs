﻿using OzonEdu.MerchandiseService.Domain.Root;
using OzonEdu.MerchandiseService.Domain.Root.Exceptions;

namespace OzonEdu.MerchandiseService.Domain.AggregationModels.SkuPresets
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
		public static PresetType Parse(string name) => name?.ToLower() switch
		{
			"welcome_pack" => WelcomePack,
			"conference_listener_pack" => ConferenceListenerPack,
			"conference_speaker_pack" => ConferenceSpeakerPack,
			"probation_period_ending_pack" => ProbationPeriodEndingPack,
			"veteran_pack" => VeteranPack,
			_ => throw new DomainException("Unknown preset type name")
		};

		public static PresetType Parse(int id) => id switch
		{
			1 => WelcomePack,
			2 => ConferenceListenerPack,
			3 => ConferenceSpeakerPack,
			4 => ProbationPeriodEndingPack,
			5 => VeteranPack,
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
