---
--- Created by Hugo
--- DateTime: 2024/1/2 14:09
---

---@class RandomSelectorNode : CompositeNode
RandomSelectorNode = RandomSelectorNode or BaseClass(CompositeNode)

function RandomSelectorNode:Tick(delta_time)
    local index = nil
    for i, v in ipairs(self._children) do
        if index == nil and v:IsRunning() then
            index = i
        end
    end
    if index == nil then
        index = math.random(1, #self._children)
    end
    local v = self._children[index]
    if v then
        v:SetState(v:Tick(delta_time))
        return v:GetState()
    end
end