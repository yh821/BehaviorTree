--[[
----------------------------------------------------
	created: 2020-11-23 19:12
	author : yuanhuan
	purpose: 
----------------------------------------------------
]]

moveToPositionNode = simple_class(baseNode)

local MapManager = CS.MapManagerInterface
local IsPositionEqual = MapManager.IsPositionEqual
local GetEntityPos = MapManager.GetTilemapObjectPosition
local MoveToPosition = MapManager.MoveToPosition

function moveToPositionNode:start()
	self.state = nodeState.running
	self:setSharedVar('playState', 0)
	local targetPos = self:getSharedVar('targetPos')
	local entityPos = GetEntityPos(self.owner.guid)
	if targetPos == nil or IsPositionEqual(entityPos, targetPos) then
		self.state = nodeState.failure
		self:setSharedVar('playState', 1)
	else
		self:setSharedVar('animState', animatorStateEnum.walk)
		MoveToPosition(self.owner.guid, targetPos, function()
			self.state = nodeState.success
			self:setSharedVar('animState', animatorStateEnum.idle)
			self:setSharedVar('playState', 1)
		end)
	end
end