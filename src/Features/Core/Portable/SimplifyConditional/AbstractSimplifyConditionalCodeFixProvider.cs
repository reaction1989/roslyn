﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#nullable enable

using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.LanguageServices;
using Microsoft.CodeAnalysis.Shared.Extensions;

namespace Microsoft.CodeAnalysis.SimplifyConditional
{
    internal abstract class AbstractSimplifyConditionalCodeFixProvider :
        SyntaxEditorBasedCodeFixProvider
    {
        public const string Negate = nameof(Negate);

        public sealed override ImmutableArray<string> FixableDiagnosticIds { get; } =
            ImmutableArray.Create(IDEDiagnosticIds.SimplifyConditionalExpressionDiagnosticId);

        internal sealed override CodeFixCategory CodeFixCategory
            => CodeFixCategory.CodeQuality;

        public sealed override Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            context.RegisterCodeFix(new MyCodeAction(
                c => FixAsync(context.Document, context.Diagnostics.First(), c)),
                context.Diagnostics);

            return Task.CompletedTask;
        }

        protected sealed override async Task FixAllAsync(
            Document document, ImmutableArray<Diagnostic> diagnostics,
            SyntaxEditor editor, CancellationToken cancellationToken)
        {
            var syntaxFacts = document.GetRequiredLanguageService<ISyntaxFactsService>();
            var generator = SyntaxGenerator.GetGenerator(document);
            var semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);

            foreach (var diagnostic in diagnostics)
            {
                var expr = diagnostic.Location.FindNode(getInnermostNodeForTie: true, cancellationToken);
                syntaxFacts.GetPartsOfConditionalExpression(expr, out var condition, out _, out _);

                var replacement = condition;
                if (diagnostic.Properties.ContainsKey(Negate))
                    replacement = generator.Negate(replacement, semanticModel, cancellationToken);

                editor.ReplaceNode(
                    expr, generator.AddParentheses(replacement.WithTriviaFrom(expr)));
            }
        }

        private class MyCodeAction : CodeAction.DocumentChangeAction
        {
            public MyCodeAction(Func<CancellationToken, Task<Document>> createChangedDocument)
                : base(FeaturesResources.Simplify_conditional_expression, createChangedDocument, FeaturesResources.Simplify_conditional_expression)
            {
            }
        }
    }
}
