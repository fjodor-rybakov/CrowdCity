using System.Collections;
using System.Globalization;
using System.Linq;
using Boo.Lang;
using Helpers;
using Newtonsoft.Json;
using SocketIO;
using TMPro;
using UnityEngine;
using UnityEngine.AI;

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
    public Material fishRedMat;
    public Material fishYellowMat;
    public Material fishGreenMat;
    public Material fishBlueMat;
    public WinChecker winChecker;

    public readonly List<GameObject> otherPlayers = new List<GameObject>();

    public string clientId;
    public string color;

    private float _terrainWidth;
    private float _terrainLength;

    private float _xTerrainPos;
    private float _zTerrainPos;
    private SocketIOComponent _socket;
    private GameService _gameService;
    private GameObject selfNpcList;

    private const string PlayerEvent = "player";
    private const string ConnectedPlayerEvent = "connectedPlayer";
    private const string ExistAnotherPlayerEvent = "existAnotherPlayer";
    private const string NpcEvent = "npc";
    private const string OpenEvent = "open";
    private const string SetNameEvent = "setName";
    private TextMeshProUGUI _textTable;

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

        SetNameHandler();
        _socket.On(OpenEvent, OpenHandler);
        _socket.On(PlayerEvent, HandlePlayer);
        _socket.On(ConnectedPlayerEvent, HandleOpen);
        _socket.On(ExistAnotherPlayerEvent, HandleExistAnotherPlayer);
        /*_socket.On(NpcEvent, HandleNpc);*/
        _socket.On("expand", HandleExpand);

        StartCoroutine("ExistExecuteEvents");
    }

    // Update is called once per frame
    void Update()
    {
        AddNpcList();
        StartCoroutine("ExecuteEvents");
    }

    private void SetNameHandler()
    {
        var userName = PlayerPrefs.GetString("name");
            //winChecker.SetText(userName);
        var serializeJson = JsonConvert.SerializeObject(new {name = userName});
        _socket.Emit(SetNameEvent, new JSONObject(serializeJson));
    }

    private GameObject GenerateObjectOnTerrain(Transform objectParentTransform, Vector3 positionCoords,
        Quaternion rotationCoords)
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
        /*_gameService.SendCoords(NpcEvent, selfNpcList);
        yield return new WaitForSeconds(1);*/
    }

    public void ExpandPlayer(string toClintId)
    {
        var sendData = JsonConvert.SerializeObject(new {toClintId});
        StartCoroutine("ExpandPlayerCoroutine", sendData);
    }

    private IEnumerator ExpandPlayerCoroutine(string sendData)
    {
        _socket.Emit("expand", new JSONObject(sendData));
        yield return new WaitForSeconds(1);
    }

    private void AddNpcList()
    {
        if (clientId == "")
        {
            return;
        }

        if (selfNpcList.transform.childCount < countMinNpc)
        {
            GenerateObjectOnTerrain(selfNpcList.transform, CreateNpcPosition(), Quaternion.Euler(0, 0, 0));
        }
    }

    private void HandlePlayer(SocketIOEvent e)
    {
        if (e.data == null)
        {
            return;
        }

        var result = JsonConvert.DeserializeObject<ResponseData>(e.data.ToString());
        var item = otherPlayers.First(sp => sp.transform.name == result.id);
        for (var i = 0; i < result.data.Count; i++)
        {
            var coords = result.data.ElementAt(i);
            var coordsPos = Helper.ParseCoordsPos(coords);
            var coordsRot = Quaternion.Euler(0, Helper.ParseCoordsRot(coords).y, 0);
            item.transform.GetChild(i).transform.SetPositionAndRotation(coordsPos, coordsRot);
            var parentMat = item.transform.GetChild(0).GetChild(0).GetComponentInChildren<Renderer>().material;
            item.transform.GetChild(i).transform.GetChild(0).GetComponentInChildren<Renderer>().material = parentMat;
            if (result.data.Count > item.transform.childCount)
            {
                GenerateObjectOnTerrain(item.transform, Helper.ParseCoordsPos(coords), Quaternion.Euler(0, 0, 0));
            }
        }
    }

    private void HandleOpen(SocketIOEvent e)
    {
        Debug.Log("[SocketIO] Open received: " + e.name + " " + e.data);
        if (e.data == null)
        {
            return;
        }

        var result = JsonConvert.DeserializeObject<ClientData>(e.data.ToString());
        AddPlayer(result);
    }

    private void HandleExistAnotherPlayer(SocketIOEvent e)
    {
        Debug.Log("[SocketIO] Exist users: " + e.name + " " + e.data);
        if (e.data == null)
        {
            return;
        }

        var result = JsonConvert.DeserializeObject<ExistUserData>(e.data.ToString());
        for (var i = 0; i < result.data.Count; i++)
        {
            var playerRawData = result.data.ElementAt(i);
            AddPlayer(playerRawData);
        }
    }

    private void AddPlayer(ClientData result)
    {
        var newPlayer = new GameObject(result.id);
        newPlayer.transform.SetParent(field.transform);
        GenerateObjectOnTerrain(newPlayer.transform, new Vector3(0, yOffset, 0), Quaternion.Euler(0, 0, 0));
        newPlayer.transform.GetChild(0).transform.GetChild(0).GetComponentInChildren<Renderer>().material =
            SelectMaterialByName(result.color);
        var playerGroupCounter = Instantiate(groupCounter, newPlayer.transform.GetChild(0));
        playerGroupCounter.gameController = this;
        playerGroupCounter.group = newPlayer;
        playerGroupCounter.mainCamera = mainCamera;
        otherPlayers.Add(newPlayer);
        AddPlayerNpcList(result.id);
    }

    private void OpenHandler(SocketIOEvent e)
    {
        Debug.Log("[SocketIO] Connection was established: " + e.name + " " + e.data);
        var result = JsonConvert.DeserializeObject<FullUserData>(e.data.ToString());
        AddPlayerNpcList(result.id);
        clientId = result.id;
        color = result.color;
        groupParent.transform.GetChild(0).transform.GetChild(0).GetComponentInChildren<Renderer>().material =
            SelectMaterialByName(color);
    }

    private void AddPlayerNpcList(string id)
    {
        selfNpcList = new GameObject(id);
        selfNpcList.transform.SetParent(npcList.transform);
    }

    private void HandleNpc(SocketIOEvent e)
    {
        if (e.data == null)
        {
            return;
        }

        var result = JsonConvert.DeserializeObject<ResponseData>(e.data.ToString());
        var playerNpcList = npcList.transform.Find(result.id);
        /*// var countNpc = result.data.Count - npcList.transform.childCount;
        for (var i = 0; i < playerNpcList.childCount; i++)
        {
            var coords = result.data.ElementAt(i);
            if (result.data.Count > playerNpcList.childCount)
            {
                GenerateObjectOnTerrain(playerNpcList.transform, Helper.ParseCoordsPos(coords), Quaternion.Euler(0, 0, 0));
            }
        }*/
        if (playerNpcList.transform.childCount > 3) return;
        foreach (var item in result.data)
        {
            GenerateObjectOnTerrain(playerNpcList.transform, Helper.ParseCoordsPos(item), Quaternion.Euler(0, 0, 0));
        }
    }
    
    private void HandleExpand(SocketIOEvent e)
    {
        if (e.data == null)
        {
            return;
        }
        var deserializeObjectData = JsonConvert.DeserializeObject<ExpandData>(e.data.ToString());
        var toPlayer = otherPlayers.FirstOrDefault(player => player.name == deserializeObjectData.to.clientId);
        if (toPlayer == null)
        {
            return;
        }

        if (deserializeObjectData.from.isExpand == false && deserializeObjectData.to.isExpand == false)
        {
            return;
        }
        if (deserializeObjectData.from.isExpand && clientId == deserializeObjectData.from.clientId) // Если нас больше
        {
            // Убиваем игрока
            Debug.Log("Expand player");
            var item = toPlayer.transform.GetChild(toPlayer.transform.childCount - 1);
            if (item.GetComponentInChildren<GroupCounter>())
            {
                Destroy(item.GetComponentInChildren<GroupCounter>().gameObject);
            }
            AddToGroup(ref item, groupParent.transform, new ExtendsProperties{groupParent = groupParent, mainCamera = mainCamera, winChecker = winChecker});
            Destroy(toPlayer.transform.GetChild(toPlayer.transform.childCount - 1).gameObject);
        }
        else if (deserializeObjectData.from.isExpand == false && clientId == deserializeObjectData.from.clientId) // Если нас меньше
        {
            // Мы проиграли
            Debug.Log("You lose!");
            winChecker.timer.targetTime = 0;
        }
    }
    
    public void AddToGroup(ref Transform item, Transform transformPatent, ExtendsProperties extendsProperties)
    {
        item.transform.SetParent(transformPatent);
        var agentComponent = item.transform.gameObject.AddComponent<NavMeshAgent>();
        agentComponent.speed = 8;
        agentComponent.acceleration = 10;
        var moveSComponent = item.transform.gameObject.AddComponent<Move>();
        moveSComponent.mainCamera = extendsProperties.mainCamera;
        moveSComponent.gameController = this;
        moveSComponent.groupParent = extendsProperties.groupParent;
        moveSComponent.winChecker = extendsProperties.winChecker;
        item.transform.GetChild(0).transform.GetChild(0).GetComponentInChildren<Renderer>().material = SelectMaterialByName(color);
    }

    public Material SelectMaterialByName(string materialName)
    {
        switch (materialName)
        {
            case "fish-red-mat":
                return fishRedMat;
            case "fish-yellow-mat":
                return fishYellowMat;
            case "fish-green-mat":
                return fishGreenMat;
            case "fish-blue-mat":
                return fishBlueMat;
        }

        return null;
    }
}