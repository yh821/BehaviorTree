---
--- Created by Hugo
--- DateTime: 2023/4/4 19:40
---

---@class ParentNode : TaskNode
---@field _children TaskNode[]
ParentNode = BaseClass(TaskNode)

---@return TaskNode[]
function ParentNode:GetChildren()
    return self._children
end

---@param child ConditionNode
function ParentNode:ReevaluateCondition(child)
    local old_state = child:GetState()
    local new_state = child:Tick()
    local is_change = old_state ~= nil and old_state ~= new_state
    return is_change
end

function ParentNode:Clear()
    for i, v in ipairs(self._children) do
        self._children[i] = nil
    end
    ParentNode.super.Clear(self)
end
