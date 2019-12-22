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

    // Start is called before the first frame update
    void Start()
    {
        _agent = GetComponent<NavMeshAgent>();
    }

    // Update is called once per frame
    void Update()
    {
        MoveToLocation();
        Grouping();
    }

    private void Grouping()
    {
        for (var i = 0; i < gameController.npcList.transform.childCount; i++)
        {
            var item = gameController.npcList.transform.GetChild(i);
            var distance = Vector3.Distance(item.position, _agent.transform.position);
            if (!(distance <= triggeredDistance)) continue;
            AddToGroup(ref item);
        }
        /*for (var i = 0; i < gameController.npcaList.transform.childCount; i++)
        {
            var item = gameController.npcaList.transform.GetChild(i);
            var distance = Vector3.Distance(item.position, _agent.transform.position);
            if (!(distance <= triggeredDistance)) continue;
            AddToGroup(ref item);
        }*/
    }

    private void AddToGroup(ref Transform item)
    {
        item.transform.SetParent(groupParent.transform);
        var agentComponent = item.transform.gameObject.AddComponent<NavMeshAgent>();
        agentComponent.speed = 8;
        agentComponent.acceleration = 10;
        var moveSComponent = item.transform.gameObject.AddComponent<Move>();
        moveSComponent.mainCamera = mainCamera;
        moveSComponent.gameController = gameController;
        moveSComponent.groupParent = groupParent;
        moveSComponent.winChecker = winChecker;
        item.transform.GetChild(0).transform.GetChild(0).GetComponentInChildren<Renderer>().material = gameController.SelectMaterialByName(gameController.color);
    }

    private void MoveToLocation()
    {
        if (!Input.GetMouseButtonDown(0) || !winChecker.isMove) return;
        var ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        if (!Physics.Raycast(ray, out var hit, 1000.0f)) return;
        _agent.SetDestination(new Vector3(hit.point.x, hit.point.y, hit.point.z));
    }
}