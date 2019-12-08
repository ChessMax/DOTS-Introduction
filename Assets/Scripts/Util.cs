using UnityEngine;

public static class Util
{
    public static Vector3 GetRandomPosition(this Camera camera)
    {
        var pos = camera.ViewportToWorldPoint(new Vector3(
            Random.Range(0f, 1f),
            Random.Range(0f, 1f)));
        pos.z = 0f;

        return pos;
    }
}