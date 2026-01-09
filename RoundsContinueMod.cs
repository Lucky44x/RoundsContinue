using System;
using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;

namespace RoundsContinue;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class RoundsContinueMod : BaseUnityPlugin
{
    public new static ManualLogSource Logger;
    public static RoundsContinueMod instance;

    private Harmony _harmony;
    
    private void Awake()
    {
        instance = this;
        
        // Plugin startup logic
        Logger = base.Logger;
        Logger.LogInfo($"Initializing {MyPluginInfo.PLUGIN_NAME}");
        Logger.LogInfo($"Creating harmony instance");
        _harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);
        Logger.LogInfo("Patching...");
        _harmony.PatchAll(typeof(GM_ArmsRace_Rematch));
        Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");
    }
    
    private readonly MethodInfo _displayYesNoMethod = AccessTools.Method(typeof(UIHandler), "DisplayYesNoLoop", [typeof(Player), typeof(Action<PopUpHandler.YesNo>)]);
    public void DisplayYesNoLoop(Player pickingPlayer, Action<PopUpHandler.YesNo> functionToCall) => _displayYesNoMethod.Invoke(UIHandler.instance, [pickingPlayer, functionToCall]);
    
    private readonly MethodInfo _getFirstPlayerInTeamMethod = AccessTools.Method(typeof(PlayerManager), "GetFirstPlayerInTeam", [typeof(int)]);
    public Player GetFirstPlayerInTeam(int teamID) => _getFirstPlayerInTeamMethod.Invoke(PlayerManager.instance, [teamID]) as Player;
    
    private readonly MethodInfo _doRestart = AccessTools.Method(typeof(GM_ArmsRace), "DoRestart");
    public void DoRestart() => _doRestart.Invoke(GM_ArmsRace.instance, []);
    
    private readonly MethodInfo _doContinue = AccessTools.Method(typeof(GM_ArmsRace), "DoContinue");
    public void DoContinue() => _doContinue.Invoke(GM_ArmsRace.instance, []);
}