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
    static AccessTools.FieldRef<GM_ArmsRace, LocalizedString> _localizedRematch = AccessTools.FieldRefAccess<GM_ArmsRace, LocalizedString>("m_localizedRematch");
    static AccessTools.FieldRef<GM_ArmsRace, LocalizedString> _localizedWaiting = AccessTools.FieldRefAccess<GM_ArmsRace, LocalizedString>("m_localizedWaiting");
    static AccessTools.FieldRef<GM_ArmsRace, bool> _waitingForOtherPlayer = AccessTools.FieldRefAccess<GM_ArmsRace, bool>("waitingForOtherPlayer");
    static AccessTools.FieldRef<GM_ArmsRace, PhotonView> _view = AccessTools.FieldRefAccess<GM_ArmsRace, PhotonView>("view");
    
    static bool Prefix(GM_ArmsRace __instance, int winningTeamID)
    {
        //Display Continue Message
        DisplayTextTeamColor(winningTeamID, _localizedContinue(__instance));
        
        RoundsContinueMod.instance.DisplayYesNoLoop(RoundsContinueMod.instance.GetFirstPlayerInTeam(winningTeamID), ContinueHandle);
        MapManager.instance.LoadNextLevel(); //Same as (false, false)
        return false;   // Abort original Execution
    }

    static void DisplayTextTeamColor(int winningTeam, LocalizedString text)
    {
        var colorTeam = winningTeam;
        if (!PhotonNetwork.OfflineMode)
        {
            if (PhotonNetwork.IsMasterClient) colorTeam = 0;
            else colorTeam = 1;
        }

        Color textColor = PlayerManager.instance.GetColorFromTeam(colorTeam).winText;
        UIHandler.instance.DisplayScreenTextLoop(textColor, text);
    }
    
    static void ContinueHandle(PopUpHandler.YesNo yesNo)
    {
        if (yesNo == PopUpHandler.YesNo.Yes)
        {
            GM_ArmsRace.instance.StartCoroutine(IDoContinue());
            return;
        }
        RoundsContinueMod.instance.DoRestart();
    }

    static IEnumerator IDoContinue()
    {
        if (!PhotonNetwork.OfflineMode)
        {
            //Wait for second Player
            _view(GM_ArmsRace.instance).RPC("RPCA_PlayAgain", RpcTarget.Others);
            UIHandler.instance.DisplayScreenTextLoop(_localizedWaiting(GM_ArmsRace.instance));
            float c = 0.0f;
            while (_waitingForOtherPlayer(GM_ArmsRace.instance))
            {
                c += Time.deltaTime;
                if (c > 10f)
                {
                    //10 seconds wait... TimeOut -> Return to menu
                    RoundsContinueMod.instance.DoRestart();
                    yield break;
                }
                yield return null;
            }
        }
        yield return null;
        UIHandler.instance.StopScreenTextLoop();
        _waitingForOtherPlayer(GM_ArmsRace.instance) = true;
        ResetMatchKeepCards();
        RoundsContinueMod.instance.DoContinue();
        GameManager.instance.battleOngoing = false;
        yield return new WaitForSecondsRealtime(1f);
        GameManager.instance.battleOngoing = true;
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