using UnityEngine;
using System.Collections;

public class FogManager : MonoBehaviour {

    public FogSettings normal = new FogSettings(.5f, FogMode.Linear, new Color32(147, 165, 206, 255), null);
    public FogSettings underwater = new FogSettings(.04f, FogMode.Exponential, new Color(0, .4f, .7f), null);
    public WorldController controller;
    public bool water;

    void Start()
    {
        RenderSettings.fog = true;
    }

	void FixedUpdate () {
        RenderSettings.fogEndDistance = controller.loadDistance * World.CHUNK_SIZE;
        (water ? underwater : normal).Apply();
	}
}

[System.Serializable]
public class FogSettings
{
    public float distanceOrDensity;
    public FogMode mode;
    public Color color;
    public Material skybox;

    public FogSettings(float d, FogMode m, Color c, Material s)
    {
        distanceOrDensity = d;
        mode = m;
        color = c;
        skybox = s;
    }

    public void Apply()
    {
        RenderSettings.fogStartDistance = RenderSettings.fogEndDistance * distanceOrDensity;
        RenderSettings.fogDensity = distanceOrDensity;
        RenderSettings.fogMode = mode;
        RenderSettings.fogColor = color;
        RenderSettings.skybox = skybox;
        Camera.main.backgroundColor = color;
    }
}
