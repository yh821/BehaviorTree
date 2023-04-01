--[[
----------------------------------------------------
	created: 2020-10-26 10:12
	author : yuanhuan
	purpose: 
----------------------------------------------------
]]
---@class randomSelectorNode : compositeNode
randomSelectorNode = simple_class(compositeNode)

local _random = math.random

function randomSelectorNode:tick()
	if self.children then
		local index = nil
		for i, v in ipairs(self.children) do
			if index == nil and v.state == eNodeState.running then
				index = i
			end
		end
		if index == nil then
			index = _random(1, #self.children)
		end
		local n = self.children[index]
		if n then
			n.state = n:tick()
			return n.state
		end
	end
	return eNodeState.failure
end