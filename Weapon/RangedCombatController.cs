using UnityEngine;
using System.Collections;
using MyGame;

/// <summary>
/// 枪械战斗控制器 - 处理枪械攻击、换弹、自伤等
/// </summary>
public class RangedCombatController : MonoBehaviour
{
    public static RangedCombatController Instance { get; private set; }

    [Header("引用")]
    public Player player;
    public BattleManager battleManager;
    public ActionPointSystem actionPointSystem;

    [Header("换弹设置")]
    public bool isReloading = false;
    public float currentReloadTime = 0f;

    // 事件
    public event System.Action<RangedAttackResult> OnRangedAttack;
    public event System.Action<RangedWeapon> OnReloadStart;
    public event System.Action<RangedWeapon> OnReloadComplete;
    public event System.Action<int> OnSelfDamage;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        if (player == null)
            player = FindObjectOfType<Player>();

        if (battleManager == null)
            battleManager = FindObjectOfType<BattleManager>();

        if (actionPointSystem == null)
            actionPointSystem = FindObjectOfType<ActionPointSystem>();
    }

    /// <summary>
    /// 执行枪械单发攻击
    /// </summary>
    public RangedAttackResult FireWeapon(RangedWeapon weapon, ICombatTarget target)
    {
        if (weapon == null || target == null)
        {
            return new RangedAttackResult
            {
                success = false,
                log = "无效的武器或目标"
            };
        }

        if (isReloading)
        {
            return new RangedAttackResult
            {
                success = false,
                log = "正在换弹中..."
            };
        }

        // 检查弹药
        if (!weapon.CanFire())
        {
            return new RangedAttackResult
            {
                success = false,
                log = $"{weapon.Name} 没有子弹！请换弹。"
            };
        }

        // 获取玩家属性
        int agility = player.combatData.agility;
        int strength = player.combatData.strength;

        // 执行射击
        RangedAttackResult result = weapon.FireSingle(target, agility, strength);

        // 处理自伤
        if (result.selfDamage > 0)
        {
            player.TakeDamage(result.selfDamage);
            OnSelfDamage?.Invoke(result.selfDamage);
        }

        OnRangedAttack?.Invoke(result);

        Debug.Log(result.log);
        return result;
    }

    /// <summary>
    /// 执行枪械连射攻击
    /// </summary>
    public RangedAttackResult FireBurst(RangedWeapon weapon, ICombatTarget target, int burstCount)
    {
        if (weapon == null || target == null)
        {
            return new RangedAttackResult
            {
                success = false,
                log = "无效的武器或目标"
            };
        }

        if (isReloading)
        {
            return new RangedAttackResult
            {
                success = false,
                log = "正在换弹中..."
            };
        }

        // 检查连射次数
        if (!weapon.CanBurst(burstCount))
        {
            int actualBurst = Mathf.Min(burstCount, weapon.MaxBurst, weapon.CurrentAmmo);
            if (actualBurst <= 0)
            {
                return new RangedAttackResult
                {
                    success = false,
                    log = $"{weapon.Name} 子弹不足！当前: {weapon.CurrentAmmo}"
                };
            }
            burstCount = actualBurst;
        }

        // 获取玩家属性
        int agility = player.combatData.agility;
        int strength = player.combatData.strength;

        // 执行连射
        RangedAttackResult result = weapon.FireBurst(target, burstCount, agility, strength);

        // 处理自伤
        if (result.selfDamage > 0)
        {
            player.TakeDamage(result.selfDamage);
            OnSelfDamage?.Invoke(result.selfDamage);
        }

        OnRangedAttack?.Invoke(result);

        Debug.Log(result.log);
        return result;
    }

    /// <summary>
    /// 开始换弹（消耗次要动作）
    /// </summary>
    public bool StartReload(RangedWeapon weapon)
    {
        if (weapon == null)
        {
            Debug.Log("没有装备枪械");
            return false;
        }

        if (isReloading)
        {
            Debug.Log("已经在换弹中");
            return false;
        }

        if (weapon.CurrentAmmo >= weapon.MaxAmmo)
        {
            Debug.Log("弹匣已满，无需换弹");
            return false;
        }

        if (weapon.ReserveAmmo <= 0)
        {
            Debug.Log("没有备用弹药！");
            return false;
        }

        // 消耗次要动作
        if (actionPointSystem != null && !actionPointSystem.CanDoMinorAction())
        {
            Debug.Log("没有次要动作点，无法换弹");
            return false;
        }

        if (actionPointSystem != null)
        {
            actionPointSystem.UseMinorAction();
        }

        // 开始换弹
        StartCoroutine(ReloadCoroutine(weapon));
        return true;
    }

    /// <summary>
    /// 换弹协程
    /// </summary>
    private IEnumerator ReloadCoroutine(RangedWeapon weapon)
    {
        isReloading = true;
        currentReloadTime = weapon.ReloadTime;

        OnReloadStart?.Invoke(weapon);
        Debug.Log($"开始换弹: {weapon.Name}，需要 {weapon.ReloadTime} 秒");

        // 等待换弹时间
        while (currentReloadTime > 0)
        {
            currentReloadTime -= Time.deltaTime;
            yield return null;
        }

        // 完成换弹
        weapon.Reload();
        isReloading = false;

        OnReloadComplete?.Invoke(weapon);
        Debug.Log($"换弹完成: {weapon.Name}，弹药: {weapon.GetAmmoStatus()}");
    }

    /// <summary>
    /// 取消换弹
    /// </summary>
    public void CancelReload()
    {
        if (isReloading)
        {
            StopAllCoroutines();
            isReloading = false;
            currentReloadTime = 0f;
            Debug.Log("换弹被取消");
        }
    }

    /// <summary>
    /// 检查武器是否是枪械
    /// </summary>
    public bool IsRangedWeapon(Weapon weapon)
    {
        return weapon is RangedWeapon;
    }

    /// <summary>
    /// 获取枪械（如果当前武器是枪械）
    /// </summary>
    public RangedWeapon GetCurrentRangedWeapon()
    {
        if (player != null && player.currentWeapon is RangedWeapon ranged)
        {
            return ranged;
        }
        return null;
    }

    /// <summary>
    /// 检查是否需要换弹
    /// </summary>
    public bool NeedsReload()
    {
        RangedWeapon weapon = GetCurrentRangedWeapon();
        return weapon != null && weapon.NeedsReload;
    }

    /// <summary>
    /// 获取换弹进度（0-1）
    /// </summary>
    public float GetReloadProgress()
    {
        if (!isReloading) return 1f;

        RangedWeapon weapon = GetCurrentRangedWeapon();
        if (weapon == null) return 1f;

        return 1f - (currentReloadTime / weapon.ReloadTime);
    }
}
