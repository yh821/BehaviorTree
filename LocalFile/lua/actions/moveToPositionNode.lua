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
	self:setSharedVar('PlayState', 0)
	local targetPos = self:getSharedVar('targetPos')
	local entityPos = GetEntityPos(self.owner.guid)
	if targetPos == nil or IsPositionEqual(entityPos, targetPos) then
		self.state = nodeState.failure
		self:setSharedVar('PlayState', 1)
	else
		self:setSharedVar('StateID', 1)
		MoveToPosition(self.owner.guid, targetPos, function()
			self.state = nodeState.success
			self:setSharedVar('StateID', 0)
			self:setSharedVar('PlayState', 1)
		end)
	end
end