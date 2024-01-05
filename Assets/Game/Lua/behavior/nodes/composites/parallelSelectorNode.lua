---
--- Created by Hugo
--- DateTime: 2020/10/26 10:12
---

---@class ParallelSelectorNode : CompositeNode
ParallelSelectorNode = BaseClass(CompositeNode)

function ParallelSelectorNode:Tick(delta_time)
    local is_complete = true
    for _, v in ipairs(self._children) do
        if v:IsNotExecuted() or v:IsRunning() then
            v:SetState(v:Tick(delta_time))
            if v:IsRunning() then
                is_complete = false
            elseif v:IsSucceed() then
                return eNodeState.Success
            end
        end
    end
    return is_complete and eNodeState.Failure or eNodeState.Running
end