using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AppSearchManager : MonoBehaviour
{
    [SerializeField] private TMP_InputField searchInputField;
    [SerializeField] private Button searchButton;

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
        searchButton.onClick.AddListener(OnSearchButton);
    }

    private void OnSearchChanged(string text)
    {
        Debug.Log("Search text changed: " + text);   
    }

    private void OnSearchEdit(string text)
    {
        Debug.Log("Search text edited: " + text);
    }

    private void OnSearchSelect(string text)
    {
        Debug.Log("Selected search item: " + text);
    }

    private void OnSearchDeselect(string text)
    {
        Debug.Log("Deselected search item: " + text);
    }

    private void OnSearchButton()
    {
        Debug.Log("Search button clicked with text: " + searchInputField.text);
    }
}
