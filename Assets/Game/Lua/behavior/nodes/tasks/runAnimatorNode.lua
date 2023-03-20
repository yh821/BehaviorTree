--[[
----------------------------------------------------
	created: 2020-11-23 19:08
	author : yuanhuan
	purpose: 
----------------------------------------------------
]]
---@class runAnimatorNode : baseNode
runAnimatorNode = simple_class(baseNode)

local MapManager = CS.MapManagerInterface
local RunAnimator = MapManager.RunAnimator

function runAnimatorNode:start()
	if self.data and self.data.stateId then
		self:refresh(self.data.stateId)
	end
end

function runAnimatorNode:update()
	self:refresh(self:getSharedVar('animState'))
	if self:getSharedVar('playState') == playStateEnum.eEnd then
		return nodeState.success
	end
	return nodeState.running
end

function runAnimatorNode:broke()
	---中断行为暂时设置为idle动画
	self:refresh(animatorStateEnum.eIdle)
end

---@param stateId number
function runAnimatorNode:refresh(stateId)
	if self.lastStateId ~= stateId then
		self.lastStateId = stateId
		RunAnimator(self.owner.guid, stateId)
	end
end