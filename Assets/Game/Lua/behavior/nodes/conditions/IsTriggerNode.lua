---
--- Created by Hugo
--- DateTime: 2024/1/3 14:19
---

---@class IsTriggerNode : ConditionNode
IsTriggerNode = BaseClass(ConditionNode)

function IsTriggerNode:Tick(delta_time)
    if self.type == nil then
        self.type = self:GetTriggerType()
        self.value = self:GetTriggerValue()
    end
    local condition = false
    local value = self:GetTrigger()
    if self.type == eTriggerType.Equals then
        condition = self.value == value
    elseif self.type == eTriggerType.NotEquals then
        condition = self.value ~= value
    elseif self.type == eTriggerType.Greater then
        condition = self.value < value
    elseif self.type == eTriggerType.Less then
        condition = self.value > value
    end
    if condition then
        return eNodeState.Success
    end
    return eNodeState.Failure
end

function IsTriggerNode:GetTriggerType()
    if self.trigger_type == nil then
        local type = self.data and self.data[BtConfig.triggerType]
        self.trigger_type = eTriggerType[type] or eTriggerType.Equals
    end
    return self.trigger_type
end

function IsTriggerNode:GetTriggerValue()
    if self.trigger_value == nil then
        self.trigger_value = self.data and self.data[BtConfig.trigger] or 0
    end
    return self.trigger_value
end