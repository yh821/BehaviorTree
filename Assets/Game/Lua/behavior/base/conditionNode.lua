---
--- Created by Hugo
--- DateTime: 2023/4/1 16:52
---

---@class ConditionNode : TaskNode
ConditionNode = BaseClass(TaskNode)

function ConditionNode:IsCondition()
    return true
end