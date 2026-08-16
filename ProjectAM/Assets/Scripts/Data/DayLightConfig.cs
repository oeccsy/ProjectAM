using System;
using UnityEngine;

[CreateAssetMenu(fileName = "DayLightConfig", menuName = "ProjectAM/DayLightConfig")]
public class DayLightConfig : ScriptableObject
{
    [Serializable]
    public class PhaseLighting
    {
        public Color sunColor;
        public float sunIntensity;
        public Color ambientSky;
        public Color ambientEquator;
        public Color ambientGround;
        public Color fogColor;
        public float fogStart;
        public float fogEnd;
    }

    public PhaseLighting day = new PhaseLighting
    {
        sunColor = new Color(1.00f, 0.96f, 0.85f),
        sunIntensity = 1.10f,
        ambientSky = new Color(0.95f, 0.92f, 0.86f),
        ambientEquator = new Color(0.78f, 0.74f, 0.70f),
        ambientGround = new Color(0.55f, 0.48f, 0.44f),
        fogColor = new Color(0.91f, 0.85f, 0.78f),
        fogStart = 40f,
        fogEnd = 120f
    };

    public PhaseLighting dusk = new PhaseLighting
    {
        sunColor = new Color(1.00f, 0.58f, 0.34f),
        sunIntensity = 0.95f,
        ambientSky = new Color(0.90f, 0.72f, 0.62f),
        ambientEquator = new Color(0.72f, 0.54f, 0.50f),
        ambientGround = new Color(0.45f, 0.34f, 0.36f),
        fogColor = new Color(0.88f, 0.66f, 0.55f),
        fogStart = 35f,
        fogEnd = 110f
    };

    public PhaseLighting evening = new PhaseLighting
    {
        sunColor = new Color(0.60f, 0.55f, 0.85f),
        sunIntensity = 0.45f,
        ambientSky = new Color(0.48f, 0.46f, 0.62f),
        ambientEquator = new Color(0.36f, 0.34f, 0.50f),
        ambientGround = new Color(0.24f, 0.22f, 0.36f),
        fogColor = new Color(0.44f, 0.36f, 0.52f),
        fogStart = 30f,
        fogEnd = 100f
    };

    public PhaseLighting night = new PhaseLighting
    {
        sunColor = new Color(0.42f, 0.52f, 0.95f),
        sunIntensity = 0.22f,
        ambientSky = new Color(0.20f, 0.21f, 0.40f),
        ambientEquator = new Color(0.15f, 0.16f, 0.32f),
        ambientGround = new Color(0.09f, 0.09f, 0.20f),
        fogColor = new Color(0.13f, 0.11f, 0.28f),
        fogStart = 25f,
        fogEnd = 90f
    };
}
