using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HomeButton : MonoBehaviour
{
    [SerializeField] private TMP_Text applicationName;
    [SerializeField] private Image applicationIcon;

    public void SetName(string name)
    {
        applicationName.text = name;
    }

    public void SetIcon(Sprite icon)
    {
        applicationIcon.sprite = icon;
    }
}
