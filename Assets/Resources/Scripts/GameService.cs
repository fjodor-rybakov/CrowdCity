using System.Globalization;
using Boo.Lang;
using Helpers;
using Newtonsoft.Json;
using SocketIO;
using UnityEngine;

public class GameService
{
    private readonly SocketIOComponent _socket;

    public GameService(SocketIOComponent socket)
    {
        _socket = socket;
    }

    public void SendCoords(string eventName, GameObject group)
    {
        if (group.transform.childCount <= 0) return;
        var data = new List<Coords>();
        for (var i = 0; i < group.transform.childCount; i++)
        {
            var item = group.transform.GetChild(i);
            var transform = item.transform;
            var pos = transform.position;
            var rot = transform.rotation.eulerAngles;
            data.Add(new Coords
            {
                X = pos.x.ToString(CultureInfo.InvariantCulture),
                Y = pos.y.ToString(CultureInfo.InvariantCulture),
                Z = pos.z.ToString(CultureInfo.InvariantCulture),
                RotY = rot.y.ToString(CultureInfo.InvariantCulture)
            });
        }

        var serializeData = JsonConvert.SerializeObject(new {data});
        _socket.Emit(eventName, new JSONObject(serializeData));
    }

    public void ExistPlayer(string eventName)
    {
        _socket.Emit(eventName);
    }
}