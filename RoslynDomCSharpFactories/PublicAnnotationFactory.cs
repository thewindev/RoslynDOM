﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RoslynDom.Common;

namespace RoslynDom.CSharp
{
    public class PublicAnnotationFactory
        : RDomMiscFactory<IPublicAnnotation, SyntaxNode>
    {
        private static WhitespaceKindLookup _whitespaceLookup;

        public PublicAnnotationFactory(RDomCorporation corporation)
            : base(corporation)
        { }
        
        private WhitespaceKindLookup WhitespaceLookup
        {
            get
            {
                if (_whitespaceLookup == null)
                {
                    _whitespaceLookup = new WhitespaceKindLookup();
                    _whitespaceLookup.AddRange(WhitespaceKindLookup.Eol);
                }
                return _whitespaceLookup;
            }
        }

        public override RDomPriority Priority
        { get { return 0; } }

        public override bool CanCreateFrom(SyntaxNode syntaxNode)
        {
            // Always tries
            return true;
        }

        protected override IEnumerable<IMisc> CreateListFrom(SyntaxNode syntaxNode, IDom parent, SemanticModel model)
        {
            IEnumerable<IMisc> list;
            var syntaxRoot = syntaxNode as CompilationUnitSyntax;
            if (syntaxRoot != null)
            {
                // TODO: Review whether this is appropriate or necessary
                list = GetPublicAnnotations(syntaxRoot);
            }
            else
            { list = GetPublicAnnotations(syntaxNode); }
            return list;
        }

        public override IEnumerable<SyntaxNode> BuildSyntax(IDom item)
        {
            return null;
        }


        #region Private methods to support public annotations
        private IEnumerable<PublicAnnotation> GetPublicAnnotations(CompilationUnitSyntax syntaxRoot)
        {
            var ret = new List<PublicAnnotation>();
            var nodes = syntaxRoot.ChildNodes();
            foreach (var node in nodes)
            {
                ret.AddRange(GetPublicAnnotationFromFirstToken(node, true));
            }
            return ret;
        }

        private IEnumerable<PublicAnnotation> GetPublicAnnotations(SyntaxNode node)
        {
            return GetPublicAnnotationFromFirstToken(node, false);
        }

        private IEnumerable<PublicAnnotation> GetPublicAnnotationFromFirstToken(
                   SyntaxNode node, bool isRoot)
        {
            var ret = new List<PublicAnnotation>();
            var firstToken = node.GetFirstToken();
            if (firstToken != default(SyntaxToken))
            {
                ret.AddRange(GetPublicAnnotationFromToken(firstToken, isRoot));
            }
            return ret;
        }

        private IEnumerable<PublicAnnotation> GetPublicAnnotationFromToken(
               SyntaxToken token, bool isRoot)
        {
            var ret = new List<PublicAnnotation>();
            var trivias = token.LeadingTrivia
                              .Where(x => x.CSharpKind() == SyntaxKind.SingleLineCommentTrivia);
            foreach (var trivia in trivias)
            {
                var str = GetPublicAnnotationAsString(trivia);
                var strRoot = GetSpecialRootAnnotation(str);
                if (isRoot)
                { str = strRoot; }
                else
                { str = string.IsNullOrWhiteSpace(strRoot) ? str : ""; }
                if (!string.IsNullOrWhiteSpace(str))
                {
                    var attribSyntax = GetAnnotationStringAsAttribute(str);
                    // Reuse the evaluation work done in attribute to follow same rules
                    var tempAttribute = Corporation.CreateFrom<IAttribute>(attribSyntax, null, null).FirstOrDefault();
                    var newPublicAnnotation = new PublicAnnotation(tempAttribute.Name.ToString());
                    newPublicAnnotation.Whitespace2Set.AddRange(tempAttribute.Whitespace2Set);
                    foreach (var attributeValue in tempAttribute.AttributeValues)
                    {
                        newPublicAnnotation.AddItem(attributeValue.Name ?? "", attributeValue.Value);
                    }
                    ret.Add(newPublicAnnotation);
                }
            }
            return ret;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree.ParseText(System.String,System.String,Microsoft.CodeAnalysis.CSharp.CSharpParseOptions,System.Threading.CancellationToken)")]
        private static AttributeSyntax GetAnnotationStringAsAttribute(string str)
        {
            // Trick Roslyn into thinking it's an attribute
            str = "[" + str + "] public class {}";
            var tree = CSharpSyntaxTree.ParseText(str);
            var attrib = tree.GetRoot().DescendantNodes()
                        .Where(x => x.CSharpKind() == SyntaxKind.Attribute)
                        .FirstOrDefault();
            return attrib as AttributeSyntax;
        }

        private static string GetPublicAnnotationAsString(SyntaxTrivia trivia)
        {
            var str = trivia.ToString().Trim();
            if (!str.StartsWith("//", StringComparison.Ordinal)) throw new InvalidOperationException("Unexpected comment format");
            str = str.SubstringAfter("//").SubstringAfter("[[").SubstringBefore("]]").Trim();
            return str;
        }

        private static string GetSpecialRootAnnotation(string str)
        {
            str = str.Trim();

            if (str.StartsWith("file:", StringComparison.Ordinal))
            { return str.SubstringAfter("file:"); }
            if (str.StartsWith("root:", StringComparison.Ordinal))
            { return str.SubstringAfter("root:"); }
            return null;
        }


        #endregion
    }
}
