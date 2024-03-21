extends Node

func _ready() -> void:
	var s = Summator.new()
	s.Add(10)
	s.Add(20)
	s.Add(30)
	print(s.GetTotal())
	s.Reset()
