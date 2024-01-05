---
--- Created by Hugo
--- DateTime: 2020/11/23 19:12
---

---@class MoveToPositionNode : ActionNode
MoveToPositionNode = BaseClass(ActionNode)

function MoveToPositionNode:Start()
    local pos_key = self.data and self.data.pos
    local target_pos = self:GetSharedVar(pos_key)
    if not target_pos then
        target_pos = self:GetSharedVar(BtConfig.target_pos_key)
    end
    if not target_pos then
        return eNodeState.Failure
    end

    local speed = self.data and self.data.speed or 5

    self.draw_obj = self:GetDrawObj()
    if not self.draw_obj then
        return eNodeState.Failure
    end

    self.draw_obj:RotateTo(target_pos, 10)
    self.draw_obj:MoveTo(target_pos, speed, function()
        self.draw_obj:SetAnimParamMain(DrawObj.AnimParamType.Boolean, "move", false)
        self:OnSuccess()
    end)
    --self:print("开始移动:", pos_key, target_pos:ToString())
    self.draw_obj:SetAnimParamMain(DrawObj.AnimParamType.Boolean, "move", true)
    return eNodeState.Running
end

function MoveToPositionNode:Abort()
    if self.draw_obj then
        self.draw_obj:StopMove()
        self.draw_obj:SetAnimParamMain(DrawObj.AnimParamType.Boolean, "move", false)
    end
    self:print("<color=red>打断移动</color>")
    return eNodeState.Failure
end

function MoveToPositionNode:GetDrawObj()
    ---@type SceneObj
    local scene_obj = self:GetSharedVar(BtConfig.self_obj_key)
    if scene_obj then
        return scene_obj:GetDrawObj()
    end
end