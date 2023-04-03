local _class = {}
local lua_obj_count = 0
--local class_file_path_table = {}

---@class BaseClass
---@field New fun(...):BaseClass
---@field DeleteMe fun():BaseClass

---@return BaseClass
function BaseClass(super)
	-- 生成一个类类型
	local class_type = {}
	class_type.__init = false
	class_type.__delete = false
	class_type.super = super
	class_type.New = function(...)
		lua_obj_count = lua_obj_count + 1
		-- 生成一个类对象
		local obj = { _class_type = class_type }
		setmetatable(obj, { __init = _class[class_type] })

		-- 初始化
		local _ctor
		_ctor = function(c, ...)
			if c.super then
				_ctor(c.super, ...)
			end
			if c.__init then
				c.__init(obj, ...)
			end
		end
		_ctor(class_type, ...)

		obj.DeleteMe = function(self)
			lua_obj_count = lua_obj_count - 1
			local now_super = self._class_type
			while now_super do
				if now_super.__delete then
					now_super.__delete(self)
				end
				now_super = now_super.super
			end
		end

		return obj
	end

	local vt = {}
	_class[class_type] = vt

	setmetatable(class_type, {})
	if super then
		setmetatable(vt, { __init = function(t, k)
			return _class[super][k]
		end })
	end

	return class_type
end