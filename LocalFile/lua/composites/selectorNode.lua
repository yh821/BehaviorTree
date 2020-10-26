--[[
----------------------------------------------------
	created: 2020-10-26 10:12
	author : yuanhuan
	purpose:
----------------------------------------------------
]]

selectorNode = simple_class(baseNode)

local _insert = table.insert

function selectorNode:addChild(node)
	if self.children == nil then
		self.children = {}
	end
	_insert(self.children, node)
end

function selectorNode:getChildren()
	return self.children
end

function selectorNode:tick()
	if self.children then
		for _, v in ipairs(self.children) do
			if v.state == nil or v.state == nodeState.running then
				v.state = v:tick()
				if v.state ~= nodeState.failure then
					return v.state
				end
			end
		end
	end
	return nodeState.failure
end