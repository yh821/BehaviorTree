--[[
----------------------------------------------------
	created: 2020-10-26 10:12
	author : yuanhuan
	purpose:
----------------------------------------------------
]]

sequenceNode = simple_class(baseNode)

local _insert = table.insert

function sequenceNode:addChild(node)
	if self.children == nil then
		self.children = {}
	end
	_insert(self.children, node)
end

function sequenceNode:getChildren()
	return self.children
end

function sequenceNode:tick()
	if self.children then
		for _, v in ipairs(self.children) do
			if v.state == nil or v.state == nodeState.running then
				v.state = v:tick()
				if v.state ~= nodeState.success then
					return v.state
				end
			end
		end
	end
	return nodeState.success
end