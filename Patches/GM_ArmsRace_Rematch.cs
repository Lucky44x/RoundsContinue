using System;
using System.Collections;
using System.Runtime.CompilerServices;
using HarmonyLib;
using Photon.Pun;
using UnityEngine;
using UnityEngine.Localization;

namespace RoundsContinue;

[HarmonyPatch(typeof(GM_ArmsRace), "GameOverRematch", [typeof(int)])]
public class GM_ArmsRace_Rematch
{
    static AccessTools.FieldRef<GM_ArmsRace, LocalizedString> _localizedContinue = AccessTools.FieldRefAccess<GM_ArmsRace, LocalizedString>("m_localizedContinue");
    static AccessTools.FieldRef<GM_ArmsRace, LocalizedString> _localizedWaiting = AccessTools.FieldRefAccess<GM_ArmsRace, LocalizedString>("m_localizedWaiting");
    static AccessTools.FieldRef<GM_ArmsRace, bool> _waitingForOtherPlayer = AccessTools.FieldRefAccess<GM_ArmsRace, bool>("waitingForOtherPlayer");
    
    static bool Prefix(GM_ArmsRace __instance, int winningTeamID)
    {
        //Display Continue Message
        UIHandler.instance.DisplayScreenTextLoop(PlayerManager.instance.GetColorFromTeam(winningTeamID).winText, _localizedContinue(__instance));
        
        RoundsContinueMod.instance.DisplayYesNoLoop(RoundsContinueMod.instance.GetFirstPlayerInTeam(winningTeamID), ContinueHandle);
        MapManager.instance.LoadNextLevel(); //Same as (false, false)
        return false;   // Abort original Execution
    }

    static void ContinueHandle(PopUpHandler.YesNo yesNo)
    {
        //TODO: Networking
        if (yesNo == PopUpHandler.YesNo.Yes)
        {
            ResetMatchKeepCards();
            RoundsContinueMod.instance.DoContinue();
            return;
        }
        RoundsContinueMod.instance.DoRestart();
    }
    
    static void ResetMatchKeepCards()
    {
        //Reset Game Data
        GM_ArmsRace.instance.p1Points = 0;
        GM_ArmsRace.instance.p2Points = 0;
        GM_ArmsRace.instance.p1Rounds = 0;
        GM_ArmsRace.instance.p2Rounds = 0;
        UIHandler.instance.ShowRoundCounterSmall(0, 0, 0, 0);
        PointVisualizer.instance.ResetPoints();
    }
}