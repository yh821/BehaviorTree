--[[
----------------------------------------------------
	created: 2020-11-30 21:31
	author : yuanhuan
	purpose: 
----------------------------------------------------
]]

checkStateNode = simple_class(baseNode)

local _insert = table.insert
local _remove = table.remove

function checkStateNode:addChild(node)
	if self.children == nil then
		self.children = {}
	end
	if #self.children > 0 then
		_remove(self.children, 1)
	end
	_insert(self.children, node)
end

function checkStateNode:getChildren()
	return self.children
end

function checkStateNode:tick()
	local stateId = self.owner:getStateId()
	if stateId == self.stateId then
		if self.children then
			local v = self.children[1]
			if v.state == nil or v.state == nodeState.running then
				v.state = v:tick()
				return v.state
			end
		end
	end
	return nodeState.failure
end