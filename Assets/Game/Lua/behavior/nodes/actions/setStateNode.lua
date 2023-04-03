--[[
----------------------------------------------------
	created: 2020-12-07 16:52
	author : yuanhuan
	purpose:
----------------------------------------------------
]]
---@class setStateNode : actionNode
setStateNode = BaseClass(actionNode)

function setStateNode:start()
	if self.data and self.data.stateId then
		self.owner:setStateId(self.data.stateId)
		self.state = eNodeState.success
	else
		self.state = eNodeState.failure
	end
end

