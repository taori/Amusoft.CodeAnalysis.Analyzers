// Copyright 2020 Andreas Müller
// This file is a part of Amusoft.CodeAnalysis.Analyzers and is licensed under Apache 2.0
// See https://github.com/taori/Amusoft.CodeAnalysis.Analyzers/blob/master/LICENSE for details

// ReSharper disable InconsistentNaming
namespace Amusoft.CodeAnalysis.Analyzers.Shared
{
	public static class DiagnosticIds
	{
		public partial class ACA0001
		{
			public const string FixByForwardingToCollectionChildren = "ACA0001";
		}

		public partial class ACA0002
		{
			public const string DiagnosticOnClass = "ACA0002";
			public const string DiagnosticOnMethod = "ACA0003";
			public const string DiagnosticOnArray = "ACA0004";
			public const string DiagnosticOnNamespace = "ACA0005";
		}

		public partial class ACA0006
		{
			public const string DiagnosticOnStaticClass = "ACA0006";
		}
	}
}