using UnityEngine;
using UnityEngine.UI;

public class HowToPlayPanel : MonoBehaviour
{
    [SerializeField] Button backButton;
    [SerializeField] MainMenuController menu;

    void Start()
    {
        if (backButton != null) backButton.onClick.AddListener(Back);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) Back();
    }

    void Back()
    {
        if (menu != null) menu.ShowMain();
        else gameObject.SetActive(false);
    }
}
