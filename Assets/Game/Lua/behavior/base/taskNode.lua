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
eNodeState = {
	running = 0,
	success = 1,
	failure = 2,
}

---@class taskNode : BaseClass
---@field owner behaviorTree
---@field id number
---@field data table
---@field state nodeState
taskNode = BaseClass()

local _id = 0
local _openLog = true
local _format = string.format

---@param owner behaviorTree
---@param data table
function taskNode:__init(super, owner, data)
	self.owner = owner
	self.data = data
	self:awake()
	_id = _id + 1
	self.id = _id
end

function taskNode:awake()
	--override
end

function taskNode:start()
	self.state = eNodeState.failure
end

---@return nodeState
function taskNode:tick()
	if self.state == nil then
		self:start()
	end
	if self.state == nil or self.state == eNodeState.running then
		self.state = self:update()
	end
	return self.state
end

---@return nodeState
function taskNode:update()
	return self.state
end

function taskNode:_reset()
	if self.state == eNodeState.running then
		self:abort()
	end
	self:reset()
	self.state = nil
end

function taskNode:reset()
	--override
end

function taskNode:abort()
	self.state = eNodeState.failure
end

---@param node taskNode
function taskNode:addChild(node)
	--override
end

function taskNode:getChildren()
	--override
end

---@param key string
---@param value any
function taskNode:setSharedVar(key, value)
	local bb = self.owner:getBlackboard()
	bb[key] = value
end

---@param key string
function taskNode:getSharedVar(key)
	local bb = self.owner:getBlackboard()
	return bb[key]
end

function taskNode:isAction()
	return false
end

function taskNode:isCondition()
	return false
end

function taskNode:isComposite()
	return false
end

function taskNode:isDecorator()
	return false
end

function taskNode:print(...)
	if _openLog then
		print(_format('[behavior][%s]', self.id), ...)
	end
end