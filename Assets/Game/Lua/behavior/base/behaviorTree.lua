--[[
----------------------------------------------------
	created: 2020-10-26 10:12
	author : yuanhuan
	purpose:
----------------------------------------------------
]]
---@class behaviorTree
---@field child baseNode
---@field blackBoard table
---@field guid number
---@field restartOnComplete boolean
behaviorTree = simple_class()

function behaviorTree:awake()
	self.child = nil
	self.blackBoard = nil
	self.guid = self.data.guid
	self.restartOnComplete = tonumber(self.data.restartOnComplete) == 1
end

---@param node
function behaviorTree:addChild(node)
	self.child = node
end

---@return baseNode
function behaviorTree:getChildren()
	return self.child
end

local _resetAll
---@param parent baseNode
_resetAll = function(parent)
	parent:reset()
	local children = parent:getChildren()
	if children then
		for i, v in ipairs(children) do
			_resetAll(v)
		end
	end
end

function behaviorTree:tick()
	local state = self.child.state
	if self.restartOnComplete and (state == nodeState.success or state == nodeState.failure) then
		_resetAll(self.child)
	end
	if self.child.state == nil or self.child.state == nodeState.running then
		self.child.state = self.child:tick()
	else
		return self.child.state
	end
end

local _breakAll
---@param parent baseNode
_breakAll = function(parent)
	parent:broke()
	local children = parent:getChildren()
	if children then
		for i, v in ipairs(children) do
			_breakAll(v)
		end
	end
end

function behaviorTree:broke()
	_breakAll(self.child)
end

---@return table
function behaviorTree:getBlackboard()
	if self.blackBoard == nil then
		self.blackBoard = {}
	end
	return self.blackBoard
end

---@param key string
---@param value any
function behaviorTree:setSharedVar(key, value)
	local bb = self:getBlackboard()
	bb[key] = value
end

---@param key string
---@return any
function behaviorTree:getSharedVar(key)
	local bb = self:getBlackboard()
	return bb[key]
end

---@param stateId number
function behaviorTree:setStateId(stateId)
	self:setSharedVar('stateId', stateId)
end

---@return number
function behaviorTree:getStateId()
	return self:getSharedVar('stateId')
end