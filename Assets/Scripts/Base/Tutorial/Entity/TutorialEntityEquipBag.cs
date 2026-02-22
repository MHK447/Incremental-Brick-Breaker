using UnityEngine;
using BanpoFri;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;
using System;
using DG.Tweening;
public class TutorialEntityEquipBag : TutorialEntity
{
    [Serializable]
    public enum MoveType
    {
        FirstEquip,
        SecondEquip,
        FirstItemMerge,
        SecondItemMerge,
        TileEquip,
        TwoStageTileEquip
    }

    [SerializeField]
    private GameObject ClickObj;

    [SerializeField]
    private MoveType Type;

    override public void StartEntity()
    {
        base.StartEntity();

        DOTween.Kill(ClickObj.transform);

        switch (Type)
        {
            case MoveType.FirstEquip:
                {
                    var starttr = GameRoot.Instance.UISystem.GetUI<PopupInGame>().TileWeaponGroup.GetTileWeaponComponentList[0].transform;

                    var gettile = GameRoot.Instance.UISystem.GetUI<PopupInGame>().TileWeaponGroup.GetTileComponent(6);

                    ClickObj.transform.position = starttr.position;

                    ClickObj.transform.DOMove(gettile.transform.position, 1.2f).SetEase(Ease.OutCubic).SetLoops(-1, LoopType.Restart);
                }
                break;
            case MoveType.SecondEquip:
                {

                    var starttr = GameRoot.Instance.UISystem.GetUI<PopupInGame>().TileWeaponGroup.GetTileWeaponComponentList[2].transform;

                    var gettile = GameRoot.Instance.UISystem.GetUI<PopupInGame>().TileWeaponGroup.GetTileComponent(1);

                    ClickObj.transform.position = starttr.position;

                    ClickObj.transform.DOMove(gettile.transform.position, 1.2f).SetEase(Ease.OutCubic).SetLoops(-1, LoopType.Restart);
                    break;
                }
            case MoveType.FirstItemMerge:
                {
                    var tileweapongroup = GameRoot.Instance.UISystem.GetUI<PopupInGame>().TileWeaponGroup;

                    var gettileweapon = tileweapongroup.GetTileWeaponComponentList[0];

                    var target = tileweapongroup.GetTileWeaponComponentList.FirstOrDefault(x => x.EquipIdx == gettileweapon.EquipIdx
                    && !x.IsEquip);


                    ClickObj.transform.position = target.transform.position;

                    ClickObj.transform.DOMove(gettileweapon.transform.position, 1.2f).SetEase(Ease.OutCubic).SetLoops(-1, LoopType.Restart);
                    break;
                }
            case MoveType.SecondItemMerge:
                {
                    var tileweapongroup = GameRoot.Instance.UISystem.GetUI<PopupInGame>().TileWeaponGroup;

                    var gettileweapon = tileweapongroup.GetTileWeaponComponentList[1];

                    var target = tileweapongroup.GetTileWeaponComponentList.FirstOrDefault(x => x.EquipIdx == gettileweapon.EquipIdx
                    && !x.IsEquip);


                    ClickObj.transform.position = target.transform.position;

                    ClickObj.transform.DOMove(gettileweapon.transform.position, 1.2f).SetEase(Ease.OutCubic).SetLoops(-1, LoopType.Restart);
                    break;
                }
            case MoveType.TileEquip:
                {
                    var tileweapongroup = GameRoot.Instance.UISystem.GetUI<PopupInGame>().TileWeaponGroup;

                    var gettileadd = tileweapongroup.GetTileAddComponentList.Find(x => x.EquipIdx == 1002);


                    var gettile = GameRoot.Instance.UISystem.GetUI<PopupInGame>().TileWeaponGroup.GetTileComponent(29);

                    ClickObj.transform.position = gettileadd.transform.position;

                    ClickObj.transform.DOMove(gettile.transform.position, 1.2f).SetEase(Ease.OutCubic).SetLoops(-1, LoopType.Restart);
                }
                break;
            case MoveType.TwoStageTileEquip:
                {
                    var starttr = GameRoot.Instance.UISystem.GetUI<PopupInGame>().TileWeaponGroup.GetTileWeaponComponentList[1].transform;

                    var gettile = GameRoot.Instance.UISystem.GetUI<PopupInGame>().TileWeaponGroup.GetTileComponent(29);

                    ClickObj.transform.position = starttr.position;

                    ClickObj.transform.DOMove(gettile.transform.position, 1.2f).SetEase(Ease.OutCubic).SetLoops(-1, LoopType.Restart);
                }
                break;
        }

        GameRoot.Instance.StartCoroutine(WailtEquip());
    }

    public IEnumerator WailtEquip()
    {
        var tileWeaponGroup = GameRoot.Instance.UISystem.GetUI<PopupInGame>().TileWeaponGroup;

        switch (Type)
        {
            case MoveType.FirstEquip:
                yield return new WaitUntil(() =>
                {
                    var list = tileWeaponGroup.GetTileWeaponComponentList;
                    return list != null && list.Count > 0 && list[0].IsEquip;
                });
                break;
            case MoveType.SecondEquip:
                yield return new WaitUntil(() =>
                {
                    var list = tileWeaponGroup.GetTileWeaponComponentList;
                    return list != null && list.Count > 2 && list[2].IsEquip;
                });
                break;
            case MoveType.FirstItemMerge:
                yield return new WaitUntil(() =>
                {
                    var list = tileWeaponGroup.GetTileWeaponComponentList;
                    return list != null && list.Count > 0 && list[0].Grade == 2;
                });
                break;
            case MoveType.SecondItemMerge:
                yield return new WaitUntil(() =>
                {
                    var list = tileWeaponGroup.GetTileWeaponComponentList;
                    return list != null && list.Count > 1 && list[1].Grade == 2;
                });
                break;
            case MoveType.TileEquip:
                yield return new WaitUntil(() =>
                {
                    var gettileadd = tileWeaponGroup.GetTileAddComponentList?.Find(x => x.EquipIdx == 1002);
                    return gettileadd == null || !gettileadd.gameObject.activeSelf;
                });
                break;
            case MoveType.TwoStageTileEquip:
                yield return new WaitUntil(() =>
                {
                    var list = tileWeaponGroup.GetTileWeaponComponentList;
                    return list != null && list.Count > 1 && list[1].IsEquip;
                });
                break;
        }


        Done();
    }

    void Update()
    {
        var gettilegroup = GameRoot.Instance.UISystem.GetUI<PopupInGame>()?.TileWeaponGroup;

        if (gettilegroup != null)
        {
            ProjectUtility.SetActiveCheck(ClickObj, !gettilegroup.IsWeaponHolding);
        }
    }

    protected override void Done()
    {
        base.Done();
    }
}

