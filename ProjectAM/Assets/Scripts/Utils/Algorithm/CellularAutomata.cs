using System;

public class CellularAutomata
{
    private CellularAutomataSettings settings;
    private float[,] cellularMap;
    private float[,] tempMap;

    public CellularAutomataSettings CellularAutomataSettings => settings;
    public float[,] CellularMap => cellularMap;

    public float[,] GenerateCellularMap(CellularAutomataSettings settings)
    {
        this.settings = settings;

        cellularMap = new float[settings.resolution.y, settings.resolution.x];
        tempMap = new float[settings.resolution.y, settings.resolution.x];
        
        FillRandom(cellularMap, settings.fillPercentage);

        int range = settings.neighborRange;
        int neighborThreshold = 2 * range * (range + 1);

        for (int i=0; i<settings.smoothIterations; i++)
        {
            Smooth(cellularMap, range, neighborThreshold);
        }

        for (int i=0; i<settings.blurIterations; i++)
        {
            Blur(cellularMap, range);
        }

        return cellularMap;
    }

    private void FillRandom(float[,] source, int fillPercentage)
    {
        int height = source.GetLength(0);
        int width = source.GetLength(1);

        for(int row = 1; row < height - 1; row++)
        {
            for(int col = 1; col < width - 1; col++)
            {
                int random = UnityEngine.Random.Range(0, 100);
                source[row, col] = (random < fillPercentage) ? 1.0f : 0.0f;
            }
        }
    }

    private void Smooth(float[,] source, int range = 1, int neighborThreshold = 4)
    {
        int height = source.GetLength(0);
        int width = source.GetLength(1);

        Array.Copy(source, tempMap, source.Length);

        for (int row = 1; row < height - 1; row++)
        {
            for (int col = 1; col < width - 1; col++)
            {
                int neighborCount = CountNeighbors(tempMap, row, col, range);
                
                if(neighborCount > neighborThreshold)
                {
                    source[row, col] = 1.0f;
                }
                else if (neighborCount < neighborThreshold)
                {
                    source[row, col] = 0.0f;
                }
            }
        }
    }

    private void Blur(float[,] source, int range = 1)
    {
        int height = source.GetLength(0);
        int width = source.GetLength(1);

        Array.Copy(source, tempMap, source.Length);

        for (int row = 1; row < height - 1; row++)
        {
            for (int col = 1; col < width - 1; col++)
            {
                float neighborSum = SumNeighbors(tempMap, row, col, range);
                int adjCellCount = CountAdjCells(tempMap, row, col, range);

                source[row, col] = (tempMap[row, col] + neighborSum) / (1 + adjCellCount);
            }
        }
    }

    private int CountNeighbors(float[,] source, int row, int col, int range = 1)
    {
        int count = 0;

        int height = source.GetLength(0);
        int width = source.GetLength(1);

        for (int dr = -range; dr <= range; dr++)
        {
            for(int dc = -range; dc <= range; dc++)
            {
                if(dr == 0 && dc == 0) continue;

                int neighborRow = row + dr;
                int neighborCol = col + dc;

                if (neighborRow < 0 || neighborCol < 0 || neighborRow >= height || neighborCol >= width) continue;
                if (source[neighborRow, neighborCol] > 0.0f) count++;
            }
        }

        return count;
    }

    private float SumNeighbors(float[,] source, int row, int col, int range = 1)
    {
        float sum = 0;

        int height = source.GetLength(0);
        int width = source.GetLength(1);

        for (int dr = -range; dr <= range; dr++)
        {
            for (int dc = -range; dc <= range; dc++)
            {
                if (dr == 0 && dc == 0) continue;

                int neighborRow = row + dr;
                int neighborCol = col + dc;

                if (neighborRow < 0 || neighborCol < 0 || neighborRow >= height || neighborCol >= width) continue;
                
                sum += source[neighborRow, neighborCol];
            }
        }

        return sum;
    }

    private int CountAdjCells(float[,] source, int row, int col, int range = 1)
    {
        int count = 0;

        int height = source.GetLength(0);
        int width = source.GetLength(1);

        for (int dr = -range; dr <= range; dr++)
        {
            for (int dc = -range; dc <= range; dc++)
            {
                if (dr == 0 && dc == 0) continue;

                int neighborRow = row + dr;
                int neighborCol = col + dc;

                if (neighborRow < 0 || neighborCol < 0 || neighborRow >= height || neighborCol >= width) continue;

                count++;
            }
        }
        return count;
    }
}