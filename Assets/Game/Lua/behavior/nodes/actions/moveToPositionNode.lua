--[[
----------------------------------------------------
	created: 2020-11-23 19:12
	author : yuanhuan
	purpose:
----------------------------------------------------
]]
---@class moveToPositionNode : actionNode
moveToPositionNode = BaseClass(actionNode)

local MapManager = CS.MapManagerInterface
local IsPositionEqual = MapManager.IsPositionEqual
local GetEntityPos = MapManager.GetTilemapObjectPosition
local MoveToPosition = MapManager.MoveToPosition
local StopMove = MapManager.StopMove

function moveToPositionNode:start()
	self.state = eNodeState.running
	self:setSharedVar('playState', playStateEnum.eStart)
	local targetPos = self:getSharedVar('targetPos')
	local entityPos = GetEntityPos(self.owner.guid)
	if targetPos == nil or IsPositionEqual(entityPos, targetPos) then
		self.state = eNodeState.failure
		self:setSharedVar('playState', playStateEnum.eEnd)
	else
		self:setSharedVar('animState', animatorStateEnum.eWalk)
		MoveToPosition(self.owner.guid, targetPos, function()
			self:moveFinish()
		end)
	end
end

function moveToPositionNode:abort()
	StopMove(self.owner.guid)
	self:moveFinish()
end

function moveToPositionNode:moveFinish()
	self:setSharedVar('animState', animatorStateEnum.eIdle)
	self:setSharedVar('playState', playStateEnum.eEnd)
	self.state = eNodeState.success
end