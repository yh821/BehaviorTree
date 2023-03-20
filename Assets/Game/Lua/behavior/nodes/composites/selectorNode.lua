--[[
----------------------------------------------------
	created: 2020-10-26 10:12
	author : yuanhuan
	purpose:
----------------------------------------------------
]]
---@class selectorNode : compositeNode
selectorNode = simple_class(compositeNode)

function selectorNode:tick()
	if self.children then
		for _, v in ipairs(self.children) do
			if v.state == nil or v.state == nodeState.running then
				v.state = v:tick()
				if v.state ~= nodeState.failure then
					return v.state
				end
			end
		end
	end
	return nodeState.failure
end