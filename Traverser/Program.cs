namespace Traverser
{
    public class NodeGenerator
    {
        private readonly List<ImmutableNode> _closedNodes = [];

        public NodeCollection Generate(IEnumerable<string> words)
        {
            _closedNodes.Clear();
            var rootNode = new OpenNode()
            {
                FullName = "",
            };
            var currentNode = rootNode;

            foreach (var word in words)
            {
                // close the nodes that are not matching
                while (!word.StartsWith(currentNode.FullName))
                {
                    currentNode = CloseNode(currentNode);
                }

                // we are matching with the current node
                for (int i = currentNode.FullName.Length + 1; i <= word.Length; i++)
                {
                    var node = new OpenNode
                    {
                        FullName = word.Substring(0, i),
                        Parent = currentNode,
                        IsEnd = i == word.Length,
                    };
                    currentNode.Children.Add(node);
                    currentNode = node;
                }
            }

            // finishing up
            while (currentNode != null)
            {
                currentNode = CloseNode(currentNode);
            }

            var closedRoot = new ImmutableNode
            {
                KeyChar = char.MinValue,
                ChildStartPos = rootNode.ChildStartIndex,
                ChildCount = (byte)rootNode.Children.Count,
                IsEnd = false
            };
            _closedNodes.Add(closedRoot);

            var collection = new NodeCollection(_closedNodes);
            return collection;
        }

        private OpenNode CloseNode(OpenNode currentNode)
        {
            currentNode.ChildStartIndex = _closedNodes.Count;
            foreach (var childNode in currentNode.Children)
            {
                var closedNode = new ImmutableNode
                {
                    KeyChar = childNode.Key,
                    ChildStartPos = childNode.ChildStartIndex,
                    ChildCount = (byte)childNode.Children.Count,
                    IsEnd = childNode.IsEnd,
                };
                _closedNodes.Add(closedNode);

                childNode.Children.Clear();
            }

            return currentNode.Parent;

        }
    }

    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello, World!");

            // sorted words
            var words = new string[]
            {
                "aaa",
                "aaaz",
                "aaf",
                "aba",
                "abbe",
                "abbf",
                "abc",
                "acd",
                "acz",
                "bx",
                "by"
            };

            var generator = new NodeGenerator();
            var result = generator.Generate(words);
            

            Console.WriteLine("");
        }

        

        
    }

    public class NodeCollection(IEnumerable<ImmutableNode> nodes)
    {
        public ImmutableNode[] Nodes = nodes.ToArray();
        public ImmutableNode RootNode = nodes.Last();

        public Span<ImmutableNode> GetChildrenOf(ImmutableNode node)
        {
            return Nodes.AsSpan().Slice(node.ChildStartPos, node.ChildCount);
        }
    }

    internal class OpenNode
    {
        public string FullName;
        public char Key => FullName.LastOrDefault();

        public OpenNode Parent;
        public List<OpenNode> Children = new();
        public int ChildStartIndex;
        public bool IsEnd;

        public override string ToString()
        {
            return $"{FullName} ({Children.Count} children)";
        }
    }

    public struct ImmutableNode
    {
        public int ChildStartPos;
        public byte ChildCount;
        public char KeyChar; // TODO: byte
        public bool IsEnd;

        public override string ToString()
        {
            return $"{KeyChar}";
        }
    }
}
