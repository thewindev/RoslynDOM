﻿using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RoslynDom.Common;

namespace RoslynDom.CSharp
{
    public class RDomRootFactory
          : RDomRootContainerFactory<RDomRoot, CompilationUnitSyntax>
    {
        public RDomRootFactory(RDomCorporation corporation)
         : base(corporation)
        { }

        protected override IRoot CreateItemFrom(SyntaxNode syntaxNode, IDom parent,SemanticModel model)
        {
            var syntax = syntaxNode as CompilationUnitSyntax;
            var newItem = new RDomRoot(syntaxNode, parent,model);
            CreateFromWorker.StandardInitialize(newItem, syntaxNode, parent, model);

            newItem.Name = "Root";
            CreateFromWorker .LoadStemMembers(newItem, syntax.Members, syntax.Usings,model);

            return newItem ;
        }

        public override IEnumerable<SyntaxNode> BuildSyntax(IDom item)
        {
            var itemAsT = item as IRoot;
            var node = SyntaxFactory.CompilationUnit();
            var usingsSyntax = itemAsT.UsingDirectives
                        .SelectMany(x => RDomCSharp.Factory.BuildSyntaxGroup(x))
                        .ToList();
            var membersSyntax = itemAsT.StemMembers
                        .Where(x=>!(x is IUsingDirective))
                        .SelectMany(x => RDomCSharp.Factory.BuildSyntaxGroup(x))
                        .ToList();
            node = node.WithUsings(SyntaxFactory.List(usingsSyntax));
            node = node.WithMembers(SyntaxFactory.List(membersSyntax));
            return item.PrepareForBuildSyntaxOutput(node);
        }
    }


}
