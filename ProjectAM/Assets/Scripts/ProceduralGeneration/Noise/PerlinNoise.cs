using System;
using System.Text;
using UnityEngine;

public class PerlinNoise
{
    private const float NoiseAmplitude = 20.0f;
    private const float NoiseMaxValue =  9.0f;
    private const float NoiseMinValue = -9.0f;

    private NoiseSettings noiseSettings;
    private int[,] noiseValue;
    private Texture2D noiseTexture;

    public NoiseSettings NoiseSettings => noiseSettings;
    public int[,] NoiseValue => noiseValue;
    public Texture2D NoiseTexture => noiseTexture;

    public void GenerateNoise(NoiseSettings noiseSettings)
    {
        this.noiseSettings = noiseSettings;

        if (noiseSettings.resolution.x <= 0 || noiseSettings.resolution.y <= 0) return;

        int width = noiseSettings.resolution.x;
        int height = noiseSettings.resolution.y;

        noiseValue = new int[height, width];
        noiseTexture = new Texture2D(width, height);
        Color[] pixels = new Color[width * height];

        for(int row = 0; row < height; row++)
        {
            for(int col = 0; col < width; col++)
            {
                Vector2 uv = new Vector2(col, row) / noiseSettings.resolution * noiseSettings.gridSize;

                Vector2Int gridID = new Vector2Int
                {
                    x = Mathf.FloorToInt(uv.x),
                    y = Mathf.FloorToInt(uv.y)
                };

                Vector2 gridUV = new Vector2
                {
                    x = uv.x - gridID.x,
                    y = uv.y - gridID.y
                };

                Vector2Int topLeft = gridID + new Vector2Int(0, 0);
                Vector2Int topRight = gridID + new Vector2Int(1, 0);
                Vector2Int bottomLeft = gridID + new Vector2Int(0, 1);
                Vector2Int bottomRight = gridID + new Vector2Int(1, 1);

                Vector2 gradientTopLeft = GetRandomGradient(topLeft, noiseSettings.seed);
                Vector2 gradientTopRight = GetRandomGradient(topRight, noiseSettings.seed);
                Vector2 gradientBottomLeft = GetRandomGradient(bottomLeft, noiseSettings.seed);
                Vector2 gradientBottomRight = GetRandomGradient(bottomRight, noiseSettings.seed);

                Vector2 offsetTopLeft = gridUV - new Vector2(0, 0);
                Vector2 offsetTopRight = gridUV - new Vector2(1, 0);
                Vector2 offsetBottomLeft = gridUV - new Vector2(0, 1);
                Vector2 offsetBottomRight = gridUV - new Vector2(1, 1);

                float dotTopLeft = Vector2.Dot(gradientTopLeft, offsetTopLeft);
                float dotTopRight = Vector2.Dot(gradientTopRight, offsetTopRight);
                float dotBottomLeft = Vector2.Dot(gradientBottomLeft, offsetBottomLeft);
                float dotBottomRight = Vector2.Dot(gradientBottomRight, offsetBottomRight);

                Vector2 smoothUV;
                smoothUV.x = Mathf.SmoothStep(0f, 1f, gridUV.x);
                smoothUV.y = Mathf.SmoothStep(0f, 1f, gridUV.y);

                float interpolatedTop = Mathf.Lerp(dotTopLeft, dotTopRight, smoothUV.x);
                float interpolatedBottom = Mathf.Lerp(dotBottomLeft, dotBottomRight, smoothUV.x);
                float perlin = Mathf.Lerp(interpolatedTop, interpolatedBottom, smoothUV.y);
                perlin = Mathf.Clamp(perlin * NoiseAmplitude, NoiseMinValue, NoiseMaxValue);

                noiseValue[row, col] = Mathf.RoundToInt(perlin);
                pixels[row * width + col] = ToGrayscaleColor(perlin);
            }
        }

        noiseTexture.SetPixels(pixels);
        noiseTexture.Apply();
    }

    public void PrintNoise()
    {
        if (NoiseSettings.resolution.x <= 0 || NoiseSettings.resolution.y <= 0)
        {
            Debug.LogError($"Invalid noise resolution: {NoiseSettings.resolution}. Cannot print noise values.");
            return;
        }

        StringBuilder stringBuilder = new StringBuilder();
        
        for (int row = 0; row < noiseValue.GetLength(0); row++)
        {
            for (int col = 0; col < noiseValue.GetLength(1); col++)
            {
                int value = noiseValue[row, col];
                if (value >= 0)
                {
                    stringBuilder.Append(value);
                }
                else
                {
                    stringBuilder.Append(' ');
                }
            }

            stringBuilder.Append('\n');
        }

        Debug.Log(stringBuilder.ToString());
    }

    private Vector2 GetRandomGradient(Vector2Int gridID, float seed)
    {
        Vector2 adjustedPosition = new Vector2
        {
            x = gridID.x + 0.02f,
            y = gridID.y + 0.02f
        };

        float x = Vector2.Dot(adjustedPosition, new Vector2(123.4f, 234.5f));
        float y = Vector2.Dot(adjustedPosition, new Vector2(234.5f, 345.6f));

        Vector2 gradient = new Vector2(Mathf.Sin(x), Mathf.Sin(y)) * 43758.5453f;

        gradient.x = Mathf.Sin(gradient.x + seed);
        gradient.y = Mathf.Sin(gradient.y + seed);

        gradient.Normalize();

        return gradient;
    }

    private static Color ToGrayscaleColor(float perlin)
    {
        float value = Mathf.InverseLerp(NoiseMinValue, NoiseMaxValue, perlin);
        return new Color(value, value, value);
    }
}
