using System.Linq;
using Helpers;
using Newtonsoft.Json;
using SocketIO;
using UnityEngine;
using UnityEngine.AI;

public class Move : MonoBehaviour
{
    public Camera mainCamera;
    public int triggeredDistance = 2;
    public GameObject groupParent;
    public WinChecker winChecker;
    public GameController gameController;

    private NavMeshAgent _agent;
    private SocketIOComponent _socket;

    // Start is called before the first frame update
    void Start()
    {
        _agent = GetComponent<NavMeshAgent>();
        GameObject go = GameObject.Find("SocketIO");
        _socket = go.GetComponent<SocketIOComponent>();
    }

    // Update is called once per frame
    void Update()
    {
        MoveToLocation();
        Grouping();
    }

    private void Grouping()
    {
        FromNpc();
        FromPlayer();
    }

    private void FromNpc()
    {
        for (var i = 0; i < gameController.npcList.transform.childCount; i++)
        {
            for (var j = 0; j < gameController.npcList.transform.GetChild(i).childCount; j++)
            {
                var item = gameController.npcList.transform.GetChild(i).GetChild(j);
                var distance = Vector3.Distance(item.position, _agent.transform.position);
                if (!(distance <= triggeredDistance)) continue;
                gameController.AddToGroup(ref item, groupParent.transform, new ExtendsProperties{groupParent = groupParent, mainCamera = mainCamera, winChecker = winChecker});
            }
        }
    }

    private void FromPlayer()
    {
        foreach (var player in gameController.otherPlayers)
        {
            for (var j = 0; j < player.transform.childCount; j++)
            {
                var item = player.transform.GetChild(j);
                var distance = Vector3.Distance(item.position, _agent.transform.position);
                if (!(distance <= triggeredDistance)) continue;
                var toClintId = player.name;
                //Debug.Log(toClintId);
                gameController.ExpandPlayer(toClintId);
                // AddToGroup(ref item);
            }
        }
    }

    private void MoveToLocation()
    {
        if (!Input.GetMouseButtonDown(0) || !winChecker.isMove) return;
        var ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        if (!Physics.Raycast(ray, out var hit, 1000.0f)) return;
        _agent.SetDestination(new Vector3(hit.point.x, hit.point.y, hit.point.z));
    }
}