public interface ICombatTarget
{
    string Name { get; }
    int CurrentAC { get; }
    int CurrentHP { get; }  // 新增：当前生命值属性
    void TakeDamage(int damage);
}
