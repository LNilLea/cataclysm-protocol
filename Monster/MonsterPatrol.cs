using UnityEngine;
using System.Collections.Generic;
using MyGame;

/// <summary>
/// 怪物巡逻和索敌组件 - 挂载在每个怪物上
/// </summary>
public class MonsterPatrol : MonoBehaviour
{
    [Header("索敌设置")]
    public float detectionRange = 8f;           // 索敌范围（360度）
    public float attackRange = 2f;              // 攻击范围（触发偷袭）
    public LayerMask playerLayer;               // 玩家层

    [Header("巡逻设置")]
    public bool enablePatrol = true;            // 是否启用巡逻
    public PatrolType patrolType = PatrolType.Waypoints;
    public float patrolSpeed = 2f;              // 巡逻速度
    public float waitTimeAtPoint = 2f;          // 在巡逻点等待时间

    [Header("巡逻路径（Waypoints模式）")]
    public Transform[] waypoints;               // 巡逻点
    private int currentWaypointIndex = 0;

    [Header("巡逻范围（Random模式）")]
    public float randomPatrolRadius = 5f;       // 随机巡逻半径
    private Vector3 randomTarget;
    private Vector3 startPosition;

    [Header("状态")]
    public bool isPlayerDetected = false;       // 是否发现玩家
    public bool isInCombat = false;             // 是否在战斗中
    public MonsterState currentState = MonsterState.Patrolling;

    [Header("调试")]
    public bool showGizmos = true;
    public Color detectionColor = new Color(1, 0, 0, 0.3f);
    public Color patrolColor = Color.blue;

    // 内部状态
    private float waitTimer = 0f;
    private Transform playerTransform;
    private Player playerComponent;

    // 事件
    public event System.Action<MonsterPatrol> OnPlayerDetected;     // 发现玩家
    public event System.Action<MonsterPatrol> OnPlayerLost;         // 丢失玩家
    public event System.Action<MonsterPatrol> OnAmbushAttack;       // 偷袭攻击

    private void Start()
    {
        startPosition = transform.position;

        // 查找玩家
        playerComponent = FindObjectOfType<Player>();
        if (playerComponent != null)
        {
            playerTransform = playerComponent.transform;
        }

        // 初始化随机巡逻目标
        if (patrolType == PatrolType.Random)
        {
            SetNewRandomTarget();
        }
    }

    private void Update()
    {
        // 战斗中不执行巡逻逻辑
        if (isInCombat) return;

        // 检测玩家
        DetectPlayer();

        // 执行当前状态行为
        switch (currentState)
        {
            case MonsterState.Patrolling:
                Patrol();
                break;
            case MonsterState.Chasing:
                ChasePlayer();
                break;
            case MonsterState.Waiting:
                Wait();
                break;
        }
    }

