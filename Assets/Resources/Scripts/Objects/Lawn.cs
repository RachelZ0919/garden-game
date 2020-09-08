using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lawn : MonoBehaviour
{
    private Vector3[] vertices;
    private Vector2[] uv;
    private Vector4[] tangents;
    private Color[] colors;
    private int[] triangles;
    private Texture2D mask;
    private Texture2D groundMask;
    private Mesh mesh;
    private int width, height;

    static public float segment = 5;
    static private int gridSize = 32;
    static private Shader lawnShader;
    static private Texture2D brushTexture;


    private void Awake()
    {
        if (lawnShader == null)
        {
            lawnShader = Resources.Load<Shader>("Shader/Lawn/LawnShader");
        }
        if (brushTexture == null)
        {
            brushTexture = Resources.Load<Texture2D>("Textures/brush");
            if (brushTexture == null)
            {
                Debug.LogError("Failed to load texture");
            }
        }
        GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
    }

    public void CreateMesh(Vector3 position, int width, int height, Vector3 normal)
    {
        int m_width = width * (int)segment;
        int m_height = height * (int)segment;
        Vector3 dirx, diry;
        Vector4 tangent = new Vector4(1, 0, 0, -1);
        this.width = width;
        this.height = height;

        vertices = new Vector3[(m_width + 1) * (m_height + 1)];
        uv = new Vector2[(m_width + 1) * (m_height + 1)];
        tangents = new Vector4[(m_width + 1) * (m_height + 1)];
        colors = new Color[(m_width + 1) * (m_height + 1)];
        triangles = new int[m_width * m_height * 6];

        if (normal == Vector3.up || normal == Vector3.down)
        {
            dirx = Vector3.right;
            diry = Vector3.forward;
            tangent = new Vector4(1, 0, 0, -1);
        }
        else if (normal == Vector3.forward || normal == Vector3.back)
        {
            dirx = Vector3.right;
            diry = Vector3.up;
            tangent = new Vector4(1, 0, 0, -1);
        }
        else
        {
            dirx = Vector3.up;
            diry = Vector3.forward;
            tangent = new Vector4(0, 1, 0, -1);
        }

        for (int j = 0, index = 0; j <= m_height; j++)
        {
            for (int i = 0; i <= m_width; i++, index++)
            {
                vertices[index] = position + 1 / segment * (i * dirx + j * diry);
                uv[index] = new Vector2((float)i / m_width, (float)j / m_height);
                tangents[index] = tangent;
                if (i  == 0 || j  == 0 || j == m_height || i == m_width)
                {
                    colors[index] = new Color(1, 1, 1, 1);
                }
                else
                {
                    colors[index] = new Color(1, 1, 1, 1);
                }
            }
        }

        if (normal == Vector3.down || normal == Vector3.forward || normal == Vector3.right)
        {
            for (int ti = 0, vi = 0, y = 0; y < m_height; y++, vi++)
            {
                for (int x = 0; x < m_width; x++, ti += 6, vi++)
                {
                    triangles[ti] = vi;
                    triangles[ti + 3] = triangles[ti + 1] = vi + 1;
                    triangles[ti + 5] = triangles[ti + 2] = vi + m_width + 1;
                    triangles[ti + 4] = vi + m_width + 2;
                }
            }
        }
        else
        {
            for (int ti = 0, vi = 0, y = 0; y < m_height; y++, vi++)
            {
                for (int x = 0; x < m_width; x++, ti += 6, vi++)
                {
                    triangles[ti] = vi;
                    triangles[ti + 3] = triangles[ti + 2] = vi + 1;
                    triangles[ti + 4] = triangles[ti + 1] = vi + m_width + 1;
                    triangles[ti + 5] = vi + m_width + 2;
                }
            }
        }

        mask = new Texture2D(width * gridSize, height * gridSize);
        for (int i = 0; i < width * gridSize; i++)
        {
            for (int j = 0; j < height * gridSize; j++)
            {
                mask.SetPixel(i, j, new Color(0, 0, 0, 0));
            }
        }
        mask.Apply();

        groundMask = new Texture2D(width * gridSize, height * gridSize);
        for(int i = 0;i < width * gridSize; i++)
        {
            for(int j = 0; j < height * gridSize; j++)
            {
                groundMask.SetPixel(i, j, new Color(0.3f, 0, 0, 0));
            }
        }
        groundMask.Apply();

        mesh = GetComponent<MeshFilter>().mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uv;
        mesh.tangents = tangents;
        mesh.colors = colors;
        mesh.RecalculateNormals();

        Material mat = GetComponent<MeshRenderer>().material = new Material(lawnShader);
        Texture blueLawnRamp = Resources.Load<Texture>("Textures/RampMap/BlueLawnRamp");
        Texture redLawnRamp = Resources.Load<Texture>("Textures/RampMap/RedLawnRamp");
        mat.SetTexture("_BlueLawnRamp", blueLawnRamp); 
        mat.SetTexture("_RedLawnRamp", redLawnRamp);
        mat.SetTexture("_SpawnMask", mask);
        mat.SetTexture("_GroundMask", groundMask);
    }

    public void DrawGridAtPosition(Vector2 uv, WaterColor waterColor, int state, bool right = false, bool left = false, bool forward = false, bool back = false)
    {
        Vector2 startPoint = new Vector2(Mathf.Floor(uv.x * width), Mathf.Floor(uv.y * height)) * gridSize;
        for (float i = 0; i < gridSize; i++)
        {
            for (float j = 0; j < gridSize; j++)
            {
                float px = i + startPoint.x;
                float py = j + startPoint.y;
                Color color = new Color(1, 1, 1, 1);
                if (waterColor == WaterColor.BLUE)
                {
                    color.b = 0;
                }
                else
                {
                    color.g = 0;
                }

                if (state <= 1)
                {
                    color.r = 0.3f;
                }
                else if (state == 2)
                {
                    color.r = 0.6f;
                }
                else
                {
                    color.r = 1f;
                }

                //if (color.a > 0)
                //{
                //    color.a = 1;
                //}
                groundMask.SetPixel((int)px, (int)py, color);
            }
        }
        groundMask.Apply();
        GetComponent<MeshRenderer>().material.SetTexture("_GroundMask", groundMask);
    }

    public void DrawAtPosition(Vector2 uv, WaterColor waterColor)
    {
        Color blackColor = new Color(0, 0, 0, 0);

        uv.x = uv.x * mask.width - 1;
        uv.y = uv.y * mask.height - 1;

        for (float i = 0; i < brushTexture.width; i++)
        {
            for (float j = 0; j < brushTexture.height; j++)
            {
                float px = i + uv.x - brushTexture.width / 2;
                float py = j + uv.y - brushTexture.height / 2;
                if (px < 0 || px >= mask.width || py < 0 || py >= mask.height) continue;
                Vector4 brushColor = brushTexture.GetPixel((int)i, (int)j) * Time.deltaTime * 7;

                if (waterColor == WaterColor.BLUE)
                {
                    brushColor.z *= -1;
                }
                else
                {
                    brushColor.y *= -1;
                }

                Color color = mask.GetPixel((int)px, (int)py);
                color.r += brushColor.x;
                color.g += brushColor.y;
                color.b += brushColor.z;
                color.a += brushColor.w;

                color.r = Mathf.Clamp(color.r, 0, 1);
                color.g = Mathf.Clamp(color.g, 0, 1);
                color.b = Mathf.Clamp(color.b, 0, 1);

                if (color.a > 0)
                {
                    color.a = 1;
                }

                mask.SetPixel((int)px, (int)py, color);
            }
        }
        mask.Apply();
        GetComponent<MeshRenderer>().material.SetTexture("_SpawnMask", mask);
    }

    public void FillAtPosition(Vector2 uv, WaterColor waterColor)
    {
        Vector2 startPoint = new Vector2(Mathf.Floor(uv.x * width), Mathf.Floor(uv.y * height)) * gridSize;
        for (float i = 0; i < gridSize; i++)
        {
            for (float j = 0; j < gridSize; j++)
            {
                float px = i + startPoint.x;
                float py = j + startPoint.y;
                Color color = new Color(1, 0, 0, 1);
                if (waterColor == WaterColor.BLUE)
                {
                    color.g = 1;
                }
                else
                {
                    color.b = 1;
                }
                mask.SetPixel((int)px, (int)py, color);
            }
        }
        mask.Apply();
        GetComponent<MeshRenderer>().material.SetTexture("_SpawnMask", mask);
    }

    public void ClearAtPosition(Vector2 uv)
    {
        Vector2 startPoint = new Vector2(Mathf.Floor(uv.x * width), Mathf.Floor(uv.y * height)) * gridSize;
        for (float i = 0; i < gridSize; i++)
        {
            for (float j = 0; j < gridSize; j++)
            {
                float px = i + startPoint.x;
                float py = j + startPoint.y;
                Color color = new Color(0, 0, 0, 0);
                groundMask.SetPixel((int)px, (int)py, color);
            }
        }
        groundMask.Apply();
        GetComponent<MeshRenderer>().material.SetTexture("_GroundMask", mask);

        for (float i = 0; i < gridSize; i++)
        {
            for (float j = 0; j < gridSize; j++)
            {
                float px = i + startPoint.x;
                float py = j + startPoint.y;
                Color color = new Color(0, 0, 0, 0);
                mask.SetPixel((int)px, (int)py, color);
            }
        }
        mask.Apply();
        GetComponent<MeshRenderer>().material.SetTexture("_SpawnMask", mask);
    }
}
