--[[
----------------------------------------------------
	created: 2020-10-26 11:22
	author : yuanhuan
	purpose: 
----------------------------------------------------
]]
---@class decoratorNode : baseNode
---@field children baseNode[]
decoratorNode = simple_class(baseNode)

local _insert = table.insert
local _remove = table.remove

function decoratorNode:addChild(node)
	if self.children == nil then
		self.children = {}
	end
	if #self.children > 0 then
		_remove(self.children, 1)
	end
	_insert(self.children, node)
end

function decoratorNode:getChildren()
	return self.children
end

function decoratorNode:tick()
	--override
end

