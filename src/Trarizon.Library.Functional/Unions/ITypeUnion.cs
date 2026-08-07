using System;
using System.Collections.Generic;
using System.Text;

namespace Trarizon.Library.Functional.Unions;

public interface ITypeUnion<T1,T2>
{
    bool IsNull { get; }
    T? As<T>();
}
