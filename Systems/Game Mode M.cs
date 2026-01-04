using UnityEngine;

public enum GameMode
{
    Exploration,  // 探索模式
    Combat        // 战斗模式
}

public class GameModeManager : MonoBehaviour
{
    public GameMode currentMode = GameMode.Exploration;  // 默认为探索模式

    // 定义事件，当模式变化时触发
    public event System.Action onModeChanged;

    // 切换到战斗模式
    public void SwitchToCombatMode()
    {
        currentMode = GameMode.Combat;
        onModeChanged?.Invoke();  // 触发模式变化事件
        Debug.Log("已切换到战斗模式");
    }

    // 切换到探索模式
    public void SwitchToExplorationMode()
    {
        currentMode = GameMode.Exploration;
        onModeChanged?.Invoke();  // 触发模式变化事件
        Debug.Log("已切换到探索模式");
    }
}

