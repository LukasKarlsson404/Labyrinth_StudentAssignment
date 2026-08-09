using UnityEngine;

public static class JsonHelper
{
    public static T FromJson<T>(string json)
    {
        T result = JsonUtility.FromJson<T>(json);

        if (result is MapData mapData)
        {
            mapData.NormalizeDefaults();
        }

        return result;
    }
}
