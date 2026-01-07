using MyGame;
using UnityEngine;

public interface IMobAction
{
    void Move();                         // 移动方法
    float GetAttackRange();              // 获取攻击范围
    int GetInitiative();                 // 获取先攻值
    string PerformAction(Player player); // 执行行动（攻击 / 技能）
  

    Transform transform { get; }         // 允许节点访问 transform
}
