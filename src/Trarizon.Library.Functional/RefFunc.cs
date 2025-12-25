namespace Trarizon.Library.Functional;

#if NET9_0_OR_GREATER

// These delegates are used to make compiler know which overload to use
// We use Func<> if returned type is not ref struct, otherwise we use RefFunc<>
// If compiler could recognize which overload to use, we don't use this

public delegate TResult RefFunc<in T, out TResult>(T arg)
    where T : allows ref struct
    where TResult : allows ref struct;

public delegate TResult RefFunc<in T1, in T2, out TResult>(T1 arg1, T2 arg2)
    where T1 : allows ref struct
    where T2 : allows ref struct
    where TResult : allows ref struct;

#endif
