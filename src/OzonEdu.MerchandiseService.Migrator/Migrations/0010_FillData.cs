using FluentMigrator;

namespace OzonEdu.MerchandiseService.Migrator.Migrations
{
	[Migration(10)]
	public class FillData : ForwardOnlyMigration
	{
		public override void Up()
		{
			Execute.Sql(@"
                INSERT INTO skus (id, name, item_type_id, clothing_size, preset_type_id)
                VALUES 
                    (1,  'TShirtStarter XS',  1, 1, 1),
                    (2,  'TShirtStarter S',   1, 2, 1),
                    (3,  'TShirtStarter M',   1, 3, 1),
                    (4,  'TShirtStarter L',   1, 4, 1),
                    (5,  'TShirtStarter XL',  1, 5, 1),
                    (6,  'TShirtStarter XXL', 1, 6, 1),
                    (7,  'TShirtAfterProbation XS',  5, 1, 4),
                    (8,  'TShirtAfterProbation S',   5, 2, 4),
                    (9,  'TShirtAfterProbation M',   5, 3, 4),
                    (10, 'TShirtAfterProbation L',   5, 4, 4),
                    (11, 'TShirtAfterProbation XL',  5, 5, 4),
                    (12, 'TShirtAfterProbation XXL', 5, 6, 4),
                    (13, 'SweatshirtAfterProbation XS',  6, 1, 4),
                    (14, 'SweatshirtAfterProbation S',   6, 2, 4),
                    (15, 'SweatshirtAfterProbation M',   6, 3, 4),
                    (16, 'SweatshirtAfterProbation L',   6, 4, 4),
                    (17, 'SweatshirtAfterProbation XL',  6, 5, 4),
                    (18, 'SweatshirtAfterProbation XXL', 6, 6, 4),
                    (19, 'SweatshirtСonferenceSpeaker XS',  7, 1, 3),
                    (20, 'SweatshirtСonferenceSpeaker S',   7, 2, 3),
                    (21, 'SweatshirtСonferenceSpeaker M',   7, 3, 3),
                    (22, 'SweatshirtСonferenceSpeaker L',   7, 4, 3),
                    (23, 'SweatshirtСonferenceSpeaker XL',  7, 5, 3),
                    (24, 'SweatshirtСonferenceSpeaker XXL', 7, 6, 3),
                    (25, 'TShirtСonferenceListener XS',  10, 1, 2),
                    (26, 'TShirtСonferenceListener S',   10, 2, 2),
                    (27, 'TShirtСonferenceListener M',   10, 3, 2),
                    (28, 'TShirtСonferenceListener L',   10, 4, 2),
                    (29, 'TShirtСonferenceListener XL',  10, 5, 2),
                    (30, 'TShirtСonferenceListener XXL', 10, 6, 2),
                    (31, 'TShirtVeteran XS',  13, 1, 5),
                    (31, 'TShirtVeteran S',   13, 2, 5),
                    (32, 'TShirtVeteran M',   13, 3, 5),
                    (33, 'TShirtVeteran L',   13, 4, 5),
                    (34, 'TShirtVeteran XL',  13, 5, 5),
                    (35, 'TShirtVeteran XXL', 13, 6, 5),
                    (36, 'SweatshirtVeteran XS',  14, 1, 5),
                    (37, 'SweatshirtVeteran S',   14, 2, 5),
                    (38, 'SweatshirtVeteran M',   14, 3, 5),
                    (38, 'SweatshirtVeteran L',   14, 4, 5),
                    (39, 'SweatshirtVeteran XL',  14, 5, 5),
                    (40, 'SweatshirtVeteran XXL', 14, 6, 5),
                    (41, 'NotepadStarter', 2, null, 1),
                    (42, 'PenStarter', 3, null, 1),
                    (43, 'SocksStarter', 4, null, 1),
                    (44, 'NotepadСonferenceSpeaker', 8, null, 3),
                    (45, 'PenСonferenceSpeaker', 9, null, 3),
                    (46, 'NotepadСonferenceListener', 11, null, 2),
                    (47, 'PenСonferenceListener', 12, null, 2),
                    (48, 'NotepadVeteran', 15, null, 5),
                    (49, 'PenVeteran', 16, null, 5),
                    (50, 'CardHolderVeteran', 17, null, 5)
                ON CONFLICT DO NOTHING");
		}
	}
}
