---
--- Created by Hugo
--- DateTime: 2020/11/23 19:06
---

---@class WeightNode : ActionNode
WeightNode = BaseClass(ActionNode)

function WeightNode:Start()
	local weight = self.data.weight or 0
	local score = math.random(0, 1000)
	if score < weight then
		--self:print("随机为真")
		return eNodeState.Success
	else
		--self:print("随机为假")
		return eNodeState.Failure
	end
end

