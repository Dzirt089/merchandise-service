using FluentMigrator;

namespace OzonEdu.MerchandiseService.Migrator.Migrations
{
    [Migration(14)]
    public class SeedSkuPresets : ForwardOnlyMigration
    {
        public override void Up()
        {
            Execute.Sql(@"
DO $$
BEGIN
	IF EXISTS (SELECT 1 FROM sku_presets) THEN
		RETURN;
	END IF;

	WITH sized_presets AS (
		SELECT DISTINCT preset_type_id, clothing_size
		FROM skus
		WHERE clothing_size IS NOT NULL
	),
	inserted_presets AS (
		INSERT INTO sku_presets (preset_type_id)
		SELECT preset_type_id
		FROM sized_presets
		ORDER BY preset_type_id, clothing_size
		RETURNING id, preset_type_id
	),
	numbered_presets AS (
		SELECT
			id,
			preset_type_id,
			ROW_NUMBER() OVER (PARTITION BY preset_type_id ORDER BY id) AS rn
		FROM inserted_presets
	),
	numbered_sizes AS (
		SELECT
			preset_type_id,
			clothing_size,
			ROW_NUMBER() OVER (PARTITION BY preset_type_id ORDER BY clothing_size) AS rn
		FROM sized_presets
	)
	INSERT INTO sku_preset_skus (sku_preset_id, sku_id)
	SELECT
		numbered_presets.id,
		skus.id
	FROM numbered_presets
	INNER JOIN numbered_sizes
		ON numbered_sizes.preset_type_id = numbered_presets.preset_type_id
		AND numbered_sizes.rn = numbered_presets.rn
	INNER JOIN skus
		ON skus.preset_type_id = numbered_sizes.preset_type_id
		AND (skus.clothing_size = numbered_sizes.clothing_size OR skus.clothing_size IS NULL);
END $$;");
        }
    }
}
