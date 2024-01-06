---
--- Created by Hugo
--- DateTime: 2020/10/26 10:12
---

---@class LogNode : ActionNode
LogNode = BaseClass(ActionNode)

function LogNode:Start()
    local msg
    if self.data.sharedVar then
        self:print(self.data.sharedVar)
        msg = self:GetSharedVar(self.data.sharedVar)
    end
    if msg == nil then
        msg = self.data.msg
    end
    if self.data.is_error then
        self:print_error(msg)
    else
        self:print(msg)
    end
    return eNodeState.Success
end
