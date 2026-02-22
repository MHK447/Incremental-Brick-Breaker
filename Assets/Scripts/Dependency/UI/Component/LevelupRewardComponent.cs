using UnityEngine;
using BanpoFri;
using TMPro;
using UnityEngine.UI;
using DG.Tweening;
using System.Linq;

public class LevelupRewardComponent : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI ChoiceDesc;


    [SerializeField]
    private TextMeshProUGUI UpgradeNameText;

    [SerializeField]
    private Image BackImg;

    [SerializeField]
    private Image FrontImg;


    [SerializeField]
    private Button ChoiceBtn;



    [SerializeField]
    private GameObject RecommendLabel;

    [SerializeField]
    private Image[] BlockLevelImages = new Image[3];

    [SerializeField]
    private Image[] BlockRootLevelImages = new Image[3];

    [SerializeField]
    private GameObject BlockLevelGroup;


    private System.Action<InGameUpgrade> OnSelectCallback;

    private InGameUpgrade Upgrade;


    private Sprite BlinkEmptySprite;
    private Sprite BlinkFilledSprite;
    private bool DoBlink;

    void Awake()
    {
        if (ChoiceBtn != null)
            ChoiceBtn.onClick.AddListener(OnClickChoice);
    }



    public void Set(InGameUpgrade upgrade, System.Action<InGameUpgrade> onselectcallback)
    {
        Upgrade = upgrade;

        OnSelectCallback = onselectcallback;

        if (upgrade == null || upgrade.UpgradeChoiceData == null) return;

        FrontImg.sprite = AtlasManager.Instance.GetSprite(Atlas.Atlas_UI_Upgrade, upgrade.UpgradeChoiceData.front_img);
        BackImg.sprite = AtlasManager.Instance.GetSprite(Atlas.Atlas_UI_Upgrade, upgrade.UpgradeChoiceData.back_img);

        bool recommended = Upgrade.IsRecommend;

        ProjectUtility.SetActiveCheck(RecommendLabel, recommended);



        UpgradeNameText.text = Tables.Instance.GetTable<Localize>().GetString(upgrade.UpgradeChoiceData.choice_name);
        try
        {
            ChoiceDesc.text = upgrade.GetDesc();
        }
        catch (System.Exception)
        {
            ChoiceDesc.text = "";
        }

        SetBlockUpgradeImages();
    }




    public void OnClickChoice()
    {
        OnSelectCallback?.Invoke(Upgrade);
    }


    private void SetBlockUpgradeImages()
    {
        if (Upgrade == null || Upgrade.UpgradeChoiceData == null) return;

        DoBlink = true;
        BlinkEmptySprite = AtlasManager.Instance.GetSprite(Atlas.Atlas_UI_Common, "Common_Icon_LevelupEmpty");
        BlinkFilledSprite = AtlasManager.Instance.GetSprite(Atlas.Atlas_UI_Common, "Common_Icon_LevelupFill");


        //set level dot
        ProjectUtility.SetActiveCheck(BlockLevelGroup, true);


        int blockLevel = GameRoot.Instance.InGameUpgradeSystem.GetUpgradeLevel(Upgrade.UpgradeChoiceData.idx);
        int safeBlockLevel = Mathf.Min(blockLevel, BlockLevelImages.Length - 1);

        for (int i = 0; i < BlockRootLevelImages.Length; ++i)
        {
            ProjectUtility.SetActiveCheck(BlockRootLevelImages[i].gameObject, Upgrade.UpgradeChoiceData.upgrade_count > i);
        }

        for (int i = 0; i < BlockLevelImages.Length; i++)
        {
            BlockLevelImages[i].DOKill();
            if (i < blockLevel) BlockLevelImages[i].color = Color.white;
            else BlockLevelImages[i].color = Color.white.WithAlpha(0);
        }

        if (safeBlockLevel >= 0 && safeBlockLevel < BlockLevelImages.Length)
        {
            BlockLevelImages[safeBlockLevel].DOFade(1, 0.5f)
                .SetEase(Ease.InOutCubic)
                .SetUpdate(true)
                .SetLoops(-1, LoopType.Yoyo);
        }
    }
}
