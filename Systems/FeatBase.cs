/// <summary>
/// 专长基类 - 所有专长的抽象基类
/// </summary>
public abstract class FeatBase
{
    public string featName;

    // ===== 被动加值（子类可重写） =====
    
    /// <summary>
    /// 伤害加值
    /// </summary>
    public virtual int DamageBonus => 0;
    
    /// <summary>
    /// 命中加值
    /// </summary>
    public virtual int HitBonus => 0;
    
    /// <summary>
    /// 先攻加值（用于先攻轴计算）
    /// </summary>
    public virtual int InitiativeBonus => 0;
    
    /// <summary>
    /// AC加值（防御加值）
    /// </summary>
    public virtual int ACBonus => 0;

    // ===== 战斗事件回调 =====
    
    /// <summary>
    /// 战斗开始时触发
    /// </summary>
    public virtual void OnBattleStart(PlayerCombatData player) { }

    /// <summary>
    /// 回合开始时触发
    /// </summary>
    public virtual void OnTurnStart(PlayerCombatData player) { }

    /// <summary>
    /// 回合结束时触发
    /// </summary>
    public virtual void OnTurnEnd(PlayerCombatData player) { }

    /// <summary>
    /// 玩家造成伤害时触发（可修改伤害值）
    /// </summary>
    public virtual void OnPlayerDealDamage(PlayerCombatData player, ref int damage) { }

    /// <summary>
    /// 玩家受到伤害时触发（可修改伤害值）
    /// </summary>
    public virtual void OnPlayerTakeDamage(PlayerCombatData player, ref int damage) { }
    
    /// <summary>
    /// 玩家攻击命中时触发
    /// </summary>
    public virtual void OnPlayerHit(PlayerCombatData player) { }
    
    /// <summary>
    /// 玩家攻击未命中时触发
    /// </summary>
    public virtual void OnPlayerMiss(PlayerCombatData player) { }
    
    /// <summary>
    /// 玩家被攻击时触发（命中检定前）
    /// </summary>
    public virtual void OnPlayerBeingAttacked(PlayerCombatData player, ref int enemyHitRoll) { }
}
