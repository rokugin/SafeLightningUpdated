using HarmonyLib;
using SafeLightningUpdated.Patches;
using StardewModdingAPI;
using StardewValley;
using System.Reflection.Emit;

namespace SafeLightningUpdated {
    internal class ModEntry : Mod {

        public static ModConfig Config = new();
        public static IMonitor? SMonitor;

        public override void Entry(IModHelper helper) {
            Config = helper.ReadConfig<ModConfig>();
            SMonitor = Monitor;

            helper.Events.GameLoop.GameLaunched += OnGameLaunched;

            var harmony = new Harmony(ModManifest.UniqueID);

            harmony.Patch(
                original: AccessTools.Method(typeof(Utility), nameof(Utility.performLightningUpdate)),
                transpiler: new HarmonyMethod(typeof(UtilityPatch), nameof(UtilityPatch.PerformLightningUpdate_Transpiler))
            );
        }

        private void OnGameLaunched(object? sender, StardewModdingAPI.Events.GameLaunchedEventArgs e) {
            var configMenu = this.Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
            if (configMenu is null) return;

            configMenu.Register(
                mod: this.ModManifest,
                reset: () => Config = new ModConfig(),
                save: () => this.Helper.WriteConfig(Config)
            );

            configMenu.AddBoolOption(
                mod: this.ModManifest,
                name: () => Helper.Translation.Get("enable-mod.label"),
                tooltip: () => Helper.Translation.Get("enable-mod.tooltip"),
                getValue: () => Config.ModEnabled,
                setValue: value => Config.ModEnabled = value
            );
            configMenu.AddBoolOption(
                mod: this.ModManifest,
                name: () => Helper.Translation.Get("disable-strikes.label"),
                tooltip: () => Helper.Translation.Get("disable-strikes.tooltip"),
                getValue: () => Config.DisableStrikes,
                setValue: value => Config.DisableStrikes = value
            );
        }

    }
}
