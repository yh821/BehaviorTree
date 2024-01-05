---
--- Created by Hugo
--- DateTime: 2020/10/26 11:27
---

---@class InverterNode : DecoratorNode
InverterNode = BaseClass(DecoratorNode)

function InverterNode:Tick(delta_time)
    local state = self._children[1]:Tick(delta_time)
    if state == eNodeState.Failure then
        return eNodeState.Success
    elseif state == eNodeState.Success then
        return eNodeState.Failure
    end
    return state
end