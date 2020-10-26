function simple_class(base)
	local _class = {}
	if base then
		for i, v in pairs(base) do
			_class[i] = v
		end
		_class._base = base
	end
	_class.__index = _class
	_class.__init = false
	_class.__delete = false

	local mt = {}
	mt.__call = function(class_tbl, ...)
		local obj = {}
		setmetatable(obj, _class)
		-- inheri constructor
		local _ctor
		_ctor = function(_c, ...)
			if _c._base then
				_ctor(_c._base, ...)
			end
			if _c.__init then
				_c.__init(obj, ...)
			end
		end
		obj._class = _class
		_ctor(_class, ...)
		return obj
	end

	_class.is_a = function(self, klass)
		local m = getmetatable(self)
		while m do
			if m == klass then
				return true
			end
			m = m._base
		end
		return false
	end

	_class.deleteSelf = function(self)
		if self.__deleted__ then
			assert(false)
		end
		local _this_class = self._class
		while _this_class ~= nil do
			if _this_class.__delete then
				_this_class.__delete(self)
			end
			_this_class = _this_class._base
		end
		self.__deleted__ = true;
	end

	setmetatable(_class, mt)
	return _class
end