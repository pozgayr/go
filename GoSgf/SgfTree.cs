using GoEngine;
using System.Text;
using System.Linq;

namespace GoSgf;

public class SgfNode
{
    public Dictionary<string, List<string>> Props { get; } = new();
}

public class SgfTree
{
    public List<SgfNode> Sequence { get; } = new();
    public List<SgfTree> Children { get; } = new();
}

public static class Sgf
{
    public static SgfTree Parse(string text)
    {
        var p = new Parser(text);
        return p.ParseTree();
    }

    private class Parser
    {
        private readonly string s;
        private int i;

        public Parser(string s)
        {
            this.s = s;
            i = 0;
        }

        public SgfTree ParseTree()
        {
            SkipWs();
            Expect('(');

            var tree = new SgfTree();

            // sequence of nodes
            while (Peek() == ';')
            {
                tree.Sequence.Add(ParseNode());
                SkipWs();
            }

            // variations
            while (Peek() == '(')
            {
                tree.Children.Add(ParseTree());
                SkipWs();
            }

            Expect(')');
            return tree;
        }

        private SgfNode ParseNode()
        {
            Expect(';');
            SkipWs();

            var node = new SgfNode();

            while (true)
            {
                SkipWs();

                if (!IsUpper(Peek()))
                    break;

                string key = ParseIdent();

                SkipWs();

                if (Peek() != '[')
                    throw Error("Expected '[' after property");

                while (Peek() == '[')
                {
                    string val = ParseValue();
                    Add(node, key, val);
                    SkipWs();
                }
            }

            return node;
        }

        private static void Add(SgfNode node, string key, string val)
        {
            if (!node.Props.TryGetValue(key, out var list))
            {
                list = new List<string>();
                node.Props[key] = list;
            }
            list.Add(val);
        }

        private string ParseIdent()
        {
            int start = i;
            while (!End() && IsUpper(s[i])) i++;
            return s[start..i];
        }

        private string ParseValue()
        {
            Expect('[');

            var sb = new StringBuilder();

            while (true)
            {
                if (End()) throw Error("Unterminated value");

                char c = s[i++];

                if (c == ']') break;

                if (c == '\\')
                {
                    if (End()) break;

                    char next = s[i++];

                    // ignore line breaks after escape
                    if (next == '\n' || next == '\r')
                        continue;

                    sb.Append(next);
                }
                else
                {
                    sb.Append(c);
                }
            }

            return sb.ToString();
        }

        private void SkipWs()
        {
            while (!End() && char.IsWhiteSpace(s[i])) i++;
        }

        private char Peek() => End() ? '\0' : s[i];

        private void Expect(char c)
        {
            if (Peek() != c)
                throw Error($"Expected '{c}'");

            i++;
        }

        private bool End() => i >= s.Length;

        private static bool IsUpper(char c) => c >= 'A' && c <= 'Z';

        private Exception Error(string msg) => new Exception($"{msg} at {i}");
    }
}

