---
--- Created by Hugo
--- DateTime: 2020/12/07 16:52
---

---@class SetTriggerNode : ActionNode
SetTriggerNode = BaseClass(ActionNode)

function SetTriggerNode:Start()
    if self.data and self.data[BtConfig.trigger] then
        self:SetTrigger(self.data[BtConfig.trigger] or 0)
        return eNodeState.Success
    else
        return eNodeState.Failure
    end
end

