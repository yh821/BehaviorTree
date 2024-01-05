---
--- Created by Hugo
--- DateTime: 2023/5/7 19:58
---

---@class IsInViewNode : ConditionNode
---@field target DrawObj
IsInViewNode = IsInViewNode or BaseClass(ConditionNode)

function IsInViewNode:Start()
    if self.view_range_sqrt then
        return
    end
    local range = self.data and self.data[BtConfig.view_range_key]
    range = range or self:GetSharedVar(BtConfig.view_range_key) or 5
    self.view_range_sqrt = range * range
end

function IsInViewNode:Tick(delta_time)
    self:Start()
    local target_pos = self.owner:GetTargetMoveObjPos()
    local self_pos = self.owner:GetSelfMoveObjPos()
    if target_pos and self_pos then
        local dist_sqrt = Vector3.DistanceSqrt(target_pos, self_pos)
        return dist_sqrt <= self.view_range_sqrt and eNodeState.Success or eNodeState.Failure
    end
    return eNodeState.Failure
end