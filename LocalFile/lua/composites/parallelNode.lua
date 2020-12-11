--[[
----------------------------------------------------
	created: 2020-10-26 10:12
	author : yuanhuan
	purpose:
----------------------------------------------------
]]

parallelNode = simple_class(baseNode)

local _insert = table.insert

function parallelNode:addChild(node)
	if self.children == nil then
		self.children = {}
	end
	_insert(self.children, node)
end

function parallelNode:getChildren()
	return self.children
end

function parallelNode:tick()
	local state = nodeState.success
	if self.children then
		for _, v in ipairs(self.children) do
			if v.state == nil or v.state == nodeState.running then
				v.state = v:tick()
				if v.state ~= nodeState.success then
					state = v.state
				end
				if state == nodeState.failure then
					break
				end
			end
		end
	end
	return state
end