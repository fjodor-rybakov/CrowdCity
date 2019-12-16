using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
    public void OnClickPlay()
    {
        SceneManager.LoadScene("base_ar", LoadSceneMode.Single);
    }
}
