using System.Linq;
using Helpers;
using Newtonsoft.Json;
using SocketIO;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class WinChecker : MonoBehaviour
{
    public GameObject buttonBackMenu;
    public GameObject resultTable;
    public bool isMove = true;
    public TimerUI timer;

    private SocketIOComponent _socket;
    private const string GetGroupCountEvent = "getCountGroup";
    private TextMeshProUGUI _textTable;

    // Start is called before the first frame update
    void Start()
    {
        GameObject go = GameObject.Find("SocketIO");
        _socket = go.GetComponent<SocketIOComponent>();
        _socket.On(GetGroupCountEvent, HandlerGetGroupCount);
        _textTable = resultTable.GetComponent<TextMeshProUGUI>();
    }

    // Update is called once per frame
    void Update()
    {
        if (timer.targetTime <= 0)
        {
            timer.isActive = false;
            _socket.Emit(GetGroupCountEvent);
        }
    }

    private void HandlerGetGroupCount(SocketIOEvent e)
    {
        if (e.data == null)
        {
            return;
        }

        var result = JsonConvert.DeserializeObject<GroupData>(e.data.ToString());
        var data = result.data.Sort((i1, i2) => i1.count > i2.count ? 1 : 0);
        _textTable.text = string.Concat(data.Select(item => $"{item.id} {item.count}\n"));
        resultTable.SetActive(true);
        buttonBackMenu.SetActive(true);
        isMove = false;
    }

    public void BackToMenu()
    {
        _socket.Close();
        SceneManager.LoadScene("menu_ar", LoadSceneMode.Single);
    }
}