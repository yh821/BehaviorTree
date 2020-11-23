--[[
----------------------------------------------------
	created: 2020-11-23 19:06
	author : yuanhuan
	purpose: 
----------------------------------------------------
]]

weightNode = simple_class(baseNode)

function weightNode:start()
	local weight = tonumber(self.data.weight or 0)
	local score = math.random(0, 100)
	if score < weight then
		self.state = nodeState.success
	else
		self.state = nodeState.failure
	end
end

