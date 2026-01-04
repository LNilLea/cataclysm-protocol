using MyGame;

public class BattleUnit
{
    public string name;
    public ICombatTarget targetComponent;
    public IMobAction actionComponent;
    public int initiativePerRound;
    public int gauge;
    public bool isPlayer;

    public BattleUnit(string name, int initiativePerRound, ICombatTarget target, IMobAction action, bool isPlayer = false)
    {
        this.name = name;
        this.initiativePerRound = initiativePerRound;
        this.targetComponent = target;
        this.actionComponent = action;
        this.isPlayer = isPlayer;
        gauge = 0;
    }

    // 删除这个空的构造函数：
    // public BattleUnit(string v1, object initiative, Player player, IMobAction mobAction, bool v2)
    // {
    // }
}