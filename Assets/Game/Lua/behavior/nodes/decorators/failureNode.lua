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
		if v.state == nodeState.failure or v.state == nodeState.success then
			return nodeState.failure
		else
			return v.state
		end
	end
	return nodeState.failure
end

