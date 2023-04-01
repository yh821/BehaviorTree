--[[
----------------------------------------------------
	created: 2020-11-23 19:06
	author : yuanhuan
	purpose: 
----------------------------------------------------
]]
---@class weightNode : taskNode
weightNode = simple_class(taskNode)

function weightNode:start()
	local weight = self.data.weight or 0
	local score = math.random(0, 100)
	if score < weight then
		self.state = eNodeState.success
	else
		self.state = eNodeState.failure
	end
end

