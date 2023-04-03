--[[
----------------------------------------------------
	created: 2020-11-30 21:31
	author : yuanhuan
	purpose:
----------------------------------------------------
]]
---@class checkStateNode : decoratorNode
checkStateNode = BaseClass(decoratorNode)

function checkStateNode:tick()
	local stateId = self.owner:getStateId()
	if stateId == self.stateId then
		if self.children then
			local v = self.children[1]
			if v.state == nil or v.state == eNodeState.running then
				v.state = v:tick()
				return v.state
			end
		end
	end
	return eNodeState.failure
end