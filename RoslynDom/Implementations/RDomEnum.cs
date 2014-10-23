using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using RoslynDom.Common;
using System.ComponentModel.DataAnnotations;
namespace RoslynDom
{
   [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming",
      "CA1711:IdentifiersShouldNotHaveIncorrectSuffix",
      Justification = "Because this represents an enum, it's an appropriate name")]
   public class RDomEnum : RDomBase<IEnum, ISymbol>, IEnum
   {
      private AttributeCollection _attributes = new AttributeCollection();
      private RDomCollection<IEnumMember> _members;

      public RDomEnum(string name, string underlyingTypeName = null, AccessModifier accessModifier = AccessModifier.Private)
          : this(name, new RDomReferencedType(underlyingTypeName), accessModifier)
      { }

      public RDomEnum(string name, IReferencedType underlyingType , AccessModifier accessModifier = AccessModifier.Private)
          : this(null, null, null)
      {
         _name = name;
         _accessModifier = accessModifier;
         _underlyingType = underlyingType;
      }

      public RDomEnum(SyntaxNode rawItem, IDom parent, SemanticModel model)
          : base(rawItem, parent, model)
      { Initialize(); }

      internal RDomEnum(RDomEnum oldRDom)
          : base(oldRDom)
      {
         Attributes.AddOrMoveAttributeRange(oldRDom.Attributes.Select(x => x.Copy()));
         _members = oldRDom.Members.Copy(this);
         _name = oldRDom.Name;
         _accessModifier = oldRDom.AccessModifier;
         _declaredAccessModifier = oldRDom.DeclaredAccessModifier;
         _underlyingType = oldRDom.UnderlyingType;
      }

      private void Initialize()
      {
         _members = new RDomCollection<IEnumMember>(this);
      }

      public override IEnumerable<IDom> Children
      {
         get
         {
            var list = base.Children.ToList();
            list.AddRange(_members);
            return list;
         }
      }

      public AttributeCollection Attributes
      { get { return _attributes; } }

      private string _name;
      [Required]
      public string Name
      {
         get { return _name; }
         set { SetProperty(ref _name, value); }
      }

      private AccessModifier _accessModifier;
      public AccessModifier AccessModifier
      {
         get { return _accessModifier; }
         set { SetProperty(ref _accessModifier, value); }
      }

      private AccessModifier _declaredAccessModifier;
      public AccessModifier DeclaredAccessModifier
      {
         get { return _declaredAccessModifier; }
         set { SetProperty(ref _declaredAccessModifier, value); }
      }

      private IReferencedType _underlyingType;
      public IReferencedType UnderlyingType
      {
         get { return _underlyingType; }
         set { SetProperty(ref _underlyingType, value); }
      }

      private IStructuredDocumentation _structuredDocumentation;
      public IStructuredDocumentation StructuredDocumentation
      {
         get { return _structuredDocumentation; }
         set { SetProperty(ref _structuredDocumentation, value); }
      }

      private string _description;
      public string Description
      {
         get { return _description; }
         set { SetProperty(ref _description, value); }
      }

      public string OuterName
      { get { return RoslynUtilities.GetOuterName(this); } }

      public string QualifiedName
      { get { return RoslynUtilities.GetQualifiedName(this); } }

      public string Namespace
      { get { return RoslynDomUtilities.GetNamespace(this.Parent); } }

      public bool IsNested
      { get { return (Parent is IType); } }

      public IType ContainingType
      { get { return Parent as IType; } }

      public RDomCollection<IEnumMember> Members
      { get { return _members; } }

      public MemberKind MemberKind
      { get { return MemberKind.Enum; } }

      public StemMemberKind StemMemberKind
      { get { return StemMemberKind.Enum; } }
   }
}
