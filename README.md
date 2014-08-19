Sws.Threading
=============

Based on Castle Dynamic Proxy.  Generates thread-safe wrappers for .NET objects.

Basic usage:

1. Add reference to Sws.Threading.
2. Import Sws.Threading.Extensions.
3. Use the ThreadSafeProxy extension method (creates a basic thread-safe proxy analogous to applying Java's "synchronized" modifier to every member), or the ConfigureThreadSafeProxy extension method (allows further configuration via a builder).

Please note that only virtual members can be proxied - Castle will try to subclass classes and implement interfaces.

You can chain calls to the extension methods to build up proxies with more complicated synchronisation requirements.

E.g.
obj = obj.ConfigureThreadSafeProxy().ForMember(proxy => proxy.SomeMethod()).Build().ConfigureThreadSafeProxy().Except().ForMember(proxy => proxy.SomeMethod()).Build() will have the effect of thread-safing SomeMethod separately to all of the other members of the object.

Conversely, you can synchronise across multiple objects by explicitly specifying an object to synchronise on.

E.g.
var lockingObject = new object();
obj1 = obj1.ConfigureThreadSafeProxy().WithLockingObject(lockingObject).Build();
obj2 = obj2.ConfigureThreadSafeProxy().WithLockingObject(lockingObject).Build();

Finally, you can override the synchronisation mechanism itself as follows:

obj = obj.ConfigureThreadSafeProxy().WithLockFactory(lockingObject => new MyLockImplementation(lockingObject));

See the Sws.Threading.Demo solution for an example of the extension methods in use.

Builder Quick Start
===================

obj.ConfigureThreadSafeProxy()
  .[Configuration Method Call]
  .[Configuration Method Call]
  .Build();

No method calls - equivalent to calling the obj.ThreadSafeProxy() extension method.  Effectively wraps the code for every virtual method/property with the "lock" keyword called on a dedicated locking object.

.ForMember/ForMembers(...) - specifies that locking will be applied to the specified properties/methods.  Other methods will be left as-is.  ForSetter and ForGetter can also be used with property lambda expressions to restrict the locking to the setter or the getter only.  You can call this any number of times to include more members.

.Except().ForMember/ForMembers(...) - as above, but specifies that locking will NOT be applied to the specified properties/methods.

.WithLockingObject(lockingObject) - causes locks to be created on a specific object, ie "lock (lockingObject) { }".  If you don't specify this, each call to Build() uses its own dedicated locking object.  WithLockingObject is primarily intended to allow the same object to be locked across multiple proxies.

.WithLockFactory(lockFactory) - causes all subsequent calls to Build() to create a lock on the locking object with the supplied lockFactory (ie replacing the "lock" keyword with something else).  You must supply a factory method because the locking object itself may vary, but the method will only be invoked once for each Build() call - one ILock instance will be used across the entire proxy.

.WithThreadSafeProxyFactory(threadSafeProxyFactory) - allows the underlying mechanism for creating thread-safe proxies to be swapped out (not really expected usage, provided for extensibility).  Standard implementation is ThreadSafeProxyFactory using Castle Dynamic Proxy for proxy generation.
