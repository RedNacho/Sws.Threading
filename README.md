Sws.Threading
=============

Based on Castle Dynamic Proxy.  Generates thread-safe wrappers for .NET objects.

Basic usage:

1. Add reference to Sws.Threading.
2. Import Sws.Threading.Extensions.
3. Use the ThreadSafeProxy extension method (creates a basic thread-safe proxy analogous to applying Java's "synchronized" modifier to every member), or the ConfigureThreadSafeProxy extension method (allows further configuration via a builder.

Please note that only virtual members can be proxied - Castle will try to subclass classes and implement interfaces.

See the Sws.Threading.Demo solution for an example of these methods in use.
