using UnityEngine;

namespace LevelCrewmate
{
    public static class Extensions
    {
        public static Vector3 SetX(this Transform transform, float x)
        {
            var position = transform.position;
            var original = position;
            position = new Vector3(x, original.y, original.z);
            transform.position = position;
            return position;
        }
        
        public static Vector3 SetY(this Transform transform, float y)
        {
            var position = transform.position;
            var original = position;
            position = new Vector3(original.x, y, original.z);
            transform.position = position;
            return position;
        }
        
        public static Vector3 SetZ(this Transform transform, float z)
        {
            var position = transform.position;
            var original = position;
            position = new Vector3(original.x, original.y, z);
            transform.position = position;
            return position;
        }
    }
}