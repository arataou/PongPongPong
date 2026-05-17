using UnityEngine;

public class PlayerRoster : MonoBehaviour
{
    [SerializeField] GameObject player1;
    [SerializeField] GameObject player2;
    [SerializeField] GameObject player3;

    void Awake()
    {
        int count = GameSession.Instance != null ? GameSession.Instance.PlayerCount : 2;

        if (player1 != null) player1.SetActive(count >= 1);
        if (player2 != null) player2.SetActive(count >= 2);
        if (player3 != null) player3.SetActive(count >= 3);
    }
}