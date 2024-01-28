# History

## v1.2.0

- New
	- Add conversion between wrappers
	- Add `ArrayFiller<>`
	- Add `Either<,>`
	- Add `IList<>.TryAt()`��`IROList<>.TryAt()`
	- Add `Optional<>.Unwrap`
- Changes
	- Use Exception static methods in .NET 8 to replace part of `ThrowHelper`
- Breaking
	- Remove target framework .NET Standard 2.0 (~~����Դ�����������ã������ʲôʱ��Դ������֧���°�.NET��.jpg~~
	- `Span.Reverse` is moved to `SpanQuery`
	- Remove `IList<>.AtOrDefault()`
	- Rename `IReadOnlyList<>.AtOrDefault()` to `ElementAtOrDefault`
- Bug fix
	- Exception judge in `IEnumerable.CountsBetween`
