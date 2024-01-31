---
--- Created by Hugo
--- DateTime: 2024/1/2 21:52
---

---@class TriggerNode : DecoratorNode
TriggerNode = TriggerNode or BaseClass(DecoratorNode)

function TriggerNode:Tick(delta_time)
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
        local v = self._children[1]
        if v:IsNotExecuted() or v:IsRunning() then
            v:SetState(v:Tick(delta_time))
            return v:GetState()
        end
    end
    return eNodeState.Failure
end

function TriggerNode:GetTriggerType()
    if self.trigger_type == nil then
        local type = self.data and self.data[BtConfig.triggerType]
        self.trigger_type = eTriggerType[type] or eTriggerType.Equals
    end
    return self.trigger_type
end

function TriggerNode:GetTriggerValue()
    if self.trigger_value == nil then
        self.trigger_value = self.data and self.data[BtConfig.trigger] or 0
    end
    return self.trigger_value
end

function IsTriggerNode:Clear()
    self.type = nil
    self.value = nil
    self.trigger_type = nil
    self.trigger_value = nil
end