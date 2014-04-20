FoundationDBSerializer
======================

This library allows users to easily serialize objects into FoundationDB's key-value store as documents and retrieve populated objects given the key of the objects.

The only modifications that are required for the classes being serialized is to have the FoundationDbSerializer.Key attribute defined on one of the properties in the class.

The library currently only serializes primitive and string properties/fields of classes, but in future versions support for nested classes will be supported.
