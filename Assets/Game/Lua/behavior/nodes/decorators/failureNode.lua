---
--- Created by Hugo
--- DateTime: 2020/10/26 11:22
---

---@class FailureNode : DecoratorNode
FailureNode = BaseClass(DecoratorNode)

function FailureNode:Tick(delta_time)
    local state = self._children[1]:Tick(delta_time)
    if state == eNodeState.Success then
        return eNodeState.Failure
    end
    return state
end

