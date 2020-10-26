--[[
----------------------------------------------------
	created: 2020-10-26 11:27
	author : yuanhuan
	purpose: 
----------------------------------------------------
]]

inverterNode = simple_class(baseNode)

local _insert = table.insert
local _remove = table.remove

function inverterNode:addChild(node)
	if self.children == nil then
		self.children = {}
	end
	if #self.children > 0 then
		_remove(self.children, 1)
	end
	_insert(self.children, node)
end

function inverterNode:getChildren()
	return self.children
end

function inverterNode:tick()
	if self.children then
		local v = self.children[1]
		if v.state == nodeState.failure then
			return nodeState.success
		elseif v.state == nodeState.success then
			return nodeState.failure
		else
			return v.state
		end
	end
	return nodeState.failure
end