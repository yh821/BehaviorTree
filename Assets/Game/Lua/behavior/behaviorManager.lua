---
--- Created by Hugo
--- DateTime: 2020/10/26 11:06
---

require("behavior/BehaviorConfig")

---@class BehaviorManager
BehaviorManager = {}

local _json_file_map = {}
local _node_file_map = {}
---@type BehaviorTree[]
local _behavior_tree_map = {}
local _globalVariables = {}

function BehaviorManager:SwitchTick(thinking)
    if self._is_tick == thinking then
        return
    end
    if thinking == nil then
        self._is_tick = not self._is_tick
    else
        self._is_tick = thinking
    end
    if self._is_tick then
        print_log("[behaviorManager] 开始心跳")
    else
        print_log("[behaviorManager] 停止心跳")
    end
end

function BehaviorManager:StopTick()
    self:SwitchTick(false)
    self:DestroyAll()
end

function BehaviorManager:DestroyAll()
    for _, bt in pairs(_behavior_tree_map) do
        bt:DeleteMe()
    end
    _behavior_tree_map = {}
    _json_file_map = {}
    _node_file_map = {}
    _globalVariables = {}

    for _, pool in pairs(BehaviorManager._tree_pool_map) do
        for _, tree in ipairs(pool) do
            tree:DeleteMe()
        end
    end
    BehaviorManager._tree_pool_map = {}

    for _, pool in pairs(BehaviorManager._node_pool_map) do
        for _, node in ipairs(pool) do
            node:DeleteMe()
        end
    end
    BehaviorManager._node_pool_map = {}
end

function BehaviorManager:CleanGlobalVar()
    _globalVariables = {}
end

---简单的池子
BehaviorManager._node_pool_map = {}
local function __GetNodePool(file)
    local pool = BehaviorManager._node_pool_map[file]
    if pool == nil then
        pool = {}
        BehaviorManager._node_pool_map[file] = pool
    end
    return pool
end

---@param node TaskNode
function BehaviorManager.RecycleNode(node)
    if not node then
        return
    end
    node._state = nil
    node.parent = nil
    node.owner = nil
    node.data = nil
    node:Clear()
    --TODO 考虑池子上限
    table.insert(__GetNodePool(node.file), node)
end

---@param file string
---@param data table
---@param tree BehaviorTree
---@return TaskNode
function BehaviorManager.CreateNode(file, data, parent, tree)
    ---@type TaskNode
    local node = table.remove(__GetNodePool(file))
    if node then
        node:Awake(file, data, parent, tree)
    else
        local class = _G[file]
        if class then
            node = class.New(file, data, parent, tree)
        end
    end
    return node
end

---简单的池子
BehaviorManager._tree_pool_map = {}
local function __GetTreePool(file)
    local pool = BehaviorManager._tree_pool_map[file]
    if pool == nil then
        pool = {}
        BehaviorManager._tree_pool_map[file] = pool
    end
    return pool
end

---@param tree BehaviorTree
function BehaviorManager.RecycleTree(tree)
    if not tree then
        return
    end
    tree:Clear()
    --TODO 考虑池子上限
    table.insert(__GetTreePool(tree.file), tree)
end

---@type fun(json:table, parent:TaskNode, tree:BehaviorTree)
local __GenBehaviorTree
__GenBehaviorTree = function(json, parent, tree)
    if _node_file_map[json.file] == nil then
        _node_file_map[json.file] = true
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
function BehaviorManager:CreateTree(file)
    local json = _json_file_map[file]
    if not json then
        json = require(string.format("behavior/config/%s", file))
        if not json then
            return
        end
        _json_file_map[file] = json
    end
    ---@type BehaviorTree
    local bt = table.remove(__GetTreePool(file))
    if bt then
        bt:Awake()
    else
        bt = BehaviorTree.New(json.data, file)
    end
    __GenBehaviorTree(json.children[1], bt, bt)
    if json.sharedData then
        for k, v in pairs(json.sharedData) do
            bt:SetSharedVar(k, v)
        end
    end
    return bt
end

---@param file string
---@return BehaviorTree
function BehaviorManager:BindBehaviorTree(gameObject, file, shared_data)
    local bt = _behavior_tree_map[gameObject]
    if bt then
        print_error("实体已经绑定了行为树:", bt.file)
        return
    end
    bt = self:CreateTree(file)
    if bt == nil then
        print_error("找不到行为树:", file)
        return
    end
    if shared_data and type(shared_data) == "table" then
        for k, v in pairs(shared_data) do
            bt:SetSharedVar(k, v)
        end
    end
    bt.gameObject = gameObject
    _behavior_tree_map[gameObject] = bt
    return bt
end

function BehaviorManager:UnBindBehaviorTree(gameObject)
    local bt = _behavior_tree_map[gameObject]
    if not bt then
        return
    end
    bt.gameObject = nil
    _behavior_tree_map[gameObject] = nil
    BehaviorManager.RecycleTree(bt)
    print_log("已解绑行为树:", bt.file)
end

function BehaviorManager:GetBehaviorTree(gameObject)
    return _behavior_tree_map[gameObject]
end

function BehaviorManager:Update(delta_time)
    if not self._is_tick then
        return
    end
    for _, bt in pairs(_behavior_tree_map) do
        bt:Update(delta_time)
    end
end

function BehaviorManager:SetGlobalVar(key, value)
    _globalVariables[key] = value
end

function BehaviorManager:GetGlobalVar(key)
    return _globalVariables[key]
end

function BehaviorManager:PopGlobalVar(key)
    local value = _globalVariables[key]
    _globalVariables[key] = nil
    return value
end

function BehaviorManager.ParseVector3(str, is_temp)
    local vec3 = Split(str, ",")
    if is_temp then
        return Vector3Pool.GetTemp(tonumber(vec3.x) or 0, tonumber(vec3.y) or 0, tonumber(vec3.z) or 0)
    else
        return Vector3.New(tonumber(vec3.x) or 0, tonumber(vec3.y) or 0, tonumber(vec3.z) or 0)
    end
end