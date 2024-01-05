---
--- Created by Hugo
--- DateTime: 2020/10/26 10:12
---

---@class LogNode : ActionNode
LogNode = BaseClass(ActionNode)

function LogNode:Start()
    if self.data.is_error then
        print_error(self.data.msg)
    else
        print_log(self.data.msg)
    end
    return eNodeState.Success
end
