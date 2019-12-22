using System.Collections;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;

namespace Amusoft.CodeAnalysis.Analyzers.Extensions
{
	public static class TypeSymbolExtensions
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsEnumerable(this ITypeSymbol source)
		{
			return source.AllInterfaces.Any(d => d.SpecialType == SpecialType.System_Collections_IEnumerable);
		}

		public static ITypeSymbol GetGenericTypeArgument(this ITypeSymbol source, int arity)
		{
			return null;
//			source.
		}
	}
}