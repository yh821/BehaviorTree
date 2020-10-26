--[[
----------------------------------------------------
	created: 2020-10-26 11:06
	author : yuanhuan
	purpose: 
----------------------------------------------------
]]

behaviorManager = {}

local _behaviorTreeDict = {}

function behaviorManager:startTick(interval)
	self.interval = interval or 0.2
	if self.timer == nil then
		self.timer = timer.new()
		self.timer:start(self.interval, function()
			self:tick()
		end)
	end
end

function behaviorManager:stopTick()
	if self.timer then
		self.timer:cancel()
		self.timer = nil
	end
end

local _genBehaviorTree
_genBehaviorTree = function(json, parent, tree)
	local class = _G[json.nodeName]
	if class then
		local node = class:new(tree)
		parent:addChild(node)
		if json.children then
			for i, v in ipairs(json.children) do
				_genBehaviorTree(v, node, tree)
			end
		end
	end
end

function behaviorManager:loadBehaviorTree(treeName, guid)
	local json = jsonHelper.readFile(string.format('%s.json', treeName))
	if json then
		local tree = behaviorTree:new()
		tree:bind(guid, { restartOnComplete = true })
		_genBehaviorTree(json, tree, tree)
		return tree
	end
end

function behaviorManager:bindBehaviorTree(treeName, guid)
	local bt = _behaviorTreeDict[guid]
	if bt then
		print('实体已经绑定了行为树, guid:', guid)
	else
		bt = self:loadBehaviorTree(treeName, guid)
		if bt == nil then
			logErr('找不到行为树:', treeName)
			return
		end
		_behaviorTreeDict[guid] = bt
	end
end

function behaviorManager:getBehaviorTree(guid)
	return _behaviorTreeDict[guid]
end

function behaviorManager:tick()
	for i, bt in pairs(_behaviorTreeDict) do
		bt:tick()
	end
end