--[[
----------------------------------------------------
	created: 2020-10-26 10:12
	author : yuanhuan
	purpose:
----------------------------------------------------
]]

waitNode = simple_class(baseNode)

local _random = math.random

function waitNode:start()
	self.deltaTime = 0
	self.waitTime = _random(tonumber(self.data.waitMin), tonumber(self.data.waitMax))
end

function waitNode:update()
	if self.deltaTime >= self.waitTime then
		--print('等待完成')
		return nodeState.success
	end
	self.deltaTime = self.deltaTime + behaviorManager.interval
	return nodeState.running
end