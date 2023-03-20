--[[
----------------------------------------------------
	created: 2020-10-26 10:12
	author : yuanhuan
	purpose:
----------------------------------------------------
]]
---@class nodeState
---@field running number
---@field success number
---@field failure number
nodeState = {
	running = 0,
	success = 1,
	failure = 2,
}

---@class baseNode
---@field owner behaviorTree
---@field id number
---@field data table
---@field state nodeState
baseNode = simple_class()

local _id = 0
local _openLog = true
local _format = string.format

---@param owner behaviorTree
---@param data table
function baseNode:__init(super, owner, data)
	self.owner = owner
	self.data = data
	self:reset()
	self:awake()
	_id = _id + 1
	self.id = _id
end

function baseNode:awake()
end

function baseNode:start()
	self.state = nodeState.success
end

---@return nodeState
function baseNode:tick()
	if self.state == nil then
		self:start()
	end
	if self.state == nil or self.state == nodeState.running then
		self.state = self:update()
	end
	return self.state
end

---@return nodeState
function baseNode:update()
	return self.state
end

function baseNode:reset()
	self.state = nil
end

function baseNode:broke()
end

---@param node baseNode
function baseNode:addChild(node)
end

function baseNode:getChildren()
end

---@param key string
---@param value any
function baseNode:setSharedVar(key, value)
	local bb = self.owner:getBlackboard()
	bb[key] = value
end

---@param key string
function baseNode:getSharedVar(key)
	local bb = self.owner:getBlackboard()
	return bb[key]
end

function baseNode:print(...)
	if _openLog then
		print(_format('[behavior][%s]', self.id), ...)
	end
end