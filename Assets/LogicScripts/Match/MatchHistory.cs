using UnityEngine;

public static class MatchHistory
{
    const string PvPWinFmt    = "PPP_PVP_WINS_P{0}";
    const string PvPDrawKey   = "PPP_PVP_DRAWS";
    const string PvPTotalKey  = "PPP_PVP_TOTAL";
    const string CoopBestFmt  = "PPP_COOP_BEST_{0}P_HARD";

    public static void RecordPvP(MatchManager.Outcome o, int winnerIdx)
    {
        PlayerPrefs.SetInt(PvPTotalKey, PlayerPrefs.GetInt(PvPTotalKey, 0) + 1);
        if (o == MatchManager.Outcome.Win)
        {
            string key = string.Format(PvPWinFmt, winnerIdx);
            PlayerPrefs.SetInt(key, PlayerPrefs.GetInt(key, 0) + 1);
        }
        else if (o == MatchManager.Outcome.Draw)
        {
            PlayerPrefs.SetInt(PvPDrawKey, PlayerPrefs.GetInt(PvPDrawKey, 0) + 1);
        }
        PlayerPrefs.Save();
    }

    public static int   PvPWinsOf(int playerIdx) => PlayerPrefs.GetInt(string.Format(PvPWinFmt, playerIdx), 0);
    public static int   PvPDraws()               => PlayerPrefs.GetInt(PvPDrawKey, 0);
    public static int   PvPTotal()               => PlayerPrefs.GetInt(PvPTotalKey, 0);

    public static float GetCoopBest(int playerCount)
    {
        return PlayerPrefs.GetFloat(string.Format(CoopBestFmt, playerCount), 0f);
    }

    public static void SetCoopBest(int playerCount, float seconds)
    {
        PlayerPrefs.SetFloat(string.Format(CoopBestFmt, playerCount), seconds);
        PlayerPrefs.Save();
    }

    public static void ClearAll()
    {
        for (int i = 1; i <= 3; i++) PlayerPrefs.DeleteKey(string.Format(PvPWinFmt, i));
        PlayerPrefs.DeleteKey(PvPDrawKey);
        PlayerPrefs.DeleteKey(PvPTotalKey);
        for (int n = 2; n <= 3; n++) PlayerPrefs.DeleteKey(string.Format(CoopBestFmt, n));
        PlayerPrefs.Save();
    }
}
