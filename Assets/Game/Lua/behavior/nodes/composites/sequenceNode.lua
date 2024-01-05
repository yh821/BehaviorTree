---
--- Created by Hugo
--- DateTime: 2020/10/26 10:12
---

---@class SequenceNode : CompositeNode
SequenceNode = BaseClass(CompositeNode)

function SequenceNode:Tick(delta_time)
    for i, v in ipairs(self._children) do
        if self:IsAbortLower() then
            if v:IsCondition() and v:IsNotExecuted() then
                self:SetNeedReevaluate()
            end
        end
        if self:IsAbortSelf() then
            if v:IsCondition() and v:IsExecuted() and self:IsRunning() then
                if v:SetState(v:Tick(delta_time)) and v:IsFailed() then
                    self:StartAbortNode(i + 1)
                    return v:GetState()
                end
            end
        end
        if v:IsNotExecuted() or v:IsRunning() then
            v:SetState(v:Tick(delta_time))
            if v:GetState() ~= eNodeState.Success then
                return v:GetState()
            end
        elseif v:IsComposite() and v:IsNeedReevaluate() then
            local state = v:ReevaluateNode(delta_time)
            if state == eNodeState.Failure then
                self:StartAbortNode(i + 1)
                return state --劫持状态
            end
        end
    end
    return eNodeState.Success
end
