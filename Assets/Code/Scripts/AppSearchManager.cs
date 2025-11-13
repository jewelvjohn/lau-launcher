using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AppSearchManager : MonoBehaviour
{
    [SerializeField] private TMP_InputField searchInputField;
    [SerializeField] private Button closeButton;

    [SerializeField] private AppDrawerManager AppDrawerManager;

    private void Awake()
    {
        InitializeSearchField();
    }

    private void InitializeSearchField()
    {
        searchInputField.onValueChanged.AddListener(OnSearchChanged);
        searchInputField.onEndEdit.AddListener(OnSearchEdit);
        searchInputField.onSelect.AddListener(OnSearchSelect);
        searchInputField.onDeselect.AddListener(OnSearchDeselect);
        closeButton.onClick.AddListener(OnSearchClose);

        closeButton.gameObject.SetActive(false);
    }

    private void OnSearchChanged(string text)
    {
        AppDrawerManager.FindSearchedApps(text);
    }

    private void OnSearchEdit(string text)
    {
        AppDrawerManager.FindSearchedApps(text);
    }

    private void OnSearchSelect(string text)
    {
        closeButton.gameObject.SetActive(true);
    }

    private void OnSearchDeselect(string text)
    {
        closeButton.gameObject.SetActive(false);
        AppDrawerManager.FindAllApps();

        searchInputField.text = "";
    }

    private void OnSearchClose()
    {
        closeButton.gameObject.SetActive(false);
        AppDrawerManager.FindAllApps();

        searchInputField.ReleaseSelection();
    }
}
