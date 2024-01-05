---
--- Created by Hugo
--- DateTime: 2020/10/26 11:06
---

require("behavior/BehaviorConfig")

---@class BehaviorManager
BehaviorManager = {}

local _behaviorNodeDict = {}
---@type BehaviorTree[]
local _behaviorTreeDict = {}
local _globalVariables = {}

function BehaviorManager:SwitchTick()
    self._is_tick = not self._is_tick
    if self._is_tick then
        print_log("[behaviorManager] 开始心跳")
    else
        print_log("[behaviorManager] 停止心跳")
        self:CleanTree()
    end
end

function BehaviorManager:CleanTree()
    for _, bt in pairs(_behaviorTreeDict) do
        --TODO 考虑回收行为树
        bt:DeleteMe()
    end
    _behaviorTreeDict = {}
end

function BehaviorManager:CleanGlobalVar()
    _globalVariables = {}
end

---简单的池子
BehaviorManager._node_pool = {}
---@param node TaskNode
function BehaviorManager.Recycle(node)
    node:Clear()
    local class = node.file
    local class_pool = BehaviorManager._node_pool[class]
    if class_pool == nil then
        class_pool = {}
        BehaviorManager._node_pool[class] = class_pool
    end
    --TODO 考虑池子上限
    table.insert(class_pool, node)
end

---@param file string
---@param data table
---@param tree BehaviorTree
---@return TaskNode
function BehaviorManager.CreateNode(file, data, parent, tree)
    local class_pool = BehaviorManager._node_pool[file]
    if class_pool == nil or #class_pool == 0 then
        ---@type TaskNode
        local class = _G[file]
        if class then
            return class.New(file, data, parent, tree)
        end
    else
        ---@type TaskNode
        local node = table.remove(class_pool)
        node:__Awake(file, data, parent, tree)
        return node
    end
end

---@type fun(json:table, parent:TaskNode, tree:BehaviorTree)
local __GenBehaviorTree
__GenBehaviorTree = function(json, parent, tree)
    if _behaviorNodeDict[json.file] == nil then
        _behaviorNodeDict[json.file] = true
        require("behavior/nodes/" .. json.type)
    end
    local node = BehaviorManager.CreateNode(json.file, json.data, parent, tree)
    if node then
        parent:AddChild(node)
        if json.children then
            for _, v in ipairs(json.children) do
                __GenBehaviorTree(v, node, tree)
            end
        end
    end
end

---@param file string
function BehaviorManager:__LoadBehaviorTree(file)
    local json = require(string.format("behavior/config/%s", file))
    if json then
        local bt = BehaviorTree.New(json.data, file)
        local root = json.children[1]
        __GenBehaviorTree(root, bt, bt)
        return bt
    end
end

---@param file string
---@return BehaviorTree
function BehaviorManager:BindBehaviorTree(gameObject, file)
    local bt = _behaviorTreeDict[gameObject]
    if bt then
        print_error("实体已经绑定了行为树:", bt.file)
        return
    end
    bt = self:__LoadBehaviorTree(file)
    if bt == nil then
        print_error("找不到行为树:", file)
        return
    end
    bt.gameObject = gameObject
    _behaviorTreeDict[gameObject] = bt
    return bt
end

function BehaviorManager:UnBindBehaviorTree(gameObject)
    local bt = _behaviorTreeDict[gameObject]
    if not bt then
        return
    end
    bt.gameObject = nil
    _behaviorTreeDict[gameObject] = nil
    print_log("已解绑行为树:", bt.file)
end

function BehaviorManager:GetBehaviorTree(gameObject)
    return _behaviorTreeDict[gameObject]
end

function BehaviorManager:Update(delta_time)
    if not self._is_tick then
        return
    end
    for _, bt in pairs(_behaviorTreeDict) do
        bt:Update(delta_time)
    end
end

function BehaviorManager:SetGlobalVar(key, value)
    _globalVariables[key] = value
end

function BehaviorManager:GetGlobalVar(key)
    return _globalVariables[key]
end

function BehaviorManager.ParseVector3(str, is_temp)
    local vec3 = Split(str, ",")
    if is_temp then
        return Vector3Pool.GetTemp(tonumber(vec3.x) or 0, tonumber(vec3.y) or 0, tonumber(vec3.z) or 0)
    else
        return Vector3.New(tonumber(vec3.x) or 0, tonumber(vec3.y) or 0, tonumber(vec3.z) or 0)
    end
end