using HarmonyLib;
using SafeLightningUpdated.Patches;
using StardewModdingAPI;
using StardewValley;

namespace SafeLightningUpdated;

internal class ModEntry : Mod {

    public static ModConfig Config = new();
    public static IMonitor? SMonitor;

    public override void Entry(IModHelper helper) {
        Config = helper.ReadConfig<ModConfig>();
        SMonitor = Monitor;

        helper.Events.GameLoop.GameLaunched += OnGameLaunched;

        var harmony = new Harmony(ModManifest.UniqueID);

        harmony.Patch(original: AccessTools.Method(typeof(Utility), nameof(Utility.performLightningUpdate)),
            prefix: new HarmonyMethod(typeof(UtilityPatch), nameof(UtilityPatch.PerformLightningUpdate_Prefix)));
    }

    private void OnGameLaunched(object? sender, StardewModdingAPI.Events.GameLaunchedEventArgs e) {
        var configMenu = this.Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
        if (configMenu is null) return;

        configMenu.Register(ModManifest, () => Config = new ModConfig(), () => this.Helper.WriteConfig(Config));

        configMenu.AddBoolOption(ModManifest, () => Config.DisableStrikes, value => Config.DisableStrikes = value,
            () => Helper.Translation.Get("disable-strikes.label"), () => Helper.Translation.Get("disable-strikes.tooltip"));
    }

}