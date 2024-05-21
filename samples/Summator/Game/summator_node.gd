extends SummatorNode


# Called when the node enters the scene tree for the first time.
func _ready():
	Add(10)
	Add(20)
	Add(30)
	print("Node " , GetTotal())
	Reset()
	pass # Replace with function body.


# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta):
	pass
