---
--- Created by Hugo
--- DateTime: 2020/10/26 11:28
---

---@class SuccessNode : DecoratorNode
SuccessNode = BaseClass(DecoratorNode)

function SuccessNode:Tick(delta_time)
    local state = self._children[1]:Tick(delta_time)
    if state == eNodeState.Failure then
        return eNodeState.Success
    end
    return state
end