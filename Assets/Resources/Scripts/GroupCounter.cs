using TMPro;
using UnityEngine;

public class GroupCounter : MonoBehaviour
{
    public Camera mainCamera;
    public GameObject group;

    private TextMeshPro _text;

    private void Start()
    {
        _text = GetComponent<TextMeshPro>();
    }
    
    // Update is called once per frame
    void Update()
    {
        transform.LookAt(mainCamera.transform);
        transform.localScale = new Vector3(-1, 1, 1);
        _text.text = group.transform.childCount.ToString();
    }
}
