using System;
using System.Collections.Generic;
using System.Text;

namespace Trarizon.Library.Functional.Abstraction;

public interface ITypeUnion<T1,T2>
{
    bool IsNull { get; }
    T? As<T>();
}
