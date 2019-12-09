using UnityEngine;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour
{
    public GameObject npcList;
    public Terrain terrain;
    public GameObject npcPrefab;
    public float yOffset = 0.5f;
    public int countMinNpc = 3;

    private float _terrainWidth;
    private float _terrainLength;

    private float _xTerrainPos;
    private float _zTerrainPos;

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
    }

    private void GenerateObjectOnTerrain()
    {
        //Generate random x,z,y position on the terrain
        var randX = Random.Range(_xTerrainPos, _xTerrainPos + _terrainWidth);
        var randZ = Random.Range(_zTerrainPos, _zTerrainPos + _terrainLength);
        var yVal = Terrain.activeTerrain.SampleHeight(new Vector3(randX, 0, randZ));

        //Apply Offset if needed
        yVal += yOffset;

        //Generate the Prefab on the generated position
        var objInstance = Instantiate(npcPrefab, new Vector3(randX, yVal, randZ), Quaternion.identity);
        objInstance.transform.SetParent(npcList.transform);
    }

    // Update is called once per frame
    void Update()
    {
        AddNpcList();
    }

    private void AddNpcList()
    {
        if (npcList.transform.childCount < countMinNpc)
            GenerateObjectOnTerrain();
    }
}
