---
--- Created by Hugo
--- DateTime: 2020/10/26 10:12
---

---@class eNodeState
eNodeState = {
    Failure = 0,
    Success = 1,
    Running = 2,
}

---@class TaskNode : BaseClass
---@field parent ParentNode
---@field owner BehaviorTree
---@field uid number
---@field data table
---@field _state eNodeState
TaskNode = BaseClass()

local _id = 0
local _openLog = true
local _format = string.format

---@param owner BehaviorTree
---@param data table
function TaskNode:__init(file, data, parent, owner)
    _id = _id + 1
    self.uid = _id
    self:Awake(file, data, parent, owner)
end

function TaskNode:Awake(file, data, parent, owner)
    self.file = file
    self.data = data
    self.parent = parent
    self.owner = owner
end

---@return eNodeState
function TaskNode:Start()
    --override
end

---@return eNodeState
function TaskNode:Tick(delta_time)
    if self:IsNotExecuted() then
        self:SetState(self:Start())
    end
    if self:IsNotExecuted() or self:IsRunning() then
        self:SetState(self:Update(delta_time))
    end
    return self:GetState()
end

---@return eNodeState
function TaskNode:Update(delta_time)
    return eNodeState.Running
end

---@return boolean
function TaskNode:IsExecuted()
    return self._state ~= nil
end

---@return boolean
function TaskNode:IsNotExecuted()
    return self._state == nil
end

---@return boolean
function TaskNode:IsRunning()
    return self._state == eNodeState.Running
end

---@return boolean
function TaskNode:IsSucceed()
    return self._state == eNodeState.Success
end

---@return boolean
function TaskNode:IsFailed()
    return self._state == eNodeState.Failure
end

---@return number
function TaskNode:GetState()
    return self._state
end

---@return boolean @是否发生改变, 变成非nil不算
function TaskNode:SetState(state, check_nil)
    if check_nil and state == nil then
        return false
    end
    local is_change = self._state ~= nil and self._state ~= state
    self._state = state
    --self:print("SetState:" .. (self._state or "nil"))
    return is_change
end

function TaskNode:Reset()
    --override
end

function TaskNode:Abort()
    --override
end

function TaskNode:Clear()
    --override
end

---@param node TaskNode
function TaskNode:AddChild(node)
    --override
end

---@return TaskNode[]
function TaskNode:GetChildren()
    --override
end

function TaskNode:IsAction()
    return false
end

function TaskNode:IsCondition()
    return false
end

function TaskNode:IsComposite()
    return false
end

function TaskNode:IsDecorator()
    return false
end

---@param key string
---@param value any
function TaskNode:SetSharedVar(key, value)
    self.owner:SetSharedVar(key, value)
end

---@param key string
---@return any
function TaskNode:GetSharedVar(key)
    return self.owner:GetSharedVar(key)
end

---@param key string
---@return any
function TaskNode:PopSharedVar(key)
    return self.owner:PopSharedVar(key)
end

---@param key string
---@param value any
function TaskNode:SetGlobalVar(key, value)
    return self.owner:SetGlobalVar(key, value)
end

---@param key string
---@return any
function TaskNode:GetGlobalVar(key)
    return self.owner:GetGlobalVar(key)
end

---@param key string
---@return any
function TaskNode:PopGlobalVar(key)
    return self.owner:PopGlobalVar(key)
end

---@param value number
function TaskNode:SetTrigger(value)
    self.owner:SetTrigger(value)
end

---@return number
function TaskNode:GetTrigger()
    return self.owner:GetTrigger()
end

function TaskNode:print(...)
    if _openLog then
        print_log(_format("[<color=yellow>%s</color>]", self.uid), ...)
    end
end

function TaskNode:print_error(...)
    if _openLog then
        print_error(_format("[<color=yellow>%s</color>]", self.uid), ...)
    end
end