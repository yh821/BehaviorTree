---
--- Created by Hugo
--- DateTime: 2020/10/26 10:12
---

---@class BehaviorTree : BaseClass
---@field child TaskNode
---@field blackBoard table
---@field gameObject userdata
BehaviorTree = BaseClass()

local _id = 0

function BehaviorTree:__init(data, file)
    _id = _id + 1
    self.uid = _id
    self.data = data
    self.file = file
    self:Awake()
end

function BehaviorTree:__delete()
    self:Clear()
    self.child = nil
    self.blackBoard = nil
    self.gameObject = nil
end

function BehaviorTree:Awake()
    self.child = nil
    self.blackBoard = nil
    self.child_count = 0
    self.restart = self.data.restart == 1
end

function BehaviorTree:Update(delta_time)
    if self.child:IsNotExecuted() or self.child:IsRunning() then
        self.child:SetState(self.child:Tick(delta_time))
    elseif self.restart then
        self:Reset()
    end
end

---@param node TaskNode
function BehaviorTree:AddChild(node)
    self.child = node
end

---@return TaskNode
function BehaviorTree:GetChildren()
    return self.child
end

---@type fun(parent:TaskNode)
local __ResetNode
__ResetNode = function(node)
    local children = node:GetChildren()
    if children then
        for _, v in ipairs(children) do
            __ResetNode(v)
        end
    end
    node:Reset()
    node._state = nil
end

function BehaviorTree:Reset()
    __ResetNode(self.child)
end

---@type fun(node:TaskNode)
local __RecycleNode
__RecycleNode = function(node)
    local children = node:GetChildren()
    if children then
        for _, v in ipairs(children) do
            __RecycleNode(v)
        end
    end
    --回收节点
    BehaviorManager.RecycleNode(node)
end

function BehaviorTree:Clear()
    __RecycleNode(self.child)
    self.blackBoard = nil
end

---@return table
function BehaviorTree:GetBlackboard()
    if self.blackBoard == nil then
        self.blackBoard = {}
    end
    return self.blackBoard
end

---@param key string
---@param value any
function BehaviorTree:SetSharedVar(key, value)
    local bb = self:GetBlackboard()
    bb[key] = value
end

---@param key string
---@return any
function BehaviorTree:GetSharedVar(key)
    local bb = self:GetBlackboard()
    return bb[key]
end

---@param key string
---@return any
function BehaviorTree:PopSharedVar(key)
    local bb = self:GetBlackboard()
    local value = bb[key]
    bb[key] = nil
    return value
end

---@param key string
---@param value any
function BehaviorTree:SetGlobalVar(key, value)
    BehaviorManager:SetGlobalVar(key, value)
end

---@param key string
---@return any
function BehaviorTree:GetGlobalVar(key)
    return BehaviorManager:GetGlobalVar(key)
end

---@param key string
---@return any
function BehaviorTree:PopGlobalVar(key)
    return BehaviorManager:PopGlobalVar(key)
end

---@param value number
function BehaviorTree:SetTrigger(value)
    self:SetSharedVar(BtConfig.trigger, value)
end

---@return number
function BehaviorTree:GetTrigger()
    return self:GetSharedVar(BtConfig.trigger)
end

function BehaviorTree:IsComposite()
    return false
end

------------------------------------------------------------------------

function BehaviorTree:GetSelfMoveObjPos()
    local scene_obj = self:GetSharedVar(BtConfig.self_obj_key)
    if scene_obj then
        local draw_obj = scene_obj:GetDrawObj()
        if draw_obj then
            local pos = draw_obj:GetRoot().transform.position
            self:SetSharedVar(BtConfig.self_pos_key, pos)
            return pos
        end
    end
end

function BehaviorTree:GetTargetMoveObjPos()
    local scene_obj = self:GetSharedVar(BtConfig.target_obj_key)
    if scene_obj then
        local draw_obj = scene_obj:GetDrawObj()
        if draw_obj then
            local pos = draw_obj:GetRoot().transform.position
            self:SetSharedVar(BtConfig.target_pos_key, pos)
            return pos
        end
    end
end