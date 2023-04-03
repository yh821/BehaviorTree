--[[
----------------------------------------------------
	created: 2020-10-26 10:12
	author : yuanhuan
	purpose:
----------------------------------------------------
]]
---@class behaviorTree : taskNode
---@field child taskNode
---@field blackBoard table
---@field guid number
---@field restartOnComplete boolean
behaviorTree = BaseClass(taskNode)

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

---@return taskNode
function behaviorTree:getChildren()
	return self.child
end

---@type fun(parent:taskNode)
local __ResetAll
__ResetAll = function(parent)
	parent:_reset()
	local children = parent:getChildren()
	if children then
		for i, v in ipairs(children) do
			__ResetAll(v)
		end
	end
end

function behaviorTree:tick()
	local state = self.child.state
	if self.restartOnComplete and (state == eNodeState.success or state == eNodeState.failure) then
		__ResetAll(self.child)
	end
	if self.child.state == nil or self.child.state == eNodeState.running then
		self.child.state = self.child:tick()
	else
		return self.child.state
	end
end

---@type fun(parent:taskNode)
local __AbortAll
__AbortAll = function(parent)
	if parent.state == eNodeState.running then
		parent:abort()
		parent.state = eNodeState.failure
	end
	local children = parent:getChildren()
	if children then
		for i, v in ipairs(children) do
			__AbortAll(v)
		end
	end
end

function behaviorTree:abort()
	__AbortAll(self.child)
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