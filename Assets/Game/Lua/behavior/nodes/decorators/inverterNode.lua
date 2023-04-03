--[[
----------------------------------------------------
	created: 2020-10-26 11:27
	author : yuanhuan
	purpose:
----------------------------------------------------
]]
---@class inverterNode : decoratorNode
inverterNode = BaseClass(decoratorNode)

function inverterNode:tick()
	if self.children then
		local v = self.children[1]
		if v.state == eNodeState.failure then
			return eNodeState.success
		elseif v.state == eNodeState.success then
			return eNodeState.failure
		else
			return v.state
		end
	end
	return eNodeState.failure
end