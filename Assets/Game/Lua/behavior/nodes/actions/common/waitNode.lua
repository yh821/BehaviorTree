---
--- Created by Hugo
--- DateTime: 2020/10/26 10:12
---

---@class WaitNode : ActionNode
WaitNode = BaseClass(ActionNode)

local _random = math.random

function WaitNode:Start()
    self.deltaTime = 0
    self.waitTime = _random(self.data.min_time, self.data.max_time)
end

function WaitNode:Update(delta_time)
    if self.deltaTime >= self.waitTime then
        --self:print("等待完成")
        return eNodeState.Success
    end
    self.deltaTime = self.deltaTime + delta_time
    return eNodeState.Running
end

function WaitNode:Abort()
    self:print("<color=red>打断等待</color>")
    return eNodeState.Failure
end