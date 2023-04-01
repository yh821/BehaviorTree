--[[
----------------------------------------------------
	created: 2020-10-26 11:22
	author : yuanhuan
	purpose: 
----------------------------------------------------
]]
---@class failureNode : decoratorNode
failureNode = simple_class(decoratorNode)

function failureNode:tick()
	if self.children then
		local v = self.children[1]
		if v.state == eNodeState.failure or v.state == eNodeState.success then
			return eNodeState.failure
		else
			return v.state
		end
	end
	return eNodeState.failure
end

