using System.Collections;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;

namespace Amusoft.CodeAnalysis.Analyzers.Extensions
{
	public static class TypeSymbolExtensions
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool TryGetEnumerableType(this ITypeSymbol source, SemanticModel semanticModel, out ITypeSymbol enumerableTypeOfT)
		{
			var genericEnumerableType = semanticModel.Compilation.GetTypeByMetadataName("System.Collections.Generic.IEnumerable`1");
			var match = source.AllInterfaces.FirstOrDefault(d => d.ConstructedFrom.Equals(genericEnumerableType));
			if (match is INamedTypeSymbol namedTypeSymbol)
			{
				enumerableTypeOfT = namedTypeSymbol.TypeArguments[0];
				return true;
			}

			enumerableTypeOfT = null;
			return false;
		}
	}
}