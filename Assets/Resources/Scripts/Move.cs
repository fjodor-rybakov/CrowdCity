using UnityEngine;
using UnityEngine.AI;

public class Move : MonoBehaviour
{
    public Camera mainCamera;
    public GameObject npcList;
    public int triggeredDistance = 2;
    public GameObject groupParent;
    
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
        for (var i = 0; i < npcList.transform.childCount; i++)
        {
            var item = npcList.transform.GetChild(i);
            var distance = Vector3.Distance(item.position, _agent.transform.position);
            if (!(distance <= triggeredDistance)) continue;
            AddToGroup(ref item);
        }
    }

    private void AddToGroup(ref Transform item)
    {
        item.transform.SetParent(groupParent.transform);
        var agentComponent = item.transform.gameObject.AddComponent<NavMeshAgent>();
        agentComponent.speed = 8;
        agentComponent.acceleration = 10;
        var moveSComponent = item.transform.gameObject.AddComponent<Move>();
        moveSComponent.mainCamera = mainCamera;
        moveSComponent.npcList = npcList;
        moveSComponent.groupParent = groupParent;
    }

    private void MoveToLocation()
    {
        if (!Input.GetMouseButtonDown(0)) return;
        var ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        if (!Physics.Raycast(ray, out var hit, 1000.0f)) return;
        _agent.SetDestination(new Vector3(hit.point.x, hit.point.y, hit.point.z));
    }
}
