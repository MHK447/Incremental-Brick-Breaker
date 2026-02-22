using UnityEngine;
using BanpoFri;
using System.Collections.Generic;
using System.Linq;

public class InGameDamageEffect : MonoBehaviour
{
    public List<SpriteRenderer> SolidSpriteList;

    public List<SpriteRenderer> UnitSpriteList = new List<SpriteRenderer>();

    private List<Material> SolidMaterialList = new List<Material>();

    void Awake()
    {
        foreach (var sprite in SolidSpriteList)
        {
            if (sprite == null) continue;

            Material newMaterial = new Material(Config.Instance.SolidMat);

            sprite.sharedMaterial = newMaterial;

            SolidMaterialList.Add(newMaterial);

            ProjectUtility.SetActiveCheck(sprite.gameObject, true);

            newMaterial.SetFloat("_SelfIllum", 1f);
            Color currentColor = newMaterial.GetColor("_Color");
            currentColor.a = 0f;
            newMaterial.SetColor("_Color", currentColor);

        }
    }



    void OnDestroy()
    {
        foreach (var mat in SolidMaterialList)
        {
            if (mat != null)
                Destroy(mat);
        }
        SolidMaterialList.Clear();
    }

    public void Init()
    {

        foreach (var sprite in UnitSpriteList)
        {
            if (sprite == null) continue;

            sprite.sharedMaterial = Config.Instance.DefaultMat;
        }


        foreach (var solid in SolidSpriteList)
        {
            if (solid == null) continue;
            ProjectUtility.SetActiveCheck(solid.gameObject, false);
        }


    }


    private Coroutine _damageCoroutine = null;

    public void DamageColorEffect()
    {
        if (_damageCoroutine != null)
        {
            if (GameRoot.Instance != null)
                GameRoot.Instance.StopCoroutine(_damageCoroutine);
            _damageCoroutine = null;
        }

        foreach (var solidobj in SolidSpriteList)
        {
            if (solidobj == null) continue;
            ProjectUtility.SetActiveCheck(solidobj.gameObject, true);
        }
        foreach (var unitobj in UnitSpriteList)
        {
            if (unitobj == null) continue;
            ProjectUtility.SetActiveCheck(unitobj.gameObject, false);
        }

        foreach (var mat in SolidMaterialList)
        {
            Color currentColor = mat.GetColor("_Color");
            currentColor.a = 0.7f;
            mat.SetColor("_Color", currentColor);
        }

        _damageCoroutine = GameRoot.Instance.WaitTimeAndCallback(0.1f, () =>
        {
            _damageCoroutine = null;
            if (this != null)
            {
                foreach (var solidobj in SolidSpriteList)
                {
                    if (solidobj == null) continue;
                    ProjectUtility.SetActiveCheck(solidobj.gameObject, false);
                }
                foreach (var unitobj in UnitSpriteList)
                {
                    if (unitobj == null) continue;
                    ProjectUtility.SetActiveCheck(unitobj.gameObject, true);
                }

                foreach (var mat in SolidMaterialList)
                {
                    Color currentColor = mat.GetColor("_Color");
                    currentColor.a = 0f;
                    mat.SetColor("_Color", currentColor);
                }
            }
        });
    }

}

