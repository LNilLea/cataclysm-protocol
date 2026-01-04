namespace MyGame
{
    using UnityEngine;

    public class Player : MonoBehaviour, ICombatTarget
    {
        // 【已移除】不再需要 CharacterCreation 引用
        // public CharacterCreation characterCreation;

        // —— 战斗数据 —— 
        public PlayerCombatData combatData;

        // —— 当前武器 —— 
        public Weapon currentWeapon;

        // —— ICombatTarget 接口实现 —— 
        public string Name => "玩家";
        public int CurrentAC => combatData.CurrentAC;
        public int CurrentHP => combatData.currentHP;  // 【新增】大写版本 - 用于ICombatTarget接口
        public int currentHP => combatData.currentHP;  // 【保留】小写版本 - 保持向后兼容

        // —— 内部组件引用 —— 
        private CombatSystem combatSystem;
        private WeaponManager weaponManager;

        // —— 初始化 —— 
        private void Start()
        {
            // 获取组件
            combatSystem = GetComponent<CombatSystem>();
            weaponManager = GetComponent<WeaponManager>();

            // 【修改】检查是否已创建角色
            if (!CharacterData.IsCharacterCreated)
            {
                Debug.LogWarning("Player: 角色尚未创建，使用默认属性值！");
            }

            // 【修改】使用新的无参构造函数，从 CharacterData 静态类读取数据
            combatData = new PlayerCombatData();
            combatData.currentHP = combatData.maxHP;

            // 【重要】应用等级加成（从 GameProgressManager 获取）
            ApplyLevelBonus();

            // 初始化武器
            if (currentWeapon == null)
            {
                Debug.LogWarning("玩家没有装备武器。");
            }
            else
            {
                weaponManager.AddWeapon(currentWeapon);
            }

            // 绑定战斗系统
            if (combatSystem != null)
            {
                combatSystem.InitializeCombat(this);
            }

            Debug.Log($"玩家初始化完成 - HP: {combatData.currentHP}/{combatData.maxHP}, AC: {combatData.CurrentAC}");
        }

        // —— 应用等级加成 —— 
        private void ApplyLevelBonus()
        {
            if (GameProgressManager.Instance != null)
            {
                GameProgressManager.Instance.ApplyLevelBonusToPlayer(combatData);
                Debug.Log($"应用等级加成，当前等级: {GameProgressManager.Instance.playerLevel}");
            }
        }

        // —— 玩家受到伤害 —— 
        public void TakeDamage(int damage)
        {
            combatData.TakeDamage(ref damage);
            Debug.Log($"玩家受到 {damage} 点伤害，当前HP = {combatData.currentHP}/{combatData.maxHP}");

            // 检查死亡
            if (combatData.currentHP <= 0)
            {
                OnPlayerDeath();
            }
        }

        // —— 玩家死亡处理 —— 
        private void OnPlayerDeath()
        {
            Debug.Log("玩家死亡！");
            // 可以在这里添加死亡动画、音效等
        }

        // —— 玩家用当前武器攻击目标 —— 
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

        // —— 给玩家添加新武器（加入背包） —— 
        public void AddWeaponToInventory(Weapon weapon)
        {
            if (weapon == null) return;

            if (weaponManager != null)
            {
                weaponManager.AddWeapon(weapon);
            }

            // 如果当前没有武器，自动装备
            if (currentWeapon == null)
            {
                EquipWeapon(weapon);
            }

            Debug.Log($"玩家获得了新武器：{weapon.Name}");
        }

        // —— 装备武器 —— 
        public void EquipWeapon(Weapon weapon)
        {
            if (weapon == null) return;

            currentWeapon = weapon;

            // 更新战斗系统的当前武器
            if (combatSystem != null)
            {
                combatSystem.currentWeapon = weapon;
            }

            Debug.Log($"玩家装备了武器：{weapon.Name}");
        }

        // —— 获取攻击范围 —— 
        public float GetAttackRange()
        {
            if (currentWeapon != null)
            {
                // 如果武器有范围属性，可以在这里返回
                // return currentWeapon.Range;
            }
            return 1f;  // 默认攻击范围
        }

        // —— 治疗玩家 —— 
        public void Heal(int amount)
        {
            combatData.currentHP += amount;
            if (combatData.currentHP > combatData.maxHP)
            {
                combatData.currentHP = combatData.maxHP;
            }
            Debug.Log($"玩家恢复了 {amount} 点生命值，当前HP = {combatData.currentHP}/{combatData.maxHP}");
        }

        // —— 完全恢复 HP —— 
        public void FullHeal()
        {
            combatData.currentHP = combatData.maxHP;
            Debug.Log($"玩家完全恢复，当前HP = {combatData.currentHP}/{combatData.maxHP}");
        }

        // —— 获取当前等级 —— 
        public int GetLevel()
        {
            if (GameProgressManager.Instance != null)
            {
                return GameProgressManager.Instance.playerLevel;
            }
            return 1;
        }

        // —— 获取当前经验值 —— 
        public int GetCurrentExp()
        {
            if (GameProgressManager.Instance != null)
            {
                return GameProgressManager.Instance.currentExp;
            }
            return 0;
        }

        // —— 获取升级所需经验 —— 
        public int GetExpToNextLevel()
        {
            if (GameProgressManager.Instance != null)
            {
                return GameProgressManager.Instance.expToNextLevel;
            }
            return 100;
        }

        // —— 获取战斗数据（供外部调用） —— 
        public PlayerCombatData GetCombatData()
        {
            return combatData;
        }

        // —— 检查玩家是否存活 —— 
        public bool IsAlive()
        {
            return combatData.currentHP > 0;
        }
    }
}
