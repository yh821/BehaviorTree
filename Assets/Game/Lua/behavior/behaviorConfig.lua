---
--- Created by Hugo
--- DateTime: 2023/4/4 23:04
---

require("behavior/base/TaskNode")
require("behavior/base/BehaviorTree")
require("behavior/base/ActionNode")
require("behavior/base/ParentNode")
require("behavior/base/CompositeNode")
require("behavior/base/DecoratorNode")
require("behavior/base/ConditionNode")

BtConfig = {
    trigger = "triggerValue",
    triggerType = "triggerType",
    self_obj_key = "self_obj_key",
    target_obj_key = "target_obj_key",

    self_pos_key = "self_pos_key",
    target_pos_key = "target_pos_key",
    random_pos_key = "random_pos_key",

    view_range_key = "view_range_key",
}

eAbortType = {
    None = 0,
    Self = 1,
    Lower = 2,
    Both = 3
}

eTriggerType = {
    Equals = 0,
    NotEquals = 1,
    Greater = 2,
    Less = 3,
}

AnimatorStateEnum = {
    eIdle = 0,
    eWalk = 1,
}

PlayStateEnum = {
    eStart = 0,
    eEnd = 1,
}

BehaviorStateEnum = {
    eIdle = 0, --空闲
    eClick = 1, --点中
}