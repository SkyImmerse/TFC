using Assets.Tibia.DAO;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LightSurface : MonoBehaviour
{
    public Dictionary<Thing, Vector4> Colors = new Dictionary<Thing, Vector4>();
    public Dictionary<Thing, Vector4> Positions = new Dictionary<Thing, Vector4>();
    private Color Ambient;

    static float LIGHT_LEVEL_DAY = 2f;
    static float LIGHT_LEVEL_NIGHT = 3;

    // Update is called once per frames
    void FixedUpdate()
    {
        if (!GetComponent<MeshRenderer>().enabled) return;

        SetupLights();
        GetComponent<MeshRenderer>().material.SetColor("_AmbientColor", Ambient);
    }

    public void SetupLights()
    {
        if (Colors.Count != Positions.Count) return;
        if (Colors.Count == 0) return;
        GetComponent<MeshRenderer>().material.SetInt("_Layer", int.Parse(LayerMask.LayerToName(gameObject.layer)));
        GetComponent<MeshRenderer>().material.SetInt("_LightCounts", Colors.Count);
        var colors = new List<Vector4>();
        colors.AddRange(Colors.Values.Take(300));
        if (Colors.Count < 300)
            colors.AddRange(Enumerable.Repeat(new Vector4(), 300 - Colors.Count));

        GetComponent<MeshRenderer>().material.SetVectorArray("_LightColors", colors);

        var positions = new List<Vector4>();
        positions.AddRange(Positions.Values.Take(300));
        if (Positions.Count < 300)
            positions.AddRange(Enumerable.Repeat(new Vector4(), 300 - Positions.Count));

        GetComponent<MeshRenderer>().material.SetVectorArray("_LightsPositions", positions);
    }

    internal void SetupAmbientLight(Color c, float intensity)
    {
        // 3 -- 40
        // 1.5 -- 250
        var l = map(intensity, 40, 250, LIGHT_LEVEL_NIGHT, LIGHT_LEVEL_DAY);

        Ambient = new Color((c.r / 255f) / l, (c.g / 255f) / l, (c.b / 255f) / l, 1);
    }

    float map(float s, float a1, float a2, float b1, float b2)
    {
        return b1 + (s - a1) * (b2 - b1) / (a2 - a1);
    }

}
