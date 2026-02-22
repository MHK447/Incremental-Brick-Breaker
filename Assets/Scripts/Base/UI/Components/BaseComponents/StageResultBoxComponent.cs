using UnityEngine;
using BanpoFri;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;
using TMPro;
public class StageResultBoxComponent : MonoBehaviour
{
    [SerializeField]
    private Image BoxImg;

    [SerializeField]
    private Image BoxFxGlow;

    [SerializeField]
    private TextMeshProUGUI BoxNameText;
    

    [SerializeField]
    private List<GameObject> BoxFxList = new List<GameObject>();

    public void Set(int boxidx)
    {
        var td = Tables.Instance.GetTable<RewardBoxInfo>().GetData(boxidx);

        if(td != null)
        {
            BoxImg.sprite = AtlasManager.Instance.GetSprite(Atlas.Atlas_UI_Common, td.image);
            BoxFxGlow.color = Config.Instance.GetImageColor($"box_grade_{td.box_idx}");
            BoxNameText.text = Tables.Instance.GetTable<Localize>().GetString(td.name);




            BoxNameText.color = Config.Instance.GetBoxGradeTextColor(td.box_idx);
            

            foreach(var fx in BoxFxList)
            {
                ProjectUtility.SetActiveCheck(fx, false);
            }


            ProjectUtility.SetActiveCheck(BoxFxList[td.box_idx - 2], true);
        }
    }


}

