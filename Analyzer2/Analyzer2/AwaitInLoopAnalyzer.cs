

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
namespace Analyzer2
{

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class AwaitInLoopAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "AwaitInLoop";
        private static readonly LocalizableString Title = "Avoid 'await' inside loops";
        private static readonly LocalizableString MessageFormat = "'await' call inside {0} loop";
        private static readonly LocalizableString Description = "Asynchronous method calls should not be awaited inside loops.";
        private const string Category = "Performance";

        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.ForStatement, SyntaxKind.ForEachStatement, SyntaxKind.WhileStatement, SyntaxKind.DoStatement);
        }

        private static void AnalyzeNode(SyntaxNodeAnalysisContext context)
        {
            var loopStatement = (StatementSyntax)context.Node;

            foreach (var descendantNode in loopStatement.DescendantNodes())
            {
                if (descendantNode is AwaitExpressionSyntax awaitExpression)
                {
                    var diagnostic = Diagnostic.Create(Rule, awaitExpression.GetLocation(), loopStatement.Kind().ToString());
                    context.ReportDiagnostic(diagnostic);
                }
            }
        }
    }
}
