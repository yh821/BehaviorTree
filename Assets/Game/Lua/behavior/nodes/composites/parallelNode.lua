--[[
----------------------------------------------------
	created: 2020-10-26 10:12
	author : yuanhuan
	purpose:
----------------------------------------------------
]]
---@class parallelNode : compositeNode
parallelNode = simple_class(compositeNode)

function parallelNode:tick()
	local state = nodeState.success
	if self.children then
		for _, v in ipairs(self.children) do
			if v.state == nil or v.state == nodeState.running then
				v.state = v:tick()
				if v.state ~= nodeState.success then
					state = v.state
				end
				if state == nodeState.failure then
					break
				end
			end
		end
	end
	return state
end