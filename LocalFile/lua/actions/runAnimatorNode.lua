--[[
----------------------------------------------------
	created: 2020-11-23 19:08
	author : yuanhuan
	purpose: 
----------------------------------------------------
]]

runAnimatorNode = simple_class(baseNode)

local RunAnimator = CS.MapManagerInterface.RunAnimator

function runAnimatorNode:refresh(stateId)
	if self.lastStateId ~= stateId then
		self.lastStateId = stateId
		RunAnimator(self.owner.guid, stateId)
	end
end

function runAnimatorNode:start()
	if self.data and self.data.stateId then
		self:refresh(tonumber(self.data.stateId))
	end
end

function runAnimatorNode:update()
	self:refresh(self:getSharedVar('StateID'))
	if self:getSharedVar('PlayState') == 1 then
		return nodeState.success
	end
	return nodeState.running
end