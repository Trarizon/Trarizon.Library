using BenchmarkDotNet.Attributes;
using System.Runtime.InteropServices;
using System.Text;
using Trarizon.Library.Collections.Extensions;
using RtnType = string;

namespace Trarizon.Test.Run;
[MemoryDiagnoser]
public class Benchmarks
{
	public IEnumerable<object> ArgsSource()
	{
		yield return Args().ToArray();

		IEnumerable<object> Args()
		{
			yield return Enumerable.Range(0, 8).Select(i => $"{i}_string").ToArray();
		}
	}


	[Benchmark]
	[ArgumentsSource(nameof(ArgsSource))]
	public RtnType StackAndCacheLength(IEnumerable<string> strs)
	{
		var stack = new Stack<(char, string)>();
		int strLength = 0;

		int i = 0;
		foreach (var str in strs) {
			stack.Push((i < 2 ? '+' : '.', str));
			strLength += str.Length + 1;
			i++;
		}

		var result = (stackalloc char[strLength]);
		strLength = 0;
		foreach (var (c, str) in stack) {
			result[strLength++] = c;
			str.CopyTo(result[strLength..]);
			strLength += str.Length;
		}
		return result[1..].ToString();
	}

	[Benchmark]
	[ArgumentsSource(nameof(ArgsSource))]
	public RtnType StringBuilder(IEnumerable<string> strs)
	{
		var sb = new StringBuilder();

		int i = 0;
		foreach (var str in strs) {
			if (i < 2) {
				sb.Insert(0, '+');
			}
			else if (i != 7) {
				sb.Insert(0, '.');
			}
			sb.Insert(0, str);
			i++;
		}
		return sb.ToString();
	}
}
