using UnityEngine;
using UnityEngine.UI;

public class UITileComponent : MonoBehaviour
{
    enum moveDir
    {
        LeftTop_To_RightBottom,
        RightTop_To_LeftBottom,
        LeftBottom_To_RightTop,
        RightBottom_To_LeftlTop,
        Right_To_Left,
        Left_To_Right,
    }

    [SerializeField]
    private float moveSpeed = 0.1f;
    [SerializeField]
    private moveDir direction;

    private Material mat = null;
    private Image targetImg = null;
    // Start is called before the first frame update

    void Start()
    {
        targetImg = GetComponent<Image>();
        if (targetImg != null)
        {
            mat = targetImg.material;
            mat.SetTextureOffset("_MainTex", Vector2.zero);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (targetImg.materialForRendering != targetImg.material)
        {
            mat = targetImg.materialForRendering;
        }

        float offset = Time.unscaledTime * moveSpeed;
        Vector2 moveOffset;
        if (direction == moveDir.LeftTop_To_RightBottom) moveOffset = new Vector2(-offset, offset);
        else if (direction == moveDir.RightTop_To_LeftBottom) moveOffset = new Vector2(offset, offset);
        else if (direction == moveDir.LeftBottom_To_RightTop) moveOffset = new Vector2(-offset, -offset);
        else if (direction == moveDir.Right_To_Left) moveOffset = new Vector2(offset, 0);
        else if (direction == moveDir.Left_To_Right) moveOffset = new Vector2(-offset, 0);
        else moveOffset = new Vector2(offset, -offset);

        mat.SetTextureOffset("_MainTex", moveOffset);
    }

    private void OnApplicationQuit() => ResetOffset();
    private void OnDestroy() => ResetOffset();
    private void OnDisable() => ResetOffset();

    private void ResetOffset()
    {
        if (targetImg == null) return;
        targetImg.material.SetTextureOffset("_MainTex", Vector2.zero);
    }
}
