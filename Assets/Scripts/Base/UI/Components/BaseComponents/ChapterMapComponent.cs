using UnityEngine;
using BanpoFri;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;

public class ChapterMapComponent : MonoBehaviour
{
    [SerializeField]
    private List<Image> UnitImgs = new List<Image>();

    [SerializeField]
    private Image MapImg;

    [SerializeField]
    private RectTransform MapGroupRect;



    int StageIdx = 0;

    public void Set(int stageidx)
    {
        StageIdx = stageidx;

        var td = Tables.Instance.GetTable<StageInfo>().GetData(stageidx);

        if (td != null)
        {
            MapGroupRect.anchoredPosition = new Vector2(MapGroupRect.anchoredPosition.x, td.map_unit_ypos);

            foreach (var unit in UnitImgs)
            {
                ProjectUtility.SetActiveCheck(unit.gameObject, false);
            }
        
            MapImg.sprite = AtlasManager.Instance.GetSprite(Atlas.Atlas_UI_Map, td.map_img);
            for (int i = 0; i < td.map_enemy_unit.Count; i++)
            {
                ProjectUtility.SetActiveCheck(UnitImgs[i].gameObject, true);
                UnitImgs[i].sprite = AtlasManager.Instance.GetSprite(Atlas.Atlas_UI_InGame, $"EnemyUnit_{td.map_enemy_unit[i]}");
            }
        }
    }
}
