using BepInEx;
using BepInEx.Hacknet;
using Hacknet;
using HarmonyLib;
using Pathfinder.Executable;
using Microsoft.Xna.Framework;
using System.Reflection;

namespace HacknetPluginTemplate;

[BepInPlugin(ModGUID, ModName, ModVer)]
public class HacknetPluginTemplate : HacknetPlugin
{
    public const string ModGUID = "com.test.test";
    public const string ModName = "wallpaper";
    public const string ModVer = "1.0.0";

    public override bool Load()
    {
        ExecutableManager.RegisterExecutable<VideoWallpaperExe>("#VW#");

        // 应用 Harmony 补丁
        Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), ModGUID);

        return true;
    }

    [HarmonyPatch(typeof(OS))]
    [HarmonyPatch("Update")]
    static class OSUpdatePatch
    {
        static void Postfix(OS __instance, GameTime gameTime)
        {
            // 从 GameTime 获取经过的时间
            float t = (float)gameTime.ElapsedGameTime.TotalSeconds;

            // 确保视频壁纸模块被更新
            if (VideoWallpaperModule.Instance != null)
            {
                VideoWallpaperModule.Instance.ManualUpdate(t);
            }
        }
    }
}
