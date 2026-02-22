using UnityEngine;
using BanpoFri;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;
using TMPro;

public class ShopPackageComponent : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI SaleText;


    [SerializeField]
    private TextMeshProUGUI OriginalPriceText;

    [SerializeField]
    private TextMeshProUGUI PriceText;


    [SerializeField]
    private Button PurchaseBtn;

    [SerializeField]
    private PackageType PackageType;

    void Awake()
    {
        PurchaseBtn.onClick.AddListener(OnClickPurchase);
    }


    void OnEnable()
    {
        Init();
    }



    public void Init()
    {
        var td = Tables.Instance.GetTable<ShopProduct>().GetData((int)PackageType);

        if (td != null)
        {
            if (GameRoot.Instance.UserData.BuyInappIds.Contains(td.product_id) || GameRoot.Instance.UserData.Herogroudata.Equipplayeridx <= 0)
            {
                ProjectUtility.SetActiveCheck(this.gameObject, false);
            }

            var product = GameRoot.Instance.InAppPurchaseManager.GetProduct(td.product_id);

            SaleText.text = $"{td.sale}%";
            OriginalPriceText.text = GameRoot.Instance.ShopSystem.OriginalPackageCost((int)PackageType);
            PriceText.text = product.metadata.localizedPriceString;
        }
    }


    public void OnClickPurchase()
    {
        GameRoot.Instance.ShopSystem.PurchasePackage((int)PackageType, ()=> { ProjectUtility.SetActiveCheck(this.gameObject , false); }, InAppPurchaseLocation.hud, null);
    }

}

