#pragma warning disable TRAEXP

using CommunityToolkit.HighPerformance.Buffers;
using System.Collections;
using System.Collections.Specialized;
using Trarizon.Library.Collections;
using Trarizon.Library.Collections.Generic;
using Trarizon.Library.Components;
using Trarizon.Library.Wrappers;
using Trarizon.Test.Run;


EventBus.UseDictionaryByDefault = false;

EventBus.AddListener((int a) => Console.WriteLine(a));

EventBus.Invoke(2);