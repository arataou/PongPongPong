using System;
using System.Collections.Generic;
using UnityEngine;

public class MatchManager : MonoBehaviour
{
    public enum Outcome { Ongoing, Win, Draw }

    public static MatchManager Instance { get; private set; }

    static readonly List<PlayerBase> alivePlayers = new List<PlayerBase>();

    public Outcome CurrentOutcome    { get; private set; } = Outcome.Ongoing;
    public int     WinningPlayerIndex { get; private set; }
    public event Action<Outcome> OnMatchEnded;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        alivePlayers.Clear();
        CurrentOutcome = Outcome.Ongoing;
        WinningPlayerIndex = 0;
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
            BeginEnd(Outcome.Draw, 0);
        else if (alivePlayers.Count == 1)
            BeginEnd(Outcome.Win, alivePlayers[0].PlayerIndex);
    }

    void BeginEnd(Outcome o, int winnerIdx)
    {
        CurrentOutcome     = o;
        WinningPlayerIndex = winnerIdx;

        if (GameFeel.Instance != null)
            GameFeel.Instance.PlayMatchEnd(FinalizeEnd);
        else
            FinalizeEnd();
    }

    void FinalizeEnd()
    {
        Time.timeScale = 0f;
        OnMatchEnded?.Invoke(CurrentOutcome);
    }

    public static IReadOnlyList<PlayerBase> AlivePlayers => alivePlayers;
}
