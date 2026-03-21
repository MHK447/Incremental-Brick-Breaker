using UnityEngine;
using BanpoFri;
using DG.Tweening;

public class EnemyBlockSpawner : MonoBehaviour
{
    [SerializeField]
    private SpriteRenderer BlockSpawnImg;
    private int SpawnIdx = 0;
    private int Hp = 0;
    private int MaxHp = 0;

    private InGameHpProgress HpProgress;
    private bool HpProgressLoading = false;

    public bool IsDead { get { return Hp <= 0; } }

    public void Set(int spawnidx , int hp)
    {
        SpawnIdx = spawnidx;

        Hp = hp;
        MaxHp = hp;

        transform.DOKill(true);
        transform.localScale = Vector3.one;
        if (BlockSpawnImg != null)
        {
            BlockSpawnImg.DOKill();
            var c = BlockSpawnImg.color;
            c.a = 1f;
            BlockSpawnImg.color = c;
        }

        BlockSpawnImg.sprite = AtlasManager.Instance.GetSprite(Atlas.Atlas_InGame, $"EnemyUnitSpawner_{SpawnIdx}");

        EnsureHpProgress();
    }

    private void BindHpProgress()
    {
        if (HpProgress == null || MaxHp <= 0)
            return;

        HpProgress.Init(transform);
        ProjectUtility.SetActiveCheck(HpProgress.gameObject, true);
        HpProgress.SetHpText(Hp, MaxHp);
    }

    private void EnsureHpProgress()
    {
        if (HpProgress != null)
        {
            BindHpProgress();
            return;
        }

        if (HpProgressLoading || MaxHp <= 0)
            return;

        HpProgressLoading = true;
        GameRoot.Instance.UISystem.LoadFloatingUI<InGameHpProgress>(ui =>
        {
            HpProgressLoading = false;
            if (ui == null)
                return;
            if (this == null)
            {
                ui.Hide();
                return;
            }

            HpProgress = ui;
            BindHpProgress();
        }, false);
    }

    public virtual void Damage(int damage)
    {
        if (Hp <= 0)
            return;

        Hp -= Mathf.Max(0, damage);
        transform.DOPunchScale(Vector3.one * 0.14f, 0.16f, 6, 0.45f);

        if (HpProgress != null && MaxHp > 0)
            HpProgress.SetHpText(Hp, MaxHp);

        if (Hp <= 0)
        {
            transform.DOKill(true);
            GetComponentInParent<EnemyUnitGroup>()?.OnBlockSpawnerDestroyed();

            transform.localScale = Vector3.one;
            transform.DOScale(0f, 0.24f).SetEase(Ease.InBack)
                .OnComplete(() => ProjectUtility.SetActiveCheck(gameObject, false));

            GameRoot.Instance.InGameSystem.GetInGame<InGameBase>().Stage.StopWave();
            GameRoot.Instance.InGameSystem.GetInGame<InGameBase>().Stage.StartRest();
        }
    }

    public void ClearData()
    {
        transform.DOKill(true);
        if (BlockSpawnImg != null)
            BlockSpawnImg.DOKill();
        if (HpProgress != null)
            HpProgress.Hide();
        HpProgressLoading = false;
        Hp = 0;
        MaxHp = 0;
        SpawnIdx = 0;
    }

    private void OnDestroy()
    {
        if (HpProgress != null)
            HpProgress.Hide();
    }
}

