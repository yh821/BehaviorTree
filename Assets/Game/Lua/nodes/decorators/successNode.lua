--[[
----------------------------------------------------
	created: 2020-10-26 11:28
	author : yuanhuan
	purpose: 
----------------------------------------------------
]]

successNode = simple_class(baseNode)

local _insert = table.insert
local _remove = table.remove

function successNode:addChild(node)
	if self.children == nil then
		self.children = {}
	end
	if #self.children > 0 then
		_remove(self.children, 1)
	end
	_insert(self.children, node)
end

function successNode:getChildren()
	return self.children
end

function successNode:tick()
	if self.children then
		local v = self.children[1]
		if v.state == nodeState.failure or v.state == nodeState.success then
			return nodeState.success
		else
			return v.state
		end
	end
	return nodeState.success
end