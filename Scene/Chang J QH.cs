using UnityEngine;
using UnityEngine.SceneManagement;  // 引用 Unity 的 SceneManager

public class GameManager : MonoBehaviour
{
    public SceneManager sceneMgr; // 使用 sceneMgr 代替 sceneManager
    public GameModeManager gameModeManager;
    public Character playerCharacter;

    void Start()
    {
        // 订阅模式变化事件
        gameModeManager.onModeChanged += HandleModeChanged;
    }

    // 切换到战斗模式并加载相应场景
    public void StartCombatMode(string combatSceneName)
    {
        // 切换到战斗模式
        gameModeManager.SwitchToCombatMode();

        // 加载战斗场景
        sceneMgr.LoadSceneAsync(combatSceneName);

        // 设置战斗模式下的格子大小
        sceneMgr.currentSceneSize = SceneSize.Medium;  // 例如设置为中场景

        // 更新角色的移动力
        playerCharacter.Move();  // 确保 Move() 是有效的方法
    }

    // 切换到探索模式并加载相应场景
    public void StartExplorationMode(string explorationSceneName)
    {
        // 切换到探索模式
        gameModeManager.SwitchToExplorationMode();

        // 加载探索场景
        sceneMgr.LoadSceneAsync(explorationSceneName);

        // 设置探索模式下的格子大小（如果需要）
        sceneMgr.currentSceneSize = SceneSize.Small;  // 例如设置为小场景

        // 更新角色的移动力
        playerCharacter.Move();  // 确保 Move() 是有效的方法
    }

    // 处理模式切换后的变化
    private void HandleModeChanged()
    {
        if (gameModeManager.currentMode == GameMode.Exploration)
        {
            // 在探索模式中，做一些与战斗模式不同的初始化
            playerCharacter.Move();  // 确保 Move() 是有效的方法
        }
        else if (gameModeManager.currentMode == GameMode.Combat)
        {
            // 在战斗模式中，准备战斗相关的设置
            playerCharacter.Move();  // 确保 Move() 是有效的方法
        }
    }
}
