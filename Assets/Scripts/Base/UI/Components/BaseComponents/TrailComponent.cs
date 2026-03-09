using UnityEngine;
using BanpoFri;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;

public class TrailComponent : MonoBehaviour
{
    private static Material SharedTrailMaterial;

    private TrailRenderer trail;

    public Color TrailColor = Color.white;

    private static Material GetSharedMaterial()
    {
        if (SharedTrailMaterial != null) return SharedTrailMaterial;

        Shader shader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
        if (shader == null)
            shader = Shader.Find("Sprites/Default");

        if (shader == null)
        {
            Debug.LogError("Trail shader not found!");
            return null;
        }

        SharedTrailMaterial = new Material(shader);
        SharedTrailMaterial.SetFloat("_Surface", 1);
        SharedTrailMaterial.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        SharedTrailMaterial.SetFloat("_Blend", 0);
        SharedTrailMaterial.SetColor("_BaseColor", Color.white);
        SharedTrailMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        SharedTrailMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        SharedTrailMaterial.SetInt("_ZWrite", 0);
        SharedTrailMaterial.renderQueue = 3000;
        return SharedTrailMaterial;
    }

    public void InitTrail(float bulletSpeed = 12f)
    {
        if (trail == null)
        {
            trail = gameObject.AddComponent<TrailRenderer>();
        }

        float targetTrailLength = 3f;
        trail.time = targetTrailLength / Mathf.Max(bulletSpeed, 1f);
        
        trail.startWidth = 0.05f;
        trail.endWidth = 0.05f;
        trail.sortingOrder = 300;
        trail.numCornerVertices = 3;
        trail.numCapVertices = 3;
        trail.minVertexDistance = 0.05f;

        var mat = GetSharedMaterial();
        if (mat != null)
        {
            trail.sharedMaterial = mat;
        }

        var colorGradient = new Gradient();
        colorGradient.SetKeys(
            new[] {     
                new GradientColorKey(TrailColor, 0.0f),
                new GradientColorKey(TrailColor, 0.3f),
                new GradientColorKey(Color.clear, 1.0f) 
            },
            new[] { 
                new GradientAlphaKey(0.8f, 0.0f),
                new GradientAlphaKey(0.5f, 0.5f),
                new GradientAlphaKey(0.0f, 1.0f) 
            }
        );
        trail.colorGradient = colorGradient;
    }


    public void SetTrailActive(bool active)
    {
        if (trail != null)
        {
            trail.enabled = active;
        }
    }

    public void ClearTrail()
    {
        if (trail != null)
        {
            trail.Clear(); // 이전 트레일 데이터 제거
        }
    }

    /// <summary> 트레일이 자연스럽게 사라지는 데 걸리는 시간 (히트 후 비활성화 지연용) </summary>
    public float GetTrailTime()
    {
        return trail != null ? trail.time : 0f;
    }
}

