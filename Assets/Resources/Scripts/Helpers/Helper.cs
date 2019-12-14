using System.Globalization;
using UnityEngine;

namespace Helpers
{
    public class Helper : MonoBehaviour
    {
        public static Vector3 ParseCoords(Coords coords)
        {
            var x = float.Parse(coords.X, CultureInfo.InvariantCulture.NumberFormat);
            var y = float.Parse(coords.Y, CultureInfo.InvariantCulture.NumberFormat);
            var z = float.Parse(coords.Z, CultureInfo.InvariantCulture.NumberFormat);
            return new Vector3(x, y, z);
        }
    }
}