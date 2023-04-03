--[[
----------------------------------------------------
	created: 2020-10-26 10:12
	author : yuanhuan
	purpose:
----------------------------------------------------
]]
---@class waitNode : actionNode
waitNode = BaseClass(actionNode)

local _random = math.random

function waitNode:start()
	self.deltaTime = 0
	self.waitTime = _random(self.data.waitMin, self.data.waitMax)
end

function waitNode:update()
	if self.deltaTime >= self.waitTime then
		--print('等待完成')
		return eNodeState.success
	end
	self.deltaTime = self.deltaTime + behaviorManager.interval
	return eNodeState.running
end