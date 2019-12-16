using Newtonsoft.Json;
using SocketIO;
using TMPro;
using UnityEngine;

public class GroupCounter : MonoBehaviour
{
    public Camera mainCamera;
    public GameObject group;
    public GameController gameController;

    private SocketIOComponent _socket;
    private TextMeshPro _text;
    private int _currentCount = 1;
    private const string SetGroupCountEvent = "setCountGroup";

    private void Start()
    {
        _text = GetComponent<TextMeshPro>();
        GameObject go = GameObject.Find("SocketIO");
        _socket = go.GetComponent<SocketIOComponent>();
    }

    // Update is called once per frame
    void Update()
    {
        transform.LookAt(mainCamera.transform);
        transform.localScale = new Vector3(-1, 1, 1);
        _text.text = group.transform.childCount.ToString();
        if (group.transform.childCount == _currentCount) return;
        _currentCount = group.transform.childCount;
        SendGroupCount(SetGroupCountEvent, _currentCount);
    }

    private void SendGroupCount(string eventName, int count)
    {
        var objectData = JsonConvert.SerializeObject(new {count});
        _socket.Emit(eventName, new JSONObject(objectData));
    }
}