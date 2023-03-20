--[[
----------------------------------------------------
	created: 2022-1-20 15:16
	author : yuanhuan
	purpose:
----------------------------------------------------
]]
---@class compositeNode : baseNode
---@field children baseNode[]
compositeNode = simple_class(baseNode)

local _insert = table.insert
local _remove = table.remove
--local _max_child = 1 --最大子节点数

---@param node baseNode
function compositeNode:addChild(node)
	if self.children == nil then
		self.children = {}
	end
	if #self.children > 0 then
		_remove(self.children, 1)
	end
	_insert(self.children, node)
end

---@return baseNode[]
function compositeNode:getChildren()
	return self.children
end