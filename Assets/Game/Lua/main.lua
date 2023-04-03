require 'common.baseClass'
require 'common.baseNode'
require 'common.behaviorTree'
require 'behaviorManager'

--local root = rootNode:new()
--local sequence = sequenceNode:new()
--local wait1 = waitAction:new()
--local wait2 = waitAction:new()
--local wait3 = waitAction:new()
--sequence:addChild(wait1)
--sequence:addChild(wait2)
--sequence:addChild(wait3)
--root:addChild(sequence)
--root:run()

local function new(self)
	local t = {}
	setmetatable(t, self)
	self.__index = self
	return t
end

local t1 = { name = 't1' }
local t2 = { name = 't2' }
local t3 = { name = 't3' }
local t4 = { name = 't4' }
local bt = { name = 'bt' }

bt.t1 = new(t1)
bt.t2 = new(t2)
bt.t3 = new(t3)
bt.t4 = new(t4)

local bt1 = new(bt)
local bt2 = new(bt)

print('bt1:', bt1)
print('bt2:', bt2)

print('bt1.t1:', bt1.t1)
print('bt2.t1:', bt2.t1)
print('bt1.t2:', bt1.t2)
print('bt2.t2:', bt2.t2)
print('bt1.t3:', bt1.t3)
print('bt2.t3:', bt2.t3)
print('bt1.t4:', bt1.t4)
print('bt2.t4:', bt2.t4)


