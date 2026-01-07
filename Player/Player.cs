namespace MyGame
{
    using UnityEngine;

    /// <summary>
    /// 玩家类 - 战斗系统核心
    /// </summary>
    public class Player : MonoBehaviour, ICombatTarget
    {
        // —— 战斗数据 —— 
        public PlayerCombatData combatData;

        // —— 当前武器 —— 
        public Weapon currentWeapon;

        // —— ICombatTarget 接口实现 —— 
        public string Name => "玩家";
        public int CurrentAC => combatData?.CurrentAC ?? 10;
        public int CurrentHP => combatData?.currentHP ?? 0;
        public int currentHP => combatData?.currentHP ?? 0;

        // —— 内部组件引用 —— 
        private CombatSystem combatSystem;
        private WeaponManager weaponManager;

        // —— 初始化 —— 
        private void Start()
        {
            // 获取组件
            combatSystem = GetComponent<CombatSystem>();
            weaponManager = GetComponent<WeaponManager>();

            // 检查是否已创建角色
            if (!CharacterData.IsCharacterCreated)
            {
                Debug.LogWarning("[Player] 角色尚未创建，使用默认属性值！");
            }

            // 创建战斗数据（从 CharacterData 静态类读取）
            combatData = new PlayerCombatData();
            
            // 确保HP有效
            EnsureValidHP();

            // 应用等级加成
            ApplyLevelBonus();

            // 再次确保HP有效
            EnsureValidHP();

            // 初始化武器
            if (currentWeapon == null)
            {
                Debug.LogWarning("[Player] 没有装备武器");
            }
            else
            {
                weaponManager?.AddWeapon(currentWeapon);
            }

            // 绑定战斗系统
            if (combatSystem != null)
            {
                combatSystem.InitializeCombat(this);
            }

            Debug.Log($"[Player] 初始化完成 - HP:{combatData.currentHP}/{combatData.maxHP}, AC:{combatData.CurrentAC}, 体魄:{combatData.strength}");
        }

        /// <summary>
        /// 确保HP有效
        /// </summary>
        private void EnsureValidHP()
        {
            if (combatData == null) return;

            // 如果maxHP无效，根据体魄重新计算
            if (combatData.maxHP <= 0)
            {
                combatData.maxHP = combatData.strength * 5;
                
                // 如果还是0，使用默认值
                if (combatData.maxHP <= 0)
                {
                    combatData.maxHP = 15;  // 默认值（3 * 5）
                    Debug.LogWarning("[Player] maxHP计算为0，使用默认值15");
                }
            }

            // 确保currentHP有效
            if (combatData.currentHP <= 0)
            {
                combatData.currentHP = combatData.maxHP;
                Debug.LogWarning($"[Player] currentHP为0，重置为 {combatData.currentHP}");
            }
        }

        /// <summary>
        /// 应用等级加成
        /// </summary>
        private void ApplyLevelBonus()
        {
            if (GameProgressManager.Instance != null)
            {
                GameProgressManager.Instance.ApplyLevelBonusToPlayer(combatData);
                Debug.Log($"[Player] 应用等级加成，当前等级: {GameProgressManager.Instance.playerLevel}");
            }
        }

        /// <summary>
        /// 玩家受到伤害
        /// </summary>
        public void TakeDamage(int damage)
        {
            if (combatData == null) return;
            
            combatData.TakeDamage(ref damage);
            Debug.Log($"[Player] 受到 {damage} 点伤害，当前HP = {combatData.currentHP}/{combatData.maxHP}");

            if (combatData.currentHP <= 0)
            {
                OnPlayerDeath();
            }
        }

        /// <summary>
        /// 玩家死亡处理
        /// </summary>
        private void OnPlayerDeath()
        {
            Debug.Log("[Player] 玩家死亡！");
        }

        /// <summary>
        /// 玩家用当前武器攻击目标
        /// </summary>
        public void UseCurrentWeaponOnTarget(ICombatTarget target, out string log)
        {
            if (combatSystem != null)
            {
                combatSystem.UseWeaponOnTarget(target, this, out log);
            }
            else
            {
                log = "错误：没有战斗系统组件！";
                Debug.LogError(log);
            }
        }

        /// <summary>
        /// 给玩家添加新武器
        /// </summary>
        public void AddWeaponToInventory(Weapon weapon)
        {
            if (weapon == null) return;

            weaponManager?.AddWeapon(weapon);

            if (currentWeapon == null)
            {
                EquipWeapon(weapon);
            }

            Debug.Log($"[Player] 获得了新武器：{weapon.Name}");
        }

        /// <summary>
        /// 装备武器
        /// </summary>
        public void EquipWeapon(Weapon weapon)
        {
            if (weapon == null) return;

            currentWeapon = weapon;

            if (combatSystem != null)
            {
                combatSystem.currentWeapon = weapon;
            }

            Debug.Log($"[Player] 装备了武器：{weapon.Name}");
        }

        /// <summary>
        /// 获取攻击范围
        /// </summary>
        public float GetAttackRange()
        {
            return 1f;
        }

        /// <summary>
        /// 治疗玩家
        /// </summary>
        public void Heal(int amount)
        {
            if (combatData == null) return;
            
            combatData.currentHP += amount;
            if (combatData.currentHP > combatData.maxHP)
            {
                combatData.currentHP = combatData.maxHP;
            }
            Debug.Log($"[Player] 恢复了 {amount} 点生命值，当前HP = {combatData.currentHP}/{combatData.maxHP}");
        }

        /// <summary>
        /// 完全恢复 HP
        /// </summary>
        public void FullHeal()
        {
            if (combatData == null) return;
            
            combatData.currentHP = combatData.maxHP;
            Debug.Log($"[Player] 完全恢复，当前HP = {combatData.currentHP}/{combatData.maxHP}");
        }

        /// <summary>
        /// 获取当前等级
        /// </summary>
        public int GetLevel()
        {
            return GameProgressManager.Instance?.playerLevel ?? 1;
        }

        /// <summary>
        /// 获取当前经验值
        /// </summary>
        public int GetCurrentExp()
        {
            return GameProgressManager.Instance?.currentExp ?? 0;
        }

        /// <summary>
        /// 获取升级所需经验
        /// </summary>
        public int GetExpToNextLevel()
        {
            return GameProgressManager.Instance?.expToNextLevel ?? 100;
        }

        /// <summary>
        /// 获取战斗数据
        /// </summary>
        public PlayerCombatData GetCombatData()
        {
            return combatData;
        }

        /// <summary>
        /// 检查玩家是否存活
        /// </summary>
        public bool IsAlive()
        {
            return combatData != null && combatData.currentHP > 0;
        }
    }
}
