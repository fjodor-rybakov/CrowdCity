using System.Collections;
using System.Linq;
using Boo.Lang;
using Helpers;
using Newtonsoft.Json;
using SocketIO;
using UnityEngine;

public class GameController : MonoBehaviour
{
    public GameObject npcList;
    public Terrain terrain;
    public GameObject npcPrefab;
    public float yOffset = 0.5f;
    public float yOffsetCounter = 7.24f;
    public int countMinNpc = 3;
    public GameObject field;
    public GameObject groupParent;
    public Camera mainCamera;
    public GroupCounter groupCounter;
    private readonly List<GameObject> _otherPlayers = new List<GameObject>();

    public string clientId;

    private float _terrainWidth;
    private float _terrainLength;

    private float _xTerrainPos;
    private float _zTerrainPos;
    private SocketIOComponent _socket;
    private GameService _gameService;

    private const string PlayerEvent = "player";
    private const string ConnectedPlayerEvent = "connectedPlayer";
    private const string ExistAnotherPlayerEvent = "existAnotherPlayer";
    private const string NpcEvent = "npc";
    private const string OpenEvent = "open";
    private const string GetGroupCoordsEvent = "getGroupCoords";

    // Start is called before the first frame update
    void Start()
    {
        //Get terrain size
        var terrainData = terrain.terrainData;
        _terrainWidth = terrainData.size.x;
        _terrainLength = terrainData.size.z;

        //Get terrain position
        var position = terrain.transform.position;
        _xTerrainPos = position.x;
        _zTerrainPos = position.z;
        
        GameObject go = GameObject.Find("SocketIO");
        _socket = go.GetComponent<SocketIOComponent>();
        _gameService = new GameService(_socket);

        _socket.On(OpenEvent, OpenHandler);
        _socket.On(PlayerEvent, HandlePlayer);
        _socket.On(ConnectedPlayerEvent, HandleOpen);
        _socket.On(ExistAnotherPlayerEvent, HandleExistAnotherPlayer);
        /*_socket.On(NpcEvent, HandleNpc);*/
        
        StartCoroutine("ExistExecuteEvents");
    }
    
    // Update is called once per frame
    void Update()
    {
        AddNpcList();
        StartCoroutine("ExecuteEvents");
    }

    private GameObject GenerateObjectOnTerrain(Transform objectParentTransform, Vector3 positionCoords, Quaternion rotationCoords)
    {
        //Generate the Prefab on the generated position
        var objInstance = Instantiate(npcPrefab, positionCoords, rotationCoords);
        objInstance.transform.SetParent(objectParentTransform);
        return objInstance;
    }

    private Vector3 CreateNpcPosition()
    {
        //Generate random x,z,y position on the terrain
        var randX = Random.Range(_xTerrainPos, _xTerrainPos + _terrainWidth);
        var randZ = Random.Range(_zTerrainPos, _zTerrainPos + _terrainLength);
        var yVal = Terrain.activeTerrain.SampleHeight(new Vector3(randX, 0, randZ));

        //Apply Offset if needed
        yVal += yOffset;
        return new Vector3(randX, yVal, randZ);
    }

    private IEnumerator ExistExecuteEvents()
    {
        yield return new WaitForSeconds(1);
        _gameService.ExistPlayer(ExistAnotherPlayerEvent);
    }
    
    private IEnumerator ExecuteEvents()
    {
        _gameService.SendCoords(PlayerEvent, groupParent);
        yield return new WaitForSeconds(1);
        /*_gameService.SendCoords(NpcEvent, npcList);
        yield return new WaitForSeconds(1);*/
    }

    private void AddNpcList()
    {
        if (npcList.transform.childCount < countMinNpc)
            GenerateObjectOnTerrain(npcList.transform, CreateNpcPosition(), Quaternion.Euler(0, 0, 0));
    }

    private void HandlePlayer(SocketIOEvent e)
    {
        if (e.data == null) { return; }
        var result = JsonConvert.DeserializeObject<ResponseData>(e.data.ToString());
        var item = _otherPlayers.First(sp => sp.transform.name == result.id);
        for (var i = 0; i < result.data.Count; i++)
        {
            var coords = result.data.ElementAt(i);
            var coordsPos = Helper.ParseCoordsPos(coords);
            var coordsRot = Quaternion.Euler(0, Helper.ParseCoordsRot(coords).y, 0);
            item.transform.GetChild(i).transform.SetPositionAndRotation(coordsPos, coordsRot);
            if (result.data.Count > item.transform.childCount)
            {
                GenerateObjectOnTerrain(item.transform, Helper.ParseCoordsPos(coords), Quaternion.Euler(0, 0, 0));
            }
        }
    }

    private void HandleOpen(SocketIOEvent e)
    {
        Debug.Log("[SocketIO] Open received: " + e.name + " " + e.data);
        if (e.data == null) { return; }
        var result = JsonConvert.DeserializeObject<UserData>(e.data.ToString());
        var newPlayer = new GameObject(result.id);
        newPlayer.transform.SetParent(field.transform);
        GenerateObjectOnTerrain(newPlayer.transform, new Vector3(0, yOffsetCounter, 0), Quaternion.Euler(0, 0, 0));
        var playerGroupCounter = Instantiate(groupCounter, newPlayer.transform.GetChild(0));
        playerGroupCounter.gameController = this;
        playerGroupCounter.group = newPlayer;
        playerGroupCounter.mainCamera = mainCamera;
        _otherPlayers.Add(newPlayer);
    }

    private void HandleExistAnotherPlayer(SocketIOEvent e)
    {
        Debug.Log("[SocketIO] Exist users: " + e.name + " " + e.data);
        if (e.data == null) { return; }
        var result = JsonConvert.DeserializeObject<ExistUserData>(e.data.ToString());
        if (result.data.Count == 0) { return; }

        for (var i = 0; i < result.data.Count; i++)
        {
            var newPlayer = new GameObject(result.data.ElementAt(i));
            newPlayer.transform.SetParent(field.transform);
            GenerateObjectOnTerrain(newPlayer.transform, new Vector3(0, yOffset, 0), Quaternion.Euler(0, 0, 0));
            var playerGroupCounter = Instantiate(groupCounter, newPlayer.transform.GetChild(0));
            playerGroupCounter.gameController = this;
            playerGroupCounter.group = newPlayer;
            playerGroupCounter.mainCamera = mainCamera;
            _otherPlayers.Add(newPlayer);
        }
    }

    private void OpenHandler(SocketIOEvent e)
    {
        Debug.Log("[SocketIO] Connection was established: " + e.name + " " + e.data);
        var result = JsonConvert.DeserializeObject<UserData>(e.data.ToString());
        clientId = result.id;
    }

    private void HandleNpc(SocketIOEvent e)
    {
        if (e.data == null) { return; }
        var result = JsonConvert.DeserializeObject<ResponseData>(e.data.ToString());
        for (var i = 0; i < result.data.Count; i++)
        {
            var coords = result.data.ElementAt(i);
            if (result.data.Count > npcList.transform.childCount)
            {
                GenerateObjectOnTerrain(npcList.transform, Helper.ParseCoordsPos(coords), Quaternion.Euler(0, 0, 0));
            }
        }
    }
}
