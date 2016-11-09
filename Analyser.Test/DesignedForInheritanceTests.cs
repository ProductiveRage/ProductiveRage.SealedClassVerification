using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestHelper;

namespace ProductiveRage.SealedClassVerification.Analyser.Test
{
	[TestClass]
	public class DesignedForInheritanceTests : DiagnosticVerifier
	{
		[TestMethod]
		public void NoGoodIfNoModifierAndNoAttribute()
		{
			var testContent = @"
				namespace TestCase
				{
					public class Example { }
				}";


			var expected = new DiagnosticResult
			{
				Id = DesignedForInheritanceAnalyser.DiagnosticId,
				Message = DesignedForInheritanceAnalyser.ClassesMustBeAbstractSealedStaticOrMarkedAsDesignedForInheritanceRule.MessageFormat.ToString(),
				Severity = DiagnosticSeverity.Warning,
				Locations = new[]
				{
					new DiagnosticResultLocation("Test0.cs", 4, 6)
				}
			};

			VerifyCSharpDiagnostic(testContent, expected);
		}

		[TestMethod]
		public void AcceptableToHaveNoModifierIfAttributeIsPresent()
		{
			var testContent = @"
				using ProductiveRage.SealedClassVerification;

				namespace TestCase
				{
					[DesignedForInheritance]
					public class Example { }
				}";

			VerifyCSharpDiagnostic(testContent);
		}

		[TestMethod]
		public void AcceptableToHaveNoModifierIfAttributeIsPresentWithAttributeSuffixInName()
		{
			var testContent = @"
				using ProductiveRage.SealedClassVerification;

				namespace TestCase
				{
					[DesignedForInheritanceAttribute]
					public class Example { }
				}";

			VerifyCSharpDiagnostic(testContent);
		}

		[TestMethod]
		public void AcceptableToHaveNoModifierIfAttributeIsPresentWhenAttributeNameIncludesFullNamespace()
		{
			var testContent = @"
				namespace TestCase
				{
					[ProductiveRage.SealedClassVerification.DesignedForInheritance]
					public class Example { }
				}";

			VerifyCSharpDiagnostic(testContent);
		}

		[TestMethod]
		public void AcceptableIfClassIsAbstract()
		{
			var testContent = @"
				namespace TestCase
				{
					public abstract class Example { }
				}";

			VerifyCSharpDiagnostic(testContent);
		}

		[TestMethod]
		public void AcceptableIfClassIsSealed()
		{
			var testContent = @"
				namespace TestCase
				{
					public sealed class Example { }
				}";

			VerifyCSharpDiagnostic(testContent);
		}

		[TestMethod]
		public void AcceptableIfClassIsStatic()
		{
			var testContent = @"
				namespace TestCase
				{static class Example { }
				}";

			VerifyCSharpDiagnostic(testContent);
		}

		[TestMethod]
		public void DoesNotApplyToStructs()
		{
			var testContent = @"
				namespace TestCase
				{
					public struct Example { }
				}";

			VerifyCSharpDiagnostic(testContent);
		}

		[TestMethod]
		public void DoNotUseAttributeWithAbstractClass()
		{
			var testContent = @"
				using ProductiveRage.SealedClassVerification;

				namespace TestCase
				{
					[DesignedForInheritance]
					public abstract class Example { }
				}";


			var expected = new DiagnosticResult
			{
				Id = DesignedForInheritanceAnalyser.DiagnosticId,
				Message = DesignedForInheritanceAnalyser.AttributeMustNotBeUsedOnAbstractSealedOrStaticClassRule.MessageFormat.ToString(),
				Severity = DiagnosticSeverity.Warning,
				Locations = new[]
				{
					new DiagnosticResultLocation("Test0.cs", 6, 6)
				}
			};

			VerifyCSharpDiagnostic(testContent, expected);
		}

		[TestMethod]
		public void DoNotUseAttributeWithSealedClass()
		{
			var testContent = @"
				using ProductiveRage.SealedClassVerification;

				namespace TestCase
				{
					[DesignedForInheritance]
					public sealed class Example { }
				}";


			var expected = new DiagnosticResult
			{
				Id = DesignedForInheritanceAnalyser.DiagnosticId,
				Message = DesignedForInheritanceAnalyser.AttributeMustNotBeUsedOnAbstractSealedOrStaticClassRule.MessageFormat.ToString(),
				Severity = DiagnosticSeverity.Warning,
				Locations = new[]
				{
					new DiagnosticResultLocation("Test0.cs", 6, 6)
				}
			};

			VerifyCSharpDiagnostic(testContent, expected);
		}

		[TestMethod]
		public void DoNotUseAttributeWithStaticClass()
		{
			var testContent = @"
				using ProductiveRage.SealedClassVerification;

				namespace TestCase
				{
					[DesignedForInheritance]
					public static class Example { }
				}";


			var expected = new DiagnosticResult
			{
				Id = DesignedForInheritanceAnalyser.DiagnosticId,
				Message = DesignedForInheritanceAnalyser.AttributeMustNotBeUsedOnAbstractSealedOrStaticClassRule.MessageFormat.ToString(),
				Severity = DiagnosticSeverity.Warning,
				Locations = new[]
				{
					new DiagnosticResultLocation("Test0.cs", 6, 6)
				}
			};

			VerifyCSharpDiagnostic(testContent, expected);
		}

		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
		{
			return new DesignedForInheritanceAnalyser();
		}
	}
}