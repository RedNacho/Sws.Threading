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
