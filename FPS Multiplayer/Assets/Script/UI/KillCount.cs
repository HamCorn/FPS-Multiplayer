using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class KillCount : MonoBehaviour
{
    public const string PlayerKillScoreProp = "_KillScore";
    public const string PlayerDeathScoreProp = "_DeathScore";
}

public static class ScoreExtensions
{
    public static void AddKillScore(this Player player, int KillScore)
    {
        int score = player.GetKillScore();
        score = score + KillScore;

        Hashtable KillCountScore = new Hashtable();
        KillCountScore[KillCount.PlayerKillScoreProp] = score;
        player.SetCustomProperties(KillCountScore);
    }
    public static void AddDeathScore(this Player player, int DeathScore)
    {
        int score = player.GetDeathScore();
        score = score + DeathScore;

        Hashtable KillCountScore = new Hashtable();
        KillCountScore[KillCount.PlayerDeathScoreProp] = score;
        player.SetCustomProperties(KillCountScore);
    }

    public static int GetKillScore(this Player player)
    {
        object killScore;
        if (player.CustomProperties.TryGetValue(KillCount.PlayerKillScoreProp, out killScore))
        {
            return (int)killScore;
        }
        return 0;
    }
    public static int GetDeathScore(this Player player)
    {
        object deathScore;
        if (player.CustomProperties.TryGetValue(KillCount.PlayerDeathScoreProp, out deathScore))
        {
            return (int)deathScore;
        }
        return 0;
    }
}