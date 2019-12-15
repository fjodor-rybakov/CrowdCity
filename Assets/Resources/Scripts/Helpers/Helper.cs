using System.Globalization;
using UnityEngine;

namespace Helpers
{
    public class Helper : MonoBehaviour
    {
        public static Vector3 ParseCoordsPos(Coords coords)
        {
            var x = float.Parse(coords.X, CultureInfo.InvariantCulture.NumberFormat);
            var y = float.Parse(coords.Y, CultureInfo.InvariantCulture.NumberFormat);
            var z = float.Parse(coords.Z, CultureInfo.InvariantCulture.NumberFormat);
            return new Vector3(x, y, z);
        }

        public static Vector3 ParseCoordsRot(Coords coords)
        {
            var y = float.Parse(coords.RotY, CultureInfo.InvariantCulture.NumberFormat);
            return new Vector3(0, y, 0);
        }
    }
}