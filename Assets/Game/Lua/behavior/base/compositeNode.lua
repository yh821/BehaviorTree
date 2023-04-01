--[[
----------------------------------------------------
	created: 2022-1-20 15:16
	author : yuanhuan
	purpose:
----------------------------------------------------
]]
---@class compositeNode : taskNode
---@field children taskNode[]
compositeNode = simple_class(taskNode)

local _insert = table.insert
local _remove = table.remove
local _max_child = 1 --最大子节点数

---@param node taskNode
function compositeNode:addChild(node)
	if self.children == nil then
		self.children = {}
	end
	if #self.children > 0 then
		_remove(self.children, 1)
	end
	_insert(self.children, node)
end

---@return taskNode[]
function compositeNode:getChildren()
	return self.children
end

function compositeNode:isComposite()
	return true
end