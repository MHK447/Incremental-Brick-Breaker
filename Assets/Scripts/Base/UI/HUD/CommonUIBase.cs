using BanpoFri;
using System.Collections.Generic;

public class CommonUIBase : UIBase
{

    public override void CurrencyTopShow()
    {
        if(ResetCurrency && UIType != UIBaseType.OverlayPopup) GameRoot.Instance.CurrencyTop.SetCurrencyState(CurrencyToShow);
    }

}
