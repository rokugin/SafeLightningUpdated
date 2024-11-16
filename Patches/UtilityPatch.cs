using StardewValley.Extensions;
using StardewValley.TerrainFeatures;
using StardewValley;
using Microsoft.Xna.Framework;
using SObject = StardewValley.Object;

namespace SafeLightningUpdated.Patches;

static class UtilityPatch {

    static bool ShouldStrike() {
        return !ModEntry.Config.DisableStrikes;
    }

    public static bool PerformLightningUpdate_Prefix(int time_of_day) {
        try {
            Random random = Utility.CreateRandom(Game1.uniqueIDForThisGame, Game1.stats.DaysPlayed, time_of_day);
            if (random.NextDouble() < 0.125 + Game1.player.team.AverageDailyLuck() + Game1.player.team.AverageLuckLevel() / 100.0) {
                Farm.LightningStrikeEvent lightningEvent = new Farm.LightningStrikeEvent();
                lightningEvent.bigFlash = true;
                Farm farm = Game1.getFarm();
                List<Vector2> lightningRods = new List<Vector2>();
                foreach (KeyValuePair<Vector2, SObject> v in farm.objects.Pairs) {
                    if (v.Value.QualifiedItemId == "(BC)9") {
                        lightningRods.Add(v.Key);
                    }
                }
                if (lightningRods.Count > 0) {
                    for (int i = 0; i < 2; i++) {
                        Vector2 v = random.ChooseFrom(lightningRods);
                        if (farm.objects[v].heldObject.Value == null) {
                            farm.objects[v].heldObject.Value = ItemRegistry.Create<SObject>("(O)787");
                            farm.objects[v].minutesUntilReady.Value = Utility.CalculateMinutesUntilMorning(Game1.timeOfDay);
                            farm.objects[v].shakeTimer = 1000;
                            lightningEvent.createBolt = true;
                            lightningEvent.boltPosition = v * 64f + new Vector2(32f, 0f);
                            if (ShouldStrike()) farm.lightningStrikeEvent.Fire(lightningEvent);
                            return false;
                        }
                    }
                }
                if (random.NextDouble() < 0.25 - Game1.player.team.AverageDailyLuck() - Game1.player.team.AverageLuckLevel() / 100.0) {
                    try {
                        if (Utility.TryGetRandom(farm.terrainFeatures, out var tile, out var feature)) {
                            if (feature is FruitTree fruitTree) {
                                fruitTree.shake(tile, doEvenIfStillShaking: true);
                                lightningEvent.createBolt = true;
                                lightningEvent.boltPosition = tile * 64f + new Vector2(32f, -128f);
                            } else {
                                if (feature is HoeDirt hoeDirt) {
                                    Crop crop = hoeDirt.crop;
                                    bool num = crop != null && !crop.dead.Value;
                                    if (feature.performToolAction(null, 0, tile)) {
                                        lightningEvent.createBolt = true;
                                        lightningEvent.boltPosition = tile * 64f + new Vector2(32f, -128f);
                                    }
                                    if (num && crop.dead.Value) {
                                        lightningEvent.createBolt = true;
                                        lightningEvent.boltPosition = tile * 64f + new Vector2(32f, 0f);
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception) {
                    }
                }
                if (ShouldStrike()) farm.lightningStrikeEvent.Fire(lightningEvent);
            } else if (random.NextDouble() < 0.1) {
                Farm.LightningStrikeEvent lightningEvent = new Farm.LightningStrikeEvent();
                lightningEvent.smallFlash = true;
                Farm farm = Game1.getFarm();
                if (ShouldStrike()) farm.lightningStrikeEvent.Fire(lightningEvent);
            }
            return false;
        } catch (Exception) {
            return true;
        }
    }

}