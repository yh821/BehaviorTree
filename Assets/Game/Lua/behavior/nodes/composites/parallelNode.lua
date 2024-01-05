---
--- Created by Hugo
--- DateTime: 2020/10/26 10:12
---

---@class ParallelNode : CompositeNode
ParallelNode = BaseClass(CompositeNode)

function ParallelNode:Tick(delta_time)
    local is_complete = true
    for _, v in ipairs(self._children) do
        if v:IsNotExecuted() or v:IsRunning() then
            v:SetState(v:Tick(delta_time))
            if v:IsRunning() then
                is_complete = false
            elseif v:IsFailed() then
                return eNodeState.Failure
            end
        end
    end
    return is_complete and eNodeState.Success or eNodeState.Running
end