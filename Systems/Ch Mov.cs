using UnityEngine;

public class Character : MonoBehaviour
{
    public float moveSpeed = 5f;

    private Vector3 targetPosition;
    private GameModeManager gameModeManager;

    void Start()
    {
        gameModeManager = FindObjectOfType<GameModeManager>();
        if (gameModeManager != null)
            gameModeManager.onModeChanged += HandleModeChanged;

        targetPosition = transform.position;
    }

    void Update()
    {
        // 如果没有GameModeManager，默认使用探索模式移动
        if (gameModeManager == null)
        {
            HandleExplorationMovement();
            return;
        }

        if (gameModeManager.currentMode == GameMode.Exploration)
        {
            HandleExplorationMovement();
        }
        else if (gameModeManager.currentMode == GameMode.Combat)
        {
            HandleCombatMovement();
        }
    }

    public void Move()
    {
        Debug.Log("Move() 被调用");
    }

    // 探索模式 WASD 控制（2D：X-Y平面）
    void HandleExplorationMovement()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        // 2D移动：X-Y平面（不是X-Z）
        Vector3 moveDir = new Vector3(h, v, 0).normalized;
        transform.Translate(moveDir * moveSpeed * Time.deltaTime, Space.World);
    }

    // 战斗模式 鼠标点击移动（2D版本）
    void HandleCombatMovement()
    {
        if (Input.GetMouseButtonDown(0))
        {
            // 2D游戏用这个方式获取鼠标位置
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            targetPosition = new Vector3(mousePos.x, mousePos.y, transform.position.z);
        }

        transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
    }

    void HandleModeChanged()
    {
        Debug.Log("Game Mode Changed: " + gameModeManager.currentMode);
        // 切换模式时重置目标位置
        targetPosition = transform.position;
    }
}
