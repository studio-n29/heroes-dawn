#if !NET5_0_OR_GREATER
// Permet les propriétés `init` et les records C# 9 sous netstandard2.1 (Unity).
namespace System.Runtime.CompilerServices { internal static class IsExternalInit { } }
#endif