    /// <summary>
    /// 检测玩家
    /// </summary>
    private void DetectPlayer()
    {
        if (playerTransform == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

        // 在索敌范围内
        if (distanceToPlayer <= detectionRange)
        {
            // 检查视线是否被遮挡
            if (!IsLineOfSightBlocked(playerTransform.position))
            {
                if (!isPlayerDetected)
                {
                    isPlayerDetected = true;
                    OnPlayerDetectedHandler();
                }

                // 检查是否可以偷袭（在攻击范围内且玩家没看到怪物）
                if (distanceToPlayer <= attackRange)
                {
                    CheckAmbush();
                }
            }
        }
        else
        {
            if (isPlayerDetected)
            {
                isPlayerDetected = false;
                OnPlayerLostHandler();
            }
        }
    }

    /// <summary>
    /// 检查视线是否被遮挡
    /// </summary>
    private bool IsLineOfSightBlocked(Vector3 targetPosition)
    {
        Vector3 direction = targetPosition - transform.position;
        float distance = direction.magnitude;

        // 从怪物眼睛高度发射射线
        Vector3 eyePosition = transform.position + Vector3.up * 1f;

        if (Physics.Raycast(eyePosition, direction.normalized, distance, ~playerLayer))
        {
            return true; // 被遮挡
        }

        return false;
    }

    /// <summary>
    /// 检查是否可以偷袭
    /// </summary>
    private void CheckAmbush()
    {
        if (PlayerVision.Instance == null) return;

        // 如果怪物在玩家背后（玩家看不到怪物）
        if (PlayerVision.Instance.IsBehindPlayer(transform.position))
        {
            // 触发偷袭
            OnAmbushAttack?.Invoke(this);
            Debug.Log($"{gameObject.name} 偷袭玩家！");
        }
        else
        {
            // 玩家看到了怪物，正常触发战斗
            OnPlayerDetected?.Invoke(this);
        }
    }

    /// <summary>
    /// 发现玩家处理
    /// </summary>
    private void OnPlayerDetectedHandler()
    {
        Debug.Log($"{gameObject.name} 发现玩家！");

        // 如果玩家也看到了怪物，正常进入战斗
        if (PlayerVision.Instance != null && PlayerVision.Instance.IsMonsterInSight(gameObject))
        {
            OnPlayerDetected?.Invoke(this);
        }
        else
        {
            // 玩家没看到，怪物开始接近准备偷袭
            currentState = MonsterState.Chasing;
        }
    }

    /// <summary>
    /// 丢失玩家处理
    /// </summary>
    private void OnPlayerLostHandler()
    {
        Debug.Log($"{gameObject.name} 丢失玩家视野");
        currentState = MonsterState.Patrolling;
        OnPlayerLost?.Invoke(this);
    }

    /// <summary>
    /// 巡逻行为
    /// </summary>
    private void Patrol()
    {
        if (!enablePatrol) return;

        switch (patrolType)
        {
            case PatrolType.Waypoints:
                PatrolWaypoints();
                break;
            case PatrolType.Random:
                PatrolRandom();
                break;
            case PatrolType.Stationary:
                // 原地不动，只转向
                break;
        }
    }

    /// <summary>
    /// 路径点巡逻
    /// </summary>
    private void PatrolWaypoints()
    {
        if (waypoints == null || waypoints.Length == 0) return;

        Transform targetWaypoint = waypoints[currentWaypointIndex];
        Vector3 targetPos = targetWaypoint.position;

        // 移动向目标点
        MoveTowards(targetPos);

        // 到达目标点
        if (Vector3.Distance(transform.position, targetPos) < 0.5f)
        {
            currentState = MonsterState.Waiting;
            waitTimer = waitTimeAtPoint;
        }
    }

    /// <summary>
    /// 随机巡逻
    /// </summary>
    private void PatrolRandom()
    {
        MoveTowards(randomTarget);

        // 到达目标点
        if (Vector3.Distance(transform.position, randomTarget) < 0.5f)
        {
            currentState = MonsterState.Waiting;
            waitTimer = waitTimeAtPoint;
        }
    }

    /// <summary>
    /// 追逐玩家（准备偷袭）
    /// </summary>
    private void ChasePlayer()
    {
        if (playerTransform == null) return;

        MoveTowards(playerTransform.position);
    }

    /// <summary>
    /// 等待状态
    /// </summary>
    private void Wait()
    {
        waitTimer -= Time.deltaTime;

        if (waitTimer <= 0)
        {
            currentState = MonsterState.Patrolling;

            // 切换到下一个巡逻点
            if (patrolType == PatrolType.Waypoints)
            {
                currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
            }
            else if (patrolType == PatrolType.Random)
            {
                SetNewRandomTarget();
            }
        }
    }

    /// <summary>
    /// 移动向目标位置
    /// </summary>
    private void MoveTowards(Vector3 targetPosition)
    {
        Vector3 direction = (targetPosition - transform.position).normalized;
        direction.y = 0; // 保持在地面上

        // 移动
        transform.position += direction * patrolSpeed * Time.deltaTime;

        // 转向
        if (direction != Vector3.zero)
        {
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                Quaternion.LookRotation(direction),
                Time.deltaTime * 5f
            );
        }
    }

    /// <summary>
    /// 设置新的随机巡逻目标
    /// </summary>
    private void SetNewRandomTarget()
    {
        Vector2 randomCircle = Random.insideUnitCircle * randomPatrolRadius;
        randomTarget = startPosition + new Vector3(randomCircle.x, 0, randomCircle.y);
    }

    /// <summary>
    /// 进入战斗状态
    /// </summary>
    public void EnterCombat()
    {
        isInCombat = true;
        currentState = MonsterState.InCombat;
    }

    /// <summary>
    /// 退出战斗状态
    /// </summary>
    public void ExitCombat()
    {
        isInCombat = false;
        isPlayerDetected = false;
        currentState = MonsterState.Patrolling;
    }

    /// <summary>
    /// 获取到玩家的距离
    /// </summary>
    public float GetDistanceToPlayer()
    {
        if (playerTransform == null) return float.MaxValue;
        return Vector3.Distance(transform.position, playerTransform.position);
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (!showGizmos) return;

        // 绘制索敌范围
        Gizmos.color = detectionColor;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        // 绘制攻击范围
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        // 绘制巡逻路径
        if (patrolType == PatrolType.Waypoints && waypoints != null && waypoints.Length > 1)
        {
            Gizmos.color = patrolColor;
            for (int i = 0; i < waypoints.Length; i++)
            {
                if (waypoints[i] != null)
                {
                    Gizmos.DrawSphere(waypoints[i].position, 0.3f);
                    if (i < waypoints.Length - 1 && waypoints[i + 1] != null)
                    {
                        Gizmos.DrawLine(waypoints[i].position, waypoints[i + 1].position);
                    }
                }
            }
            // 连接最后一个和第一个
            if (waypoints[waypoints.Length - 1] != null && waypoints[0] != null)
            {
                Gizmos.DrawLine(waypoints[waypoints.Length - 1].position, waypoints[0].position);
            }
        }

        // 绘制随机巡逻范围
        if (patrolType == PatrolType.Random)
        {
            Gizmos.color = patrolColor;
            Vector3 center = Application.isPlaying ? startPosition : transform.position;
            Gizmos.DrawWireSphere(center, randomPatrolRadius);
        }
    }
#endif
}

/// <summary>
/// 巡逻类型
/// </summary>
public enum PatrolType
{
    Waypoints,      // 路径点巡逻
    Random,         // 随机巡逻
    Stationary      // 原地不动
}

/// <summary>
/// 怪物状态
/// </summary>
public enum MonsterState
{
    Patrolling,     // 巡逻中
    Chasing,        // 追逐中
    Waiting,        // 等待中
    InCombat        // 战斗中
}
