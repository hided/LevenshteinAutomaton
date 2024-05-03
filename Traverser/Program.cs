using System;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using static System.Runtime.InteropServices.JavaScript.JSType;

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
                    currentNode = CloseChildNodes(currentNode);
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
                currentNode = CloseChildNodes(currentNode);
            }

            var closedRoot = new ImmutableNode
            {
                KeyChar = char.MinValue,
                ChildStartPos = rootNode.ChildStartIndex,
                ChildCount = (byte)rootNode.Children.Count,
                IsEnd = false,
                ParentPos = -1
            };
            _closedNodes.Add(closedRoot);

            // update parents
            var array = _closedNodes.ToArray();
            for (int i = 0; i < array.Length; i++)
            {
                var node = array[i];

                for (int j = 0; j < node.ChildCount; j++)
                {
                    array[node.ChildStartPos + j].ParentPos = i;
                }
            }

            var collection = new NodeCollection(array);
            return collection;
        }

        private OpenNode CloseChildNodes(OpenNode currentNode)
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
        static ImmutableNode[] GetSampleData()
        {
            return new[]
            {
                new ImmutableNode
                {
                    ChildCount = 5,
                    ChildStartPos = 1,
                    IsEnd = true,
                    ParentPos = 123,
                },
                new ImmutableNode
                {
                    ChildCount = 55,
                    ChildStartPos = 11,
                    IsEnd = false,
                    ParentPos = 1234,
                },
                new ImmutableNode
                {
                    ChildCount = 6,
                    ChildStartPos = 7,
                    IsEnd = true,
                    ParentPos = 456,
                },
            };
        }

        static void WriteToDisk(ImmutableNode[] nodes)
        {
            using var fs = new FileStream(@"segment.dat", FileMode.Create);


            var byteSpan = MemoryMarshal.Cast<ImmutableNode, byte>(nodes);
            fs.Write(byteSpan);

        }

        static ImmutableNode[] ReadFromDisk()
        {
            var fi = new FileInfo("segment.dat");
            var sz = Marshal.SizeOf<ImmutableNode>();

            var array = new ImmutableNode[fi.Length / sz];
            var byteSpan = MemoryMarshal.Cast<ImmutableNode, byte>(array);

            MemoryMarshal.CreateSpan(ref array[0], 3);

            using var fs = fi.OpenRead();
            fs.Read(byteSpan);

            return array;
        }

        static void Main(string[] args)
        {

            //Span<string> span;

            var data = GetSampleData();
            WriteToDisk(data);
            var read = ReadFromDisk();

            //var arrayPointer = Marshal.UnsafeAddrOfPinnedArrayElement(data, 0);

            //var byteArray = new byte[data.Length * 12];
            //var byteArrayPointer = Marshal.UnsafeAddrOfPinnedArrayElement(byteArray, 0);
            //Buffer.MemoryCopy(arrayPointer.ToPointer(), byteArrayPointer.ToPointer(), byteArray.Length, byteArray.Length);

            //var nodes = new ImmutableNode[data.Length];
            //var nodesPointer = Marshal.UnsafeAddrOfPinnedArrayElement(nodes, 0);
            //Buffer.MemoryCopy(byteArrayPointer.ToPointer(), nodesPointer.ToPointer(), byteArray.Length, byteArray.Length);


            Console.WriteLine("Hello, World!");


            var col = new ImmutableNode[100_000];

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

    public class NodeCollection(ImmutableNode[] nodes)
    {
        public ImmutableNode[] Nodes = nodes;
        public ImmutableNode RootNode = nodes.Last();

        public ImmutableNode GetParentOf(ImmutableNode node)
        {
            return Nodes[node.ParentPos];
        }

        public Span<ImmutableNode> GetChildrenOf(ImmutableNode node)
        {
            return Nodes.AsSpan().Slice(node.ChildStartPos, node.ChildCount);
        }

        public void WriteToDisk()
        {
            using var fs = new FileStream(@"segment.dat", FileMode.Create);
            var byteSpan = MemoryMarshal.Cast<ImmutableNode, byte>(Nodes);
            fs.Write(byteSpan);

        }

        public static NodeCollection FromDisk()
        {
            var fi = new FileInfo("segment.dat");
            var sz = Marshal.SizeOf<ImmutableNode>();

            var array = new ImmutableNode[fi.Length / sz];
            var byteSpan = MemoryMarshal.Cast<ImmutableNode, byte>(array);

            using var fs = fi.OpenRead();
            fs.Read(byteSpan);

            return new(array);
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

    [StructLayout(LayoutKind.Explicit, CharSet = CharSet.Auto)]
    public struct ImmutableNode
    {
        [FieldOffset(0)]
        public int ChildStartPos;

        [FieldOffset(4)]
        public char KeyChar; // TODO: byte

        [FieldOffset(6)]
        public byte ChildCount;

        [FieldOffset(7)]
        public bool IsEnd;

        [FieldOffset(8)]
        public int ParentPos;

        public override string ToString()
        {
            return $"{KeyChar}";
        }
    }

    
}
