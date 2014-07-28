﻿using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RoslynDom.Common;

namespace RoslynDom.CSharp
{
    public class RDomMethodTypeMemberFactory
          : RDomTypeMemberFactory<RDomMethod, MethodDeclarationSyntax>
    {
        public RDomMethodTypeMemberFactory(RDomCorporation corporation)
         : base(corporation)
        { }

        protected override ITypeMemberCommentWhite CreateItemFrom(SyntaxNode syntaxNode, IDom parent, SemanticModel model)
        {
            var syntax = syntaxNode as MethodDeclarationSyntax;
            var newItem = new RDomMethod(syntaxNode, parent, model);
            CreateFromWorker.StandardInitialize(newItem, syntaxNode, parent, model);
            CreateFromWorker.InitializeStatements(newItem, syntax.Body, newItem, model);
            //if (syntax.Body != null)
            //{
            //    var statements = ListUtilities.MakeList(syntax, x => x.Body.Statements, x => Corporation.CreateFrom<IStatementCommentWhite>(x, newItem, model));
            //    newItem.StatementsAll.AddOrMoveRange(statements);
            //}
            newItem.Name = newItem.TypedSymbol.Name;

            var typeParameters = newItem.TypedSymbol.TypeParametersFrom();
            newItem.TypeParameters.AddOrMoveRange(typeParameters);

            newItem.AccessModifier = RoslynUtilities.GetAccessibilityFromSymbol(newItem.Symbol);
            newItem.ReturnType = new RDomReferencedType(newItem.TypedSymbol.DeclaringSyntaxReferences, newItem.TypedSymbol.ReturnType);
            newItem.IsAbstract = newItem.Symbol.IsAbstract;
            newItem.IsVirtual = newItem.Symbol.IsVirtual;
            newItem.IsOverride = newItem.Symbol.IsOverride;
            newItem.IsSealed = newItem.Symbol.IsSealed;
            newItem.IsStatic = newItem.Symbol.IsStatic;
            newItem.IsExtensionMethod = newItem.TypedSymbol.IsExtensionMethod;
            var parameters = ListUtilities.MakeList(syntax, x => x.ParameterList.Parameters, x => Corporation.CreateFrom<IMisc>(x, newItem, model))
                                .OfType<IParameter>();
            newItem.Parameters.AddOrMoveRange(parameters);


            return newItem;
        }

        public override IEnumerable<SyntaxNode> BuildSyntax(IDom item)
        {
            var itemAsMethod = item as IMethod;
            var nameSyntax = SyntaxFactory.Identifier(itemAsMethod.Name);

            var returnTypeSyntax = (TypeSyntax)RDomCSharp.Factory.BuildSyntaxGroup(itemAsMethod.ReturnType).First();
            var modifiers = BuildSyntaxHelpers.BuildModfierSyntax(itemAsMethod);
            var node = SyntaxFactory.MethodDeclaration(returnTypeSyntax, nameSyntax)
                            .WithModifiers(modifiers);

            var attributes = BuildSyntaxWorker.BuildAttributeSyntax(itemAsMethod.Attributes);
            if (attributes.Any()) { node = node.WithAttributeLists(attributes.WrapInAttributeList()); }

            var parameterSyntaxList = itemAsMethod.Parameters
                        .SelectMany(x => RDomCSharp.Factory.BuildSyntaxGroup(x))
                        .OfType<ParameterSyntax>()
                        .ToList();
            if (parameterSyntaxList.Any()) { node = node.WithParameterList(SyntaxFactory.ParameterList(SyntaxFactory.SeparatedList(parameterSyntaxList))); }

            node = node.WithLeadingTrivia(BuildSyntaxHelpers.LeadingTrivia(item));

            node = node.WithBody(RoslynCSharpUtilities.MakeStatementBlock(itemAsMethod.Statements));

            // TODO: typeParameters  and constraintClauses 

            return item.PrepareForBuildSyntaxOutput(node);
        }

    }


}
