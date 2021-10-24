namespace Apollo.Data
{
    public class Vector2
    {
        public float X { get; set; }
        public float Y { get; set; }

        public Vector2(float x, float y)
        {
            X = x;
            Y = y;
        }

        public static implicit operator UnityEngine.Vector3(Vector2 vec) => new (vec.X, vec.Y);
    }
}