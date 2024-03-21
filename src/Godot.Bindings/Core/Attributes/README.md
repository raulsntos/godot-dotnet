# Godot.Bindings (Attributes)

This directory contains the attributes that are consumed by our analyzers and source generators, as such we need to follow some rules to avoid adding complexity to those projects.

- Attributes can only have one constructor. \
	This allows us to assume the signature of the attribute constructor when consuming it from analyzers.

- Required data must be parameters in the constructor, and should be stored in public read-only properties. \
	The property is not really needed since we won't consume it from analyzers, but it ensures that consumers using reflection can get the same data.

- Optional data must not be parameters in the constructor, and should be public init-only properties. \
	This ensures there's only one way to set the value, which simplifies the implementation of analyzers.

- To avoid breaking compatibility, new data added to existing attributes must be optional. \
	We can't add constructor or modify existing constructors, so new data must be optional therefore it should be added as an init-only property.
