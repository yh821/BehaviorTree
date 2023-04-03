--[[
----------------------------------------------------
	created: 2020-10-26 10:12
	author : yuanhuan
	purpose:
----------------------------------------------------
]]
---@class selectorNode : compositeNode
selectorNode = BaseClass(compositeNode)

function selectorNode:tick()
	if self.children then
		for _, v in ipairs(self.children) do
			if v.state == nil or v.state == eNodeState.running then
				v.state = v:tick()
				if v.state ~= eNodeState.failure then
					return v.state
				end
			end
		end
	end
	return eNodeState.failure
end