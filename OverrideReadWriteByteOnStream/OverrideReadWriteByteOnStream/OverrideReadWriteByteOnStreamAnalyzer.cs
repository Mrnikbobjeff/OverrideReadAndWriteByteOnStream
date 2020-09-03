using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace OverrideReadWriteByteOnStream
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class OverrideReadWriteByteOnStreamAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "OverrideReadWriteByteOnStream";
        const string ReadByte = "ReadByte";
        const string WriteByte = "WriteByte";
        private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.AnalyzerTitle), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Resources.AnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.AnalyzerDescription), Resources.ResourceManager, typeof(Resources));
        private const string Category = "Naming";

        private static DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSymbolAction(AnalyzeSymbol, SymbolKind.NamedType);
        }

        static bool IsActualDescendantOf(INamedTypeSymbol typeSymbol)
        {
            var container = typeSymbol;
            do
            {
                if (container.Name == "Stream" && container.ContainingNamespace.Name == "IO" && container.ContainingNamespace.ContainingNamespace.Name == "System")
                    return true;
                container = container.BaseType;
            } while (container != null) ;
            return false;
        }

        private static void AnalyzeSymbol(SymbolAnalysisContext context)
        {
            var namedTypeSymbol = (INamedTypeSymbol)context.Symbol;
            if (!IsActualDescendantOf(namedTypeSymbol))
                return;
            var declaredMethodsNotInStream = namedTypeSymbol
                .GetMembers()
                .OfType<IMethodSymbol>()
                .Where(t => !t.ContainingType.Name.Equals("Stream"));
            var readByteCallPresent = declaredMethodsNotInStream
                .Where(t => t.Name.Equals(ReadByte)).Any();
            var writeByteCallPresent = declaredMethodsNotInStream
                .Where(t => t.Name.Equals(WriteByte)).Any();
            Diagnostic diagnostic = null;
            if (!readByteCallPresent)
                diagnostic = Diagnostic.Create(Rule, namedTypeSymbol.Locations[0], ReadByte);
            else if (!writeByteCallPresent)
                diagnostic = Diagnostic.Create(Rule, namedTypeSymbol.Locations[0], WriteByte);
            if (diagnostic != null)
                context.ReportDiagnostic(diagnostic);
        }
    }
}
