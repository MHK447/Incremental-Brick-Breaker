using UnityEngine;
using BanpoFri;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;

public class TrailComponent : MonoBehaviour
{
    private TrailRenderer trail;
    

    public void InitTrail(Color trailColor, float bulletSpeed = 12f)
    {
        if (trail == null)
        {
            trail = gameObject.AddComponent<TrailRenderer>();
        }

        // 속도에 반비례하게 trail time 조정 (느린 발사체는 더 긴 시간)
        // 기준: speed 12 = 0.25초
        float targetTrailLength = 3f; // 원하는 트레일 길이 (유닛 단위)
        trail.time = targetTrailLength / Mathf.Max(bulletSpeed, 1f);  // 속도가 낮을수록 time이 길어짐
        
        trail.startWidth = 0.25f;  // 더 얇은 시작 너비
        trail.endWidth = 0.05f;  // 완전히 0이 아닌 아주 얇은 끝부분
        trail.sortingOrder = 50;
        trail.numCornerVertices = 3;  // 코너를 부드럽게
        trail.numCapVertices = 3;  // 끝부분을 부드럽게
        trail.minVertexDistance = 0.05f;  // 더 자연스러운 곡선
        
        
        // URP 호환 셰이더로 변경
        Shader shader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
        if (shader == null)
            shader = Shader.Find("Sprites/Default"); // 폴백
        
        if (shader != null)
        {
            trail.material = new Material(shader);
            trail.material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            trail.material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.One); // Additive
            trail.material.SetInt("_ZWrite", 0);
            trail.material.renderQueue = 3000;
        }
        else
        {
            Debug.LogError("Trail shader not found! Please check if URP shaders are available.");
        }

        // 더 부드러운 페이드 아웃 그라디언트
        var colorGradient = new Gradient();
        colorGradient.SetKeys(
            new[] {     
                new GradientColorKey(trailColor, 0.0f),
                new GradientColorKey(trailColor, 0.3f),
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


    void OnDestroy()
    {
        if (trail != null && trail.material != null)
        {
            Destroy(trail.material);
        }
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


}

