using System;
using System.Collections.Generic;
using UnityEngine;

public class MatchManager : MonoBehaviour
{
    public enum Outcome { Ongoing, P1Wins, P2Wins, Draw }

    public static MatchManager Instance { get; private set; }

    static readonly List<PlayerBase> alivePlayers = new List<PlayerBase>();

    public Outcome CurrentOutcome { get; private set; } = Outcome.Ongoing;
    public event Action<Outcome> OnMatchEnded;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        alivePlayers.Clear();
        CurrentOutcome = Outcome.Ongoing;
        Time.timeScale = 1f;
    }

    public static void RegisterPlayer(PlayerBase p)
    {
        if (p != null && !alivePlayers.Contains(p)) alivePlayers.Add(p);
    }

    public static void UnregisterPlayer(PlayerBase p)
    {
        alivePlayers.Remove(p);
        if (Instance != null) Instance.CheckEnd();
    }

    void CheckEnd()
    {
        if (CurrentOutcome != Outcome.Ongoing) return;

        if (alivePlayers.Count == 0)
        {
            EndMatch(Outcome.Draw);
        }
        else if (alivePlayers.Count == 1)
        {
            int idx = alivePlayers[0].PlayerIndex;
            EndMatch(idx == 1 ? Outcome.P1Wins : Outcome.P2Wins);
        }
    }

    void EndMatch(Outcome o)
    {
        CurrentOutcome = o;
        Time.timeScale = 0f;
        OnMatchEnded?.Invoke(o);
    }

    public static IReadOnlyList<PlayerBase> AlivePlayers => alivePlayers;
}
