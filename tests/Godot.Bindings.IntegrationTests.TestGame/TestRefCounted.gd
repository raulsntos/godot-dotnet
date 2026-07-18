extends "res://TestBase.gd"


func _ready():
	# Test getting the reference count on the GDScript side after creating a C# RefCounted from GDScript.
	var refcounted1 = TestRefCounted.new()
	assert_equal(refcounted1.get_reference_count(), 1)

	# Test getting the reference count on the GDScript side after crossing the interop boundary.
	# The count is 2: the C# wrapper for the object created in create_from_csharp() holds one
	# reference, and the engine's Ref<T> that received the ptrcall return holds another. Before
	# the reference-counting fix the return dropped the engine's reference (leaving an un-counted
	# reference held by GDScript), so this read 1 and the object could be freed out from under it.
	var refcounted2 = TestRefCounted.create_from_csharp()
	assert_equal(refcounted2.get_reference_count(), 2)
	# Disposing the C# wrapper releases only its reference; the engine's counted reference survives,
	# so the object is still alive at count 1 (this is exactly what the fix guarantees).
	TestRefCounted.test_dispose_refcounted(refcounted2)
	assert_equal(refcounted2.get_reference_count(), 1)

	# Test getting the reference count on the C# side after crossing the interop boundary.
	var curve1 = Curve2D.new()
	assert_equal(TestRefCounted.test_get_reference_count(curve1), 2)

	# Test getting the reference count on the C# side when RefCounted is masked as a GodotObject.
	var curve2 = Curve2D.new()
	assert_equal(TestRefCounted.test_get_reference_count_for_object(curve2), 2)

	# Test getting the reference count on the C# side when RefCounted is wrapped in a Variant.
	var curve3 = Curve2D.new()
	assert_equal(TestRefCounted.test_get_reference_count_for_variant(curve3), 3)

	# Test creating a RefCounted and getting the reference count without crossing the interop boundary.
	assert_equal(TestRefCounted.test_create_and_get_refcount(), 1)

	# Multiple RefCounted instances for the same reference.
	var file1 = FileAccess.open("res://empty.txt", FileAccess.READ)
	var file2 = FileAccess.open("res://empty.txt", FileAccess.READ)
	var file1_refcount = file1.get_reference_count()
	var file2_refcount = file2.get_reference_count()
	print("File 1 (GD) refcount: %s" % [file1_refcount])
	print("File 2 (GD) refcount: %s" % [file2_refcount])
	assert_equal(file1.get_reference_count(), 1)
	assert_equal(file1_refcount, file2_refcount)
	file1 = null
	file2 = null
	var refcounts = TestRefCounted.test_multiple_refcounted_same_reference()
	assert_equal(refcounts.size(), 2)
	print("File 1 (C#) refcount: %s" % [refcounts[0]])
	print("File 2 (C#) refcount: %s" % [refcounts[1]])
	assert_equal(file1_refcount, refcounts[0])
	assert_equal(file2_refcount, refcounts[1])
	assert_equal(refcounts[0], refcounts[1])

	# Test disposing a RefCounted on the C# side only decrements the reference on the C# side.
	var refcounted3 = TestRefCounted.new()
	assert_equal(refcounted3.get_reference_count(), 1)
	assert_equal(TestRefCounted.test_get_reference_count(refcounted3), 2)
	TestRefCounted.test_dispose_refcounted(refcounted3)
	assert_equal(refcounted3.get_reference_count(), 1)
	var refcounted4 = Resource.new()
	assert_equal(refcounted4.get_reference_count(), 1)
	assert_equal(TestRefCounted.test_get_reference_count(refcounted4), 2)
	TestRefCounted.test_dispose_refcounted(refcounted4)
	assert_equal(refcounted4.get_reference_count(), 1)

	# Test passing the same RefCounted to the C# side after disposing it on the C# side.
	# When a user-defined type like TestRefCounted is disposed, since the implementation lives on the C# side,
	# the extension is "detached" from the native instance. Notice how the type name changes after disposal.
	var refcounted5 = TestRefCounted.new()
	assert_equal(refcounted5.get_reference_count(), 1)
	assert_equal(TestRefCounted.test_get_type_name(refcounted5), "TestRefCounted")
	TestRefCounted.test_dispose_refcounted(refcounted5)
	assert_equal(refcounted5.get_reference_count(), 1)
	assert_equal(TestRefCounted.test_get_type_name(refcounted5), "RefCounted")
	TestRefCounted.test_dispose_refcounted(refcounted5)
	assert_equal(refcounted5.get_reference_count(), 1)
	assert_equal(TestRefCounted.test_get_type_name(refcounted5), "RefCounted")
	var refcounted6 = Resource.new()
	assert_equal(refcounted6.get_reference_count(), 1)
	assert_equal(TestRefCounted.test_get_type_name(refcounted6), "Resource")
	TestRefCounted.test_dispose_refcounted(refcounted6)
	assert_equal(refcounted6.get_reference_count(), 1)
	assert_equal(TestRefCounted.test_get_type_name(refcounted6), "Resource")
	TestRefCounted.test_dispose_refcounted(refcounted6)
	assert_equal(refcounted6.get_reference_count(), 1)
	assert_equal(TestRefCounted.test_get_type_name(refcounted6), "Resource")

	# Test disposing a non-RefCounted on the C# side only frees the wrapper but does not destroy the actual object.
	var node = Node.new()
	var node_id = node.get_instance_id()
	TestRefCounted.test_dispose_object(node)
	assert_equal(is_instance_id_valid(node_id), true)
	node.queue_free()

	exit_with_status()
