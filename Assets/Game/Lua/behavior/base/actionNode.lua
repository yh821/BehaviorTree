---
--- Created by Hugo
--- DateTime: 2023/4/2 13:11
---

---@class ActionNode : TaskNode
ActionNode = BaseClass(TaskNode)

function ActionNode:IsAction()
    return true
end

function ActionNode:OnSuccess()
    self:SetState(eNodeState.Success)
end

function ActionNode:OnRunning()
    self:SetState(eNodeState.Running)
end

function ActionNode:OnFailure()
    self:SetState(eNodeState.Failure)
end