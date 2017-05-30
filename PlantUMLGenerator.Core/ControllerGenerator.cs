using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace PlantUMLGenerator.Core
{
    public class ControllerGenerator: CSharpSyntaxWalker
    {
        private readonly TextWriter _output;
        private readonly string _rootNameSpace;
        private string _moduleName;
        private readonly List<string> _packages;
        private readonly string _stylePath;
        private readonly DateTime _now;

        public ControllerGenerator(TextWriter output, string stylePath, string rootNameSpace = "")
        {
            _output = output;
            _rootNameSpace = rootNameSpace;
            _packages = new List<string>();
            _stylePath = stylePath;
            _now = DateTime.Now;
        }

        private void LoadStyle()
        {
            var styles = File.ReadAllText(_stylePath);
            _output.Write(styles);
        }

        public void Generate(string[] codes)
        {
            _output.WriteLine("@startuml");
            LoadStyle();
            _output.WriteLine("scale 3508*4961");
            _output.WriteLine("left to right direction");
            foreach (var code in codes)
            {
                var tree = CSharpSyntaxTree.ParseText(code);
                var node = tree.GetRoot();
                GenerateInternal(node);
            }
            _output.WriteLine("}");
            _output.WriteLine("@enduml");
        }

        private void GenerateInternal(SyntaxNode node)
        {
            Visit(node);
        }

        public override void VisitNamespaceDeclaration(NamespaceDeclarationSyntax node)
        {
            var ns = node.Name.ToString();
            var index = ns.IndexOf(_rootNameSpace, StringComparison.Ordinal) + 2;
            _moduleName = ns.Substring(index + _rootNameSpace.Length);
            if (!_packages.Contains(_moduleName))
            {
                if (_packages.Count > 0) 
                {
                    _output.WriteLine("}");
                }
                _output.WriteLine("center header __" + _moduleName + "__\\n" + _now.Day + "/" + _now.Month + "/" + _now.Year);
                _output.WriteLine($"package {_moduleName} {{");
                _packages.Add(_moduleName);
            }
            base.VisitNamespaceDeclaration(node);
        }

        public override void VisitClassDeclaration(ClassDeclarationSyntax node)
        {
            VisitTypeDeclaration(node, () => base.VisitClassDeclaration(node));
        }

        private void VisitTypeDeclaration(TypeDeclarationSyntax node, Action visitBase)
        {
            var modifiers = GetTypeModifiersText(node.Modifiers);
            var keyword = (node.Modifiers.Any(SyntaxKind.AbstractKeyword) ? "abstract" : "")
                          + node.Keyword;
            var typeName = TypeNameText.From(node);
            var name = typeName.Identifier;
            var typeParam = typeName.TypeArguments;
            _output.WriteLine($"{keyword} {name}{typeParam} {modifiers}{{");
            visitBase();
            _output.WriteLine("}");
        }

        public override void VisitMethodDeclaration(MethodDeclarationSyntax node)
        {
            var modifiers = GetMemberModifiersText(node.Modifiers);
            var name = node.Identifier.ToString();
            var returnType = node.ReturnType.ToString();
            var args = node.ParameterList.Parameters.Select(p => $"{p.Identifier}:{p.Type}");
            _output.WriteLine($"{modifiers}{name}({string.Join(", ", args)}) : {returnType}");
        }

        private string GetTypeModifiersText(SyntaxTokenList modifiers)
        {
            var tokens = modifiers.Select(token =>
            {
                switch (token.Kind())
                {
                    case SyntaxKind.PublicKeyword:
                    case SyntaxKind.PrivateKeyword:
                    case SyntaxKind.ProtectedKeyword:
                    case SyntaxKind.InternalKeyword:
                    case SyntaxKind.AbstractKeyword:
                        return "";
                    default:
                        return $"<<{token.ValueText}>>";
                }
            }).Where(token => token != "");
            var result = string.Join(" ", tokens);
            if (result != string.Empty)
            {
                result += " ";
            };
            return result;
        }

        private string GetMemberModifiersText(SyntaxTokenList modifiers)
        {
            var tokens = modifiers.Select(token =>
            {
                switch (token.Kind())
                {
                    case SyntaxKind.PublicKeyword:
                        return "+";
                    case SyntaxKind.PrivateKeyword:
                        return "-";
                    case SyntaxKind.ProtectedKeyword:
                        return "#";
                    case SyntaxKind.AbstractKeyword:
                    case SyntaxKind.StaticKeyword:
                        return $"{{{token.ValueText}}}";
                    case SyntaxKind.InternalKeyword:
                    default:
                        return $"<<{token.ValueText}>>";
                }
            });
            var result = string.Join(" ", tokens);
            if (result != string.Empty)
            {
                result += " ";
            };
            return result;
        }
    }
}