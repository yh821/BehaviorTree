--[[
----------------------------------------------------
	created: 2020-10-26 10:12
	author : yuanhuan
	purpose:
----------------------------------------------------
]]
---@class parallelNode : compositeNode
parallelNode = BaseClass(compositeNode)

function parallelNode:tick()
	local state = eNodeState.success
	if self.children then
		for _, v in ipairs(self.children) do
			if v.state == nil or v.state == eNodeState.running then
				v.state = v:tick()
				if v.state ~= eNodeState.success then
					state = v.state
				end
				if state == eNodeState.failure then
					break
				end
			end
		end
	end
	return state
end