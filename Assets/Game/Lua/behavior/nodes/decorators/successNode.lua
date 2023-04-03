--[[
----------------------------------------------------
	created: 2020-10-26 11:28
	author : yuanhuan
	purpose:
----------------------------------------------------
]]
---@class successNode : decoratorNode
successNode = BaseClass(decoratorNode)

function successNode:tick()
	if self.children then
		local v = self.children[1]
		if v.state == eNodeState.failure or v.state == eNodeState.success then
			return eNodeState.success
		else
			return v.state
		end
	end
	return eNodeState.success
end