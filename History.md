# History

## v1.2.0

- New
	- Add conversion between wrappers
	- Add `ArrayFiller`
- Changes
	- Use Exception static methods in .NET 8 to replace part of `ThrowHelper`
- Breaking
	- Remove target framework .NET Standard 2.0 (~~����Դ�����������ã������ʲôʱ��Դ������֧���°�.NET��.jpg~~
	- `Span.Reverse` is moved to `SpanQuery`
- Bug fix
	- Exception judge in `IEnumerable.CountsBetween`
