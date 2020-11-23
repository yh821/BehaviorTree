--[[
----------------------------------------------------
	created: 2020-10-26 11:06
	author : yuanhuan
	purpose: 
----------------------------------------------------
]]

behaviorManager = {}

local _format = string.format
local _type = type

local _behaviorNodeDict = {}
local _behaviorTreeDict = {}
local _globalVariables = {}

function behaviorManager:startTick(interval)
	self.interval = interval or 0.2
	if self.timer == nil then
		self.timer = timer.new()
		self.timer:start(self.interval, function()
			self:tick()
		end)
		print('[behavior] 开始心跳')
	end
end

function behaviorManager:stopTick()
	if self.timer then
		self.timer:cancel()
		self.timer = nil
		print('[behavior] 停止心跳')
	end
end

function behaviorManager:cleanTree()
	_behaviorTreeDict = {}
end

local _genBehaviorTree
_genBehaviorTree = function(json, parent, tree)
	if _behaviorNodeDict[json.name] == nil then
		_behaviorNodeDict[json.name] = true
		require(_format('lua.behavior.%s.%s', json.type, json.name))
	end
	local class = _G[json.name]
	if class then
		local node = class:new(tree, json.data)
		parent:addChild(node)
		if json.children then
			for i, v in ipairs(json.children) do
				_genBehaviorTree(v, node, tree)
			end
		end
	end
end

function behaviorManager:loadBehaviorTree(treeName, guid)
	local json = require(_format('behavior/%s', treeName))
	if json then
		json.data.guid = guid
		local tree = behaviorTree:new(nil, json.data)
		_genBehaviorTree(json.children[1], tree, tree)
		return tree
	end
end

function behaviorManager:bindBehaviorTree(btName, guid)
	local bt = _behaviorTreeDict[guid]
	if bt then
		print('实体已经绑定了行为树, guid:', guid)
	else
		bt = self:loadBehaviorTree(btName, guid)
		if bt == nil then
			logErr('找不到行为树:', btName)
			return
		end
		_behaviorTreeDict[guid] = bt
	end
end

function behaviorManager:unBindBehaviorTree(guid)
	_behaviorTreeDict[guid] = nil
	print('解绑行为树, guid:', guid)
end

function behaviorManager:getBehaviorTree(guid)
	return _behaviorTreeDict[guid]
end

function behaviorManager:tick()
	for i, bt in pairs(_behaviorTreeDict) do
		bt:tick()
	end
end

function behaviorManager:setGlobalVar(key, value)
	_globalVariables[key] = value
end

function behaviorManager:getGlobalVar(key)
	return _globalVariables[key]
end