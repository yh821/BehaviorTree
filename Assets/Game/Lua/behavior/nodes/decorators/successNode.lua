--[[
----------------------------------------------------
	created: 2020-10-26 11:28
	author : yuanhuan
	purpose: 
----------------------------------------------------
]]
---@class successNode : decoratorNode
successNode = simple_class(decoratorNode)

function successNode:tick()
	if self.children then
		local v = self.children[1]
		if v.state == nodeState.failure or v.state == nodeState.success then
			return nodeState.success
		else
			return v.state
		end
	end
	return nodeState.success
end