// den0bot (c) StanR 2023 - MIT License
using Newtonsoft.Json;

namespace den0bot.Modules.Osu.Types.V2
{
    public class DifficultyAttributes
    {
	    [JsonProperty("attributes")]
	    public InnerDifficultyAttributes Attributes { get; set; }
	}

    public class InnerDifficultyAttributes
    {
	    [JsonProperty("star_rating")]
	    public double StarRating { get; set; }

	    [JsonProperty("max_combo")]
	    public int MaxCombo { get; set; }

	    [JsonProperty("aim_difficulty")]
	    public double AimDifficulty { get; set; }

	    [JsonProperty("speed_difficulty")]
	    public double SpeedDifficulty { get; set; }

	    [JsonProperty("flashlight_difficulty")]
	    public double FlashlightDifficulty { get; set; }

	    [JsonProperty("slider_factor")]
	    public double SliderFactor { get; set; }

	    [JsonProperty("approach_rate")]
	    public double ApproachRate { get; set; }

	    [JsonProperty("overall_difficulty")]
	    public double OverallDifficulty { get; set; }

	    [JsonProperty("stamina_difficulty")]
	    public double StaminaStrain { get; set; }

	    [JsonProperty("rhythm_difficulty")]
	    public double RhythmStrain { get; set; }

	    [JsonProperty("colour_difficulty")]
	    public double ColourStrain { get; set; }

	    [JsonProperty("great_hit_window")]
	    public double GreatHitWindow { get; set; }
	}
}
