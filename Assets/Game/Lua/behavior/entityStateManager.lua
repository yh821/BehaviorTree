--[[
----------------------------------------------------
	created: 2020-11-30 21:37
	author : yuanhuan
	purpose: 
----------------------------------------------------
]]
---@class entityStateManager
entityStateManager = {}

local _entityBTDict = {}

function entityStateManager:addBehaviorTree(uid, entId)
	local bt = behaviorManager:bindBehaviorTree('bt_dizi', entId)
	bt:setStateId(behaviorStateEnum.eIdle) --设置默认状态
	_entityBTDict[uid] = bt
end

function entityStateManager:setStateId(entId, stateId)
	local bt = behaviorManager:getBehaviorTree(entId)
	if bt then
		bt:broke()
		bt:setStateId(stateId)
	else
		print(string.format('实体[%s]没有绑定行为树', entId))
	end
end