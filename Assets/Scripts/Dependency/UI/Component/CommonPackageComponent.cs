using UnityEngine;
using BanpoFri;
using UnityEngine.UI;
using TMPro;

public class CommonPackageComponent : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI PriceText;
    [SerializeField]
    private TextMeshProUGUI OriginalPriceText;
    [SerializeField]
    private TextMeshProUGUI DiscountText;

    public void Set(PackageType packageType)
    {
        var td = Tables.Instance?.GetTable<ShopProduct>()?.GetData((int)packageType);
        if (td == null)
            return;

        var product = GameRoot.Instance?.InAppPurchaseManager?.GetProduct(td.product_id);
        var priceString = product?.metadata?.localizedPriceString ?? string.Empty;
        var originalCost = (product != null && GameRoot.Instance?.ShopSystem != null)
            ? GameRoot.Instance.ShopSystem.OriginalPackageCost(td.idx)
            : string.Empty;

        if (PriceText != null) PriceText.text = priceString;
        if (OriginalPriceText != null) OriginalPriceText.text = originalCost;
        if (DiscountText != null) DiscountText.text = td.sale.ToString() + "%";
    }
}