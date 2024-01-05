---
--- Created by Hugo
--- DateTime: 2020/12/07 16:59
---

---@class RandomPositionNode : ActionNode
RandomPositionNode = BaseClass(ActionNode)

function RandomPositionNode:Start()
    local pos = self:GetRandomPos()
    if self:IsCanMove(pos) then
        self:SetSharedVar(BtConfig.target_pos_key, pos)
        --self:print("随机位置" .. pos:ToString())
        return eNodeState.Success
    else
        self:print("随机位置不可行走")
    end
    return eNodeState.Failure
end

function RandomPositionNode:GetRandomPos()
    local center_pos
    local center_str = self.data and self.data.center
    if not IsNilOrEmpty(center_str) then
        center_pos = BehaviorManager.ParseVector3(center_str, true)
    end
    if not center_pos then
        center_pos = self:GetSharedVar(self.data and self.data.pos)
        center_pos = self.owner:GetSelfMoveObjPos()
    end
    if center_pos then
        local range = self.data and self.data.range or 10
        local x = math.random(center_pos.x - range, center_pos.x + range)
        local z = math.random(center_pos.z - range, center_pos.z + range)
        return Vector3.New(x, 0, z)
    end
end

function RandomPositionNode:IsCanMove(pos)
    if not pos then
        return false
    end
    if pos.x > 100 or pos.x < -100 then
        return false
    end
    if pos.z > 100 or pos.z < -100 then
        return false
    end
    return true
end