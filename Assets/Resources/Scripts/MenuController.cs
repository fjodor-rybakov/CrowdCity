using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuController : MonoBehaviour
{
    public TMP_InputField inputField;
    
    private string _name;
    private const string KeyName = "name";

    private void Start()
    {
        if (PlayerPrefs.HasKey(KeyName))
        {
            inputField.text = PlayerPrefs.GetString(KeyName);
        }
    }

    public void SetName()
    {
        _name = inputField.text;
    }

    public void OnClickPlay()
    {
        PlayerPrefs.SetString(KeyName, _name);
        Debug.Log("NAME!!!: " + PlayerPrefs.GetString(KeyName));
        SceneManager.LoadScene("base_ar", LoadSceneMode.Single);
    }
}