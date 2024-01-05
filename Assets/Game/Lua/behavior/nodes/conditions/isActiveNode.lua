---
--- Created by Hugo
--- DateTime: 2023/4/1 17:30
---

---@class IsActiveNode : ConditionNode
IsActiveNode = BaseClass(ConditionNode)

function IsActiveNode:Tick(delta_time)
    local gameObject = self.owner.gameObject
    if gameObject then
        if not IsNilOrEmpty(self.data.path) then
            local trans = gameObject.transform:Find(self.data.path)
            if trans and trans.gameObject then
                local activeSelf = trans.gameObject.activeSelf
                if activeSelf then
                    self:print("Enable " .. self.data.path)
                else
                    self:print("Disable " .. self.data.path)
                end
                return activeSelf and eNodeState.Success or eNodeState.Failure
            end
        end
    end
    return eNodeState.Failure
end