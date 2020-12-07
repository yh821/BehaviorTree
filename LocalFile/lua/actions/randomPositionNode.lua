--[[
----------------------------------------------------
	created: 2020-12-07 16:59
	author : yuanhuan
	purpose: 
----------------------------------------------------
]]

randomPositionNode = simple_class(baseNode)

local MapManager = CS.MapManagerInterface
local IsCanMove = MapManager.IsCanMove

function randomPositionNode:start()
	local pos = self:getNextPos()
	if IsCanMove(pos) then
		self:setSharedVar('targetPos', pos)
		self.state = nodeState.success
	else
		self.state = nodeState.failure
	end
end