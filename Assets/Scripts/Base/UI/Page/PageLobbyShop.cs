using UnityEngine;
using BanpoFri;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;

[UIPath("UI/Page/PageLobbyShop")]
public class PageLobbyShop : CommonUIBase
{
    [SerializeField]
    private ShopArtifactGachaComponent ShopArtifactGachaComp;

    public void Init()
    {
        ShopArtifactGachaComp.Init();
        
    }
}

