Sws.Threading
=============

Based on Castle Dynamic Proxy.  Generates thread-safe wrappers for .NET objects.

Basic usage:

1. Add reference to Sws.Threading.
2. Import Sws.Threading.Extensions.
3. Use the ThreadSafeProxy extension method (creates a basic thread-safe proxy analogous to applying Java's "synchronized" modifier to every member), or the ConfigureThreadSafeProxy extension method (allows further configuration via a builder).
