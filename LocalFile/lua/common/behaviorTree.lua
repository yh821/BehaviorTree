--[[
----------------------------------------------------
	created: 2020-10-26 10:12
	author : yuanhuan
	purpose:
----------------------------------------------------
]]

behaviorTree = simple_class(baseNode)

function behaviorTree:awake()
	self.child = nil
	self.blackBoard = nil
	self.guid = self.data.guid
	self.restartOnComplete = tonumber(self.data.restartOnComplete) == 1
end

function behaviorTree:addChild(node)
	self.child = node
end

function behaviorTree:getChildren()
	return self.child
end

local _resetAll
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
	if state == nil or state == nodeState.running then
		self.child.state = self.child:tick()
	else
		return state
	end
end

local _breakAll
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

function behaviorTree:getBlackboard()
	if self.blackBoard == nil then
		self.blackBoard = {}
	end
	return self.blackBoard
end

function behaviorTree:setSharedVar(key, value)
	local bb = self:getBlackboard()
	bb[key] = value
end

function behaviorTree:getSharedVar(key)
	local bb = self:getBlackboard()
	return bb[key]
end

function behaviorTree:setStateId(stateId)
	self:setSharedVar('stateId', stateId)
end

function behaviorTree:getStateId()
	return self:getSharedVar('stateId')
end