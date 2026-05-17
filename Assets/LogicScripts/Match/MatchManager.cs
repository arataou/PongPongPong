using System;
using System.Collections.Generic;
using UnityEngine;

public class MatchManager : MonoBehaviour
{
    public enum Outcome { Ongoing, Win, Draw, CoopEnd }

    public static MatchManager Instance { get; private set; }

    static readonly List<PlayerBase> alivePlayers = new List<PlayerBase>();

    public Outcome  CurrentOutcome     { get; private set; } = Outcome.Ongoing;
    public int      WinningPlayerIndex { get; private set; }
    public float    ElapsedTime        { get; private set; }
    public float    CoopBestBefore     { get; private set; }
    public bool     CoopNewRecord      { get; private set; }
    public GameMode Mode               { get; private set; } = GameMode.PvP;
    public event Action<Outcome> OnMatchEnded;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        alivePlayers.Clear();
        CurrentOutcome = Outcome.Ongoing;
        WinningPlayerIndex = 0;
        ElapsedTime = 0f;
        CoopNewRecord = false;
        Time.timeScale = 1f;
        Mode = GameSession.Instance != null ? GameSession.Instance.Mode : GameMode.PvP;
    }

    void Update()
    {
        if (CurrentOutcome != Outcome.Ongoing) return;
        if (Time.timeScale == 0f) return;
        ElapsedTime += Time.deltaTime;
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

        if (Mode == GameMode.Coop)
        {
            if (alivePlayers.Count == 0) BeginEnd(Outcome.CoopEnd, 0);
        }
        else
        {
            if (alivePlayers.Count == 0)       BeginEnd(Outcome.Draw, 0);
            else if (alivePlayers.Count == 1)  BeginEnd(Outcome.Win, alivePlayers[0].PlayerIndex);
        }
    }

    void BeginEnd(Outcome o, int winnerIdx)
    {
        CurrentOutcome     = o;
        WinningPlayerIndex = winnerIdx;

        if (o == Outcome.CoopEnd) RecordCoopResult();
        else if (o == Outcome.Win || o == Outcome.Draw) MatchHistory.RecordPvP(o, winnerIdx);

        if (GameFeel.Instance != null)
            GameFeel.Instance.PlayMatchEnd(FinalizeEnd);
        else
            FinalizeEnd();
    }

    void RecordCoopResult()
    {
        if (GameSession.Instance == null) { CoopBestBefore = 0f; return; }
        if (GameSession.Instance.Difficulty != AIDifficulty.Hard)
        {
            CoopBestBefore = 0f;
            return;
        }
        CoopBestBefore = MatchHistory.GetCoopBest(GameSession.Instance.PlayerCount);
        if (ElapsedTime > CoopBestBefore)
        {
            CoopNewRecord = true;
            MatchHistory.SetCoopBest(GameSession.Instance.PlayerCount, ElapsedTime);
        }
    }

    void FinalizeEnd()
    {
        Time.timeScale = 0f;
        OnMatchEnded?.Invoke(CurrentOutcome);
    }

    public static IReadOnlyList<PlayerBase> AlivePlayers => alivePlayers;
}
