--[[
----------------------------------------------------
	created: 2020-10-26 10:12
	author : yuanhuan
	purpose: 
----------------------------------------------------
]]

randomSelectorNode = simple_class(baseNode)

local _insert = table.insert
local _random = math.random

function randomSelectorNode:addChild(node)
	if self.children == nil then
		self.children = {}
	end
	_insert(self.children, node)
end

function randomSelectorNode:getChildren()
	return self.children
end

function randomSelectorNode:tick()
	if self.children then
		local index = nil
		for i, v in ipairs(self.children) do
			if index == nil and v.state == nodeState.running then
				index = i
			end
		end
		if index == nil then
			index = _random(1, #self.children)
		end
		local n = self.children[index]
		if n then
			n.state = n:tick()
			return n.state
		end
	end
	return nodeState.failure
end