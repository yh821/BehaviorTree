--[[
----------------------------------------------------
	created: 2020-10-26 10:12
	author : yuanhuan
	purpose:
----------------------------------------------------
]]

nodeState = {
	running = 0,
	success = 1,
	failure = 2,
}

baseNode = simple_class()

local _id = 0
local _openLog = true
local _format = string.format

function baseNode:__init(parent, owner)
	self.owner = owner
	self:reset()
	self:awake()
	_id = _id + 1
	self.id = _id
end

function baseNode:awake()
end

function baseNode:start()
end

function baseNode:tick()
	if self.state == nil then
		self:start()
	end
	self.state = self:update()
	return self.state
end

function baseNode:update()
	return nodeState.success
end

function baseNode:reset()
	self.state = nil
end

function baseNode:addChild(node)
end

function baseNode:getChildren(node)
end

function baseNode:setSharedVar(key, value)
	local bb = self.owner:getBlackboard()
	bb[key] = value
end

function baseNode:getSharedVar(key)
	local bb = self.owner:getBlackboard()
	return bb[key]
end

function baseNode:print(...)
	if _openLog then
		print(_format('[behavior][%s]', self.id), ...)
	end
end