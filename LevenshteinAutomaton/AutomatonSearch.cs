using NoAlloq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Traverser;

namespace LevenshteinAutomaton
{
    public class AutomatonSearch
    {
        private static void DFSserach(LenvstnDFA dfa, int start, TrieNode dictNode, List<string> output)
        {
            if (dfa.final.Contains(start) && dictNode.End)
                output.Add(dictNode.Key);

            HashSet<char> inputs = new HashSet<char>();
            //for (char ch = 'a'; ch <= 'z'; ++ch)
            //{
            //    KeyValuePair<int, char> pair = new KeyValuePair<int, char>(start, ch);
            //    if (dfa.transTable.ContainsKey(pair))
            //    {
            //        inputs.Add(ch);
            //        if (dictNode.Children.ContainsKey(ch))
            //        {
            //            DFSserach(dfa, dfa.transTable[pair], dictNode.Children[ch], output);
            //        }
            //    }
            //}

            if (dfa.defaultTrans.ContainsKey(start))
            {
                foreach (char input in dictNode.Children.Keys)
                {
                    if (!inputs.Contains(input))
                    {
                        DFSserach(dfa, dfa.defaultTrans[start], dictNode.Children[input], output);
                    }
                }
            }
        }

        public static IEnumerable<string> search(string oriWord, int maxDist, TrieDictionary dict)
        {
            LenvstnNFA nfa = LenvstnNFA.BuildNFA(oriWord, maxDist);
            //nfa.Show();
            LenvstnDFA dfa = SubsetMachine.SubsetConstruct(nfa);
            //dfa.Show();
            List<string> output = new List<string>();
            DFSserach(dfa, dfa.start, dict.Root, output);
            return output;
        }
    }

    public class AutomatonSearch2
    {
        public static NodeCollection nodes;

        private static void DFSserach(LenvstnDFA dfa, int start, ImmutableNode node, List<string> output)
        {
            if (dfa.final.Contains(start) && node.IsEnd)
            {
                List<byte> result = new();
                var currNode = node;
                while (currNode.ParentPos != UInt24.MaxValue)
                {
                    result.Add(currNode.KeyChar);
                    currNode = nodes.GetParentOf(currNode);
                }
                output.Add(new string(result.Select(b => (char)b).Reverse().ToArray()));
            }

            var children = nodes.GetChildrenOf(node);
            HashSet<byte> inputs = new HashSet<byte>();

            if (dfa.transTable.TryGetValue(start, out var transitions))
            {
                foreach (var kvp in transitions)
                {
                    byte c = kvp.Key;
                    inputs.Add(c);
                    var match = children.FirstOrDefault(x => x.KeyChar == c);
                    if (match.Equals(default))
                        continue;

                    DFSserach(dfa, kvp.Value, match, output);
                }
            }
            

            if (dfa.defaultTrans.ContainsKey(start))
            {
                foreach (var child in children)
                {
                    if (!inputs.Contains(child.KeyChar))
                    {
                        DFSserach(dfa, dfa.defaultTrans[start], child, output);
                    }
                }
            }
        }

        public static IEnumerable<string> search(string oriWord, int maxDist, NodeCollection collection)
        {
            LenvstnNFA nfa = LenvstnNFA.BuildNFA(oriWord, maxDist);
            //nfa.Show();
            LenvstnDFA dfa = SubsetMachine.SubsetConstruct(nfa);
            //dfa.Show();
            List<string> output = new List<string>();
            nodes = collection;
            DFSserach(dfa, dfa.start, collection.RootNode, output);
            return output;
        }
    }
}
