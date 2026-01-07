using UnityEngine;
using System;
using System.Collections.Generic;
using MyGame;

/// <summary>
/// 目标选择器 - 处理战斗中的目标选择
/// 使用 RangeVisualizer2D 显示攻击范围
/// </summary>
public class TargetSelector : MonoBehaviour
{
    public static TargetSelector Instance { get; private set; }

    [Header("状态")]
    public bool isSelectingTarget = false;
    public ICombatTarget selectedTarget;
    public Weapon selectedWeapon;

    [Header("视觉效果")]
    public GameObject targetIndicatorPrefab;
    public Color validTargetColor = Color.green;
    public Color invalidTargetColor = Color.red;
    public Color selectedColor = Color.yellow;

    [Header("引用")]
    public Player player;
    public BattleManager battleManager;
    public GridManager2D gridManager;
    public RangeVisualizer2D rangeVisualizer2D;  // 使用2D版本

    [Header("调试")]
    public bool debugMode = true;

    // 当前可攻击的目标列表
    private List<ICombatTarget> validTargets = new List<ICombatTarget>();

    // 事件
    public event Action<ICombatTarget> OnTargetSelected;
    public event Action OnSelectionCancelled;
    public event Action<ICombatTarget, Weapon> OnAttackConfirmed;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        FindReferences();
    }

    /// <summary>
    /// 查找所有引用
    /// </summary>
    private void FindReferences()
    {
        if (player == null)
            player = FindObjectOfType<Player>();

        if (battleManager == null)
            battleManager = FindObjectOfType<BattleManager>();

        if (gridManager == null)
            gridManager = FindObjectOfType<GridManager2D>();

        if (rangeVisualizer2D == null)
            rangeVisualizer2D = FindObjectOfType<RangeVisualizer2D>();

        Debug.Log($"[TargetSelector] 引用状态:");
        Debug.Log($"  - Player: {(player != null ? "OK" : "NULL")}");
        Debug.Log($"  - BattleManager: {(battleManager != null ? "OK" : "NULL")}");
        Debug.Log($"  - GridManager2D: {(gridManager != null ? "OK" : "NULL")}");
        Debug.Log($"  - RangeVisualizer2D: {(rangeVisualizer2D != null ? "OK" : "NULL")}");
    }

    private void Update()
    {
        if (isSelectingTarget)
        {
            HandleTargetSelection();
        }
    }

    /// <summary>
    /// 开始选择目标
    /// </summary>
    public void StartTargetSelection(Weapon weapon)
    {
        if (player == null || battleManager == null)
            FindReferences();

        if (weapon == null)
        {
            Debug.LogError("[TargetSelector] 没有选择武器！");
            return;
        }

        selectedWeapon = weapon;
        isSelectingTarget = true;
        selectedTarget = null;

        Debug.Log($"[TargetSelector] ===== 开始目标选择 =====");
        Debug.Log($"[TargetSelector] 武器: {weapon.Name}, 范围: {weapon.AttackRangeMin}-{weapon.AttackRangeMax}格");

        RefreshValidTargets();
        ShowAttackRange(weapon);
    }

    /// <summary>
    /// 显示攻击范围
    /// </summary>
    private void ShowAttackRange(Weapon weapon)
    {
        if (rangeVisualizer2D == null)
            rangeVisualizer2D = FindObjectOfType<RangeVisualizer2D>();

        if (rangeVisualizer2D == null)
        {
            Debug.LogWarning("[TargetSelector] RangeVisualizer2D 未找到！请在场景中添加此组件");
            return;
        }

        if (player == null) return;

        // 设置颜色
        if (weapon.AttackRangeMax <= 1)
            rangeVisualizer2D.SetMeleeColor();
        else
            rangeVisualizer2D.SetRangedColor();

        // 获取玩家位置（对齐格子）
        Vector3 centerPos = player.transform.position;
        if (gridManager != null)
        {
            Vector2 snapped = gridManager.SnapToGrid(player.transform.position);
            centerPos = new Vector3(snapped.x, snapped.y, 0);
        }

        rangeVisualizer2D.ShowRange(centerPos, weapon.AttackRangeMin, weapon.AttackRangeMax);
        Debug.Log($"[TargetSelector] 显示攻击范围，中心: {centerPos}");
    }

    /// <summary>
    /// 取消目标选择
    /// </summary>
    public void CancelSelection()
    {
        isSelectingTarget = false;
        selectedTarget = null;
        selectedWeapon = null;
        validTargets.Clear();

        if (rangeVisualizer2D != null)
            rangeVisualizer2D.HideRange();

        OnSelectionCancelled?.Invoke();
        Debug.Log("[TargetSelector] 目标选择已取消");
    }

    /// <summary>
    /// 处理目标选择
    /// </summary>
    private void HandleTargetSelection()
    {
        // 右键取消
        if (Input.GetMouseButtonDown(1))
        {
            CancelSelection();
            return;
        }

        // 左键选择
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            if (debugMode)
                Debug.Log($"[TargetSelector] 点击位置: {mouseWorldPos}");

            ICombatTarget clickedTarget = FindTargetAtPosition(mouseWorldPos);

            if (clickedTarget != null)
            {
                if (IsValidTarget(clickedTarget))
                {
                    SelectTarget(clickedTarget);
                }
                else
                {
                    Debug.Log($"[TargetSelector] 目标 {clickedTarget.Name} 不在攻击范围内");
                }
            }
            else if (debugMode)
            {
                Debug.Log("[TargetSelector] 没有点击到目标");
            }
        }
    }

    /// <summary>
    /// 在位置查找目标
    /// </summary>
    private ICombatTarget FindTargetAtPosition(Vector2 position)
    {
        // 方法1: 精确点击
        Collider2D[] hits = Physics2D.OverlapPointAll(position);
        ICombatTarget target = FindTargetInColliders(hits);
        if (target != null) return target;

        // 方法2: 范围检测
        hits = Physics2D.OverlapCircleAll(position, 0.5f);
        target = FindTargetInColliders(hits);
        if (target != null) return target;

        // 方法3: 遍历所有敌人
        return FindNearestEnemy(position, 1.0f);
    }

    /// <summary>
    /// 从碰撞体中找目标
    /// </summary>
    private ICombatTarget FindTargetInColliders(Collider2D[] colliders)
    {
        foreach (var hit in colliders)
        {
            ICombatTarget target = hit.GetComponent<ICombatTarget>();
            if (target == null) target = hit.GetComponentInParent<ICombatTarget>();
            if (target == null) target = hit.GetComponentInChildren<ICombatTarget>();

            if (target != null && !(target is Player))
            {
                if (debugMode) Debug.Log($"[TargetSelector] 找到: {target.Name}");
                return target;
            }
        }
        return null;
    }

    /// <summary>
    /// 查找最近的敌人
    /// </summary>
    private ICombatTarget FindNearestEnemy(Vector2 position, float maxDist)
    {
        ICombatTarget nearest = null;
        float nearestDist = maxDist;

        // 检查所有怪物类型
        foreach (var m in FindObjectsOfType<MonoBehaviour>())
        {
            ICombatTarget t = m as ICombatTarget;
            if (t == null || t is Player) continue;

            float dist = Vector2.Distance(position, (Vector2)m.transform.position);
            if (dist < nearestDist)
            {
                nearestDist = dist;
                nearest = t;
            }
        }

        return nearest;
    }

    /// <summary>
    /// 选中目标
    /// </summary>
    private void SelectTarget(ICombatTarget target)
    {
        selectedTarget = target;
        OnTargetSelected?.Invoke(target);
        Debug.Log($"[TargetSelector] ★ 选中: {target.Name}");
        ConfirmAttack();
    }

    /// <summary>
    /// 确认攻击
    /// </summary>
    public void ConfirmAttack()
    {
        if (selectedTarget == null || selectedWeapon == null)
        {
            Debug.LogError("[TargetSelector] 目标或武器为空！");
            return;
        }

        Debug.Log($"[TargetSelector] ★ 攻击: {selectedWeapon.Name} -> {selectedTarget.Name}");

        if (rangeVisualizer2D != null)
            rangeVisualizer2D.HideRange();

        ICombatTarget target = selectedTarget;
        Weapon weapon = selectedWeapon;

        isSelectingTarget = false;
        selectedTarget = null;
        selectedWeapon = null;
        validTargets.Clear();

        OnAttackConfirmed?.Invoke(target, weapon);
    }

    /// <summary>
    /// 刷新有效目标
    /// </summary>
    private void RefreshValidTargets()
    {
        validTargets.Clear();

        if (battleManager == null)
            battleManager = FindObjectOfType<BattleManager>();

        if (battleManager == null || selectedWeapon == null) return;

        var units = battleManager.GetUnits();

        foreach (var unit in units)
        {
            if (unit.isPlayer || unit.targetComponent == null) continue;

            if (IsInWeaponRange(unit.targetComponent))
            {
                validTargets.Add(unit.targetComponent);
                if (debugMode) Debug.Log($"[TargetSelector] 有效目标: {unit.targetComponent.Name}");
            }
        }

        Debug.Log($"[TargetSelector] 有效目标数: {validTargets.Count}");
    }

    /// <summary>
    /// 检查是否在武器范围内
    /// </summary>
    private bool IsInWeaponRange(ICombatTarget target)
    {
        if (player == null || selectedWeapon == null) return false;

        MonoBehaviour mono = target as MonoBehaviour;
        if (mono == null) return false;

        int dist;
        if (gridManager != null)
            dist = gridManager.GetGridDistance(player.transform.position, mono.transform.position);
        else
            dist = Mathf.RoundToInt(Vector2.Distance(player.transform.position, mono.transform.position));

        return dist >= selectedWeapon.AttackRangeMin && dist <= selectedWeapon.AttackRangeMax;
    }

    public bool IsValidTarget(ICombatTarget target)
    {
        if (validTargets.Count == 0 && selectedWeapon != null)
            RefreshValidTargets();
        return validTargets.Contains(target);
    }

    public int GetDistanceToTarget(ICombatTarget target)
    {
        if (player == null) return -1;
        MonoBehaviour mono = target as MonoBehaviour;
        if (mono == null) return -1;

        if (gridManager != null)
            return gridManager.GetGridDistance(player.transform.position, mono.transform.position);
        return Mathf.RoundToInt(Vector2.Distance(player.transform.position, mono.transform.position));
    }

    public List<ICombatTarget> GetValidTargets() => new List<ICombatTarget>(validTargets);
}
