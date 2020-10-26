--[[
----------------------------------------------------
	created: 2020-10-26 11:22
	author : yuanhuan
	purpose: 
----------------------------------------------------
]]

failureNode = simple_class(baseNode)

local _insert = table.insert
local _remove = table.remove

function failureNode:addChild(node)
	if self.children == nil then
		self.children = {}
	end
	if #self.children > 0 then
		_remove(self.children, 1)
	end
	_insert(self.children, node)
end

function failureNode:getChildren()
	return self.children
end

function failureNode:tick()
	if self.children then
		local v = self.children[1]
		if v.state == nodeState.failure or v.state == nodeState.success then
			return nodeState.failure
		else
			return v.state
		end
	end
	return nodeState.failure
end

