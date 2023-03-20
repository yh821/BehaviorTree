--[[
----------------------------------------------------
	created: 2020-10-26 11:27
	author : yuanhuan
	purpose: 
----------------------------------------------------
]]
---@class inverterNode : decoratorNode
inverterNode = simple_class(decoratorNode)

function inverterNode:tick()
	if self.children then
		local v = self.children[1]
		if v.state == nodeState.failure then
			return nodeState.success
		elseif v.state == nodeState.success then
			return nodeState.failure
		else
			return v.state
		end
	end
	return nodeState.failure
end