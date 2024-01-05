---
--- Created by Hugo
--- DateTime: 2023/5/11 15:53
---

---@class AttackNode : ActionNode
AttackNode = AttackNode or BaseClass(ActionNode)

function AttackNode:Start()
    local pos_key = self.data and self.data.pos
    local target_pos = self:GetSharedVar(pos_key)
    if not target_pos then
        target_pos = self:GetSharedVar(BtConfig.target_pos_key)
    end
    if not target_pos then
        return eNodeState.Failure
    end

    --local speed = self.data and self.data.speed or 5

    self.draw_obj = self:GetDrawObj()
    if not self.draw_obj then
        return eNodeState.Failure
    end

    self:CancelAttackTimer()
    self.draw_obj:RotateTo(target_pos, 10)
    self.attack_timer = TimerQuest.Instance:AddDelayTimer(function()
        self:OnSuccess()
    end, 1.5) --攻击间隔1.5秒
    self:print("开始攻击:", pos_key, target_pos:ToString())
    self.draw_obj:SetAnimParamMain(DrawObj.AnimParamType.Trigger, "attack")
    return eNodeState.Running
end

function AttackNode:CancelAttackTimer()
    if self.attack_timer then
        TimerQuest.Instance:CancelQuest(self.attack_timer)
    end
    self.attack_timer = nil
end

function AttackNode:Abort()
    self:CancelAttackTimer()
    if self.draw_obj then
        self.draw_obj:SetAnimParamMain(DrawObj.AnimParamType.ResetTrigger, "attack")
    end
    self:print("<color=red>打断攻击</color>")
    return eNodeState.Failure
end

function AttackNode:GetDrawObj()
    ---@type SceneObj
    local scene_obj = self:GetSharedVar(BtConfig.self_obj_key)
    if scene_obj then
        return scene_obj:GetDrawObj()
    end
end