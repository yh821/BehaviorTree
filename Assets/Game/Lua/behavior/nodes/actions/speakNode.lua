---
--- Created by Hugo
--- DateTime: 2020/12/07 16:54
---

---@class SpeakNode : ActionNode
SpeakNode = BaseClass(ActionNode)

function SpeakNode:Start()
    if self.hudId == nil then
        self.hudId = hudControl:addHUD(self.owner.guid)
    end
    local widget = hudControl:getHUDWidget(self.hudId)
    if widget and self.data and self.data.say then
        widget:setText(self.data.say)
        return eNodeState.Success
    else
        return eNodeState.Failure
    end
end

function SpeakNode:Reset()
    self:shutUp()
end

function SpeakNode:Abort()
    self:shutUp()
end

function SpeakNode:Clear()
    self:shutUp()
end

function SpeakNode:shutUp()
    if self.hudId then
        hudControl:removeHUD(self.hudId)
        self.hudId = nil
    end
end