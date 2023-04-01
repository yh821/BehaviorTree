--[[
----------------------------------------------------
	created: 2020-12-07 16:59
	author : yuanhuan
	purpose: 
----------------------------------------------------
]]
---@class randomPositionNode : taskNode
randomPositionNode = simple_class(taskNode)

local MapManager = CS.MapManagerInterface
local IsCanMove = MapManager.IsCanMove

function randomPositionNode:start()
	local pos = self:getNextPos()
	if IsCanMove(pos) then
		self:setSharedVar('targetPos', pos)
		self.state = eNodeState.success
	else
		self.state = eNodeState.failure
	end
end