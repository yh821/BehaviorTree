--[[
----------------------------------------------------
	created: 2020-12-07 16:52
	author : yuanhuan
	purpose: 
----------------------------------------------------
]]

setStateNode = simple_class(baseNode)

function setStateNode:start()
	if self.data and self.data.stateId then
		self.owner:setStateId(self.data.stateId)
		self.state = nodeState.success
	else
		self.state = nodeState.failure
	end
end

