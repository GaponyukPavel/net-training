using System;
using System.Collections.Generic;
using System.IO;

namespace Collections.Tasks
{

    /// <summary>
    ///  Tree node item 
    /// </summary>
    /// <typeparam name="T">the type of tree node data</typeparam>
    public interface ITreeNode<T>
    {
        T Data { get; set; }                             // Custom data
        IEnumerable<ITreeNode<T>> Children { get; set; } // List of childrens
    }


    public class Task
    {

        /// <summary> Generate the Fibonacci sequence f(x) = f(x-1)+f(x-2) </summary>
        /// <param name="count">the size of a required sequence</param>
        /// <returns>
        ///   Returns the Fibonacci sequence of required count
        /// </returns>
        /// <exception cref="System.InvalidArgumentException">count is less then 0</exception>
        /// <example>
        ///   0 => { }  
        ///   1 => { 1 }    
        ///   2 => { 1, 1 }
        ///   12 => { 1, 1, 2, 3, 5, 8, 13, 21, 34, 55, 89, 144 }
        /// </example>
        public static IEnumerable<int> GetFibonacciSequence(int count)
        {
            if (count < 0)
                throw new System.ArgumentException();
            if (count == 0)
                return new List<int>();
            if (count == 1)
                return new List<int>() { 1 };
            List<int> FibonacciList = new List<int>(count) { 1, 1 };
            int Previus = 1, Curent = 1, Next;
            for (int i = 2; i < count; i++)
            {
                Next = Curent + Previus;
                Previus = Curent;
                Curent = Next;
                FibonacciList.Add(Curent);
            }
            return FibonacciList;
            //return CountFibonacciSequence();
            //IEnumerable<int> CountFibonacciSequence()
            //{
            //    List<int> FibonacciList = new List<int>(count);
            //    for (int i = 1; i <= count; i++)
            //        FibonacciList.Add(GetByPos(i));
            //    return FibonacciList;
            //}
            //int GetByPos(int pos)
            //{
            //    if (pos == 1)
            //        return 1;
            //    if (pos < 1)
            //        return 0;
            //    return GetByPos(pos - 1) + GetByPos(pos - 2);
            //}
        }

        /// <summary>
        ///    Parses the input string sequence into words
        /// </summary>
        /// <param name="reader">input string sequence</param>
        /// <returns>
        ///   The enumerable of all words from input string sequence. 
        /// </returns>
        /// <exception cref="System.ArgumentNullException">reader is null</exception>
        /// <example>
        ///  "TextReader is the abstract base class of StreamReader and StringReader, which ..." => 
        ///   {"TextReader","is","the","abstract","base","class","of","StreamReader","and","StringReader","which",...}
        /// </example>
        public static IEnumerable<string> Tokenize(TextReader reader)
        {
            if (reader == null)
                throw new ArgumentNullException();
            char[] delimeters = new[] { ',', ' ', '.', '\t', '\n' };
            List<string> ListToReturn = new List<string>();
            string Str;
            using (reader)
            {
                while (true)
                {
                    Str = reader.ReadLine();
                    if (Str != null)
                        ListToReturn.AddRange(Str.Split(delimeters, StringSplitOptions.RemoveEmptyEntries));
                    else
                        break;
                }
            }
            return ListToReturn;
        }



        /// <summary>
        ///   Traverses a tree using the depth-first strategy
        /// </summary>
        /// <typeparam name="T">tree node type</typeparam>
        /// <param name="root">the tree root</param>
        /// <returns>
        ///   Returns the sequence of all tree node data in depth-first order
        /// </returns>
        /// <example>
        ///    source tree (root = 1):
        ///    
        ///                      1
        ///                    / | \
        ///                   2  6  7
        ///                  / \     \
        ///                 3   4     8
        ///                     |
        ///                     5   
        ///                   
        ///    result = { 1, 2, 3, 4, 5, 6, 7, 8 } 
        /// </example>
        public static IEnumerable<T> DepthTraversalTree<T>(ITreeNode<T> root)
        {
            if (root == null)
                throw new ArgumentNullException();
            List<T> ListToReturn = new List<T>();
            Stack<ITreeNode<T>> StackOfNods = new Stack<ITreeNode<T>>(),
                LocalStack = new Stack<ITreeNode<T>>();
            StackOfNods.Push(root);

            while (StackOfNods.Count > 0)
            {
                ListToReturn.Add(StackOfNods.Peek().Data);
                LocalStack.Clear();
                if (StackOfNods.Peek().Children == null)
                {
                    StackOfNods.Pop();
                    continue;
                }
                foreach (var nod in StackOfNods.Pop().Children)
                    LocalStack.Push(nod);
                foreach (var item in LocalStack)
                    StackOfNods.Push(item);
            }
            return ListToReturn;
        }

        /// <summary>
        ///   Traverses a tree using the width-first strategy
        /// </summary>
        /// <typeparam name="T">tree node type</typeparam>
        /// <param name="root">the tree root</param>
        /// <returns>
        ///   Returns the sequence of all tree node data in width-first order
        /// </returns>
        /// <example>
        ///    source tree (root = 1):
        ///    
        ///                      1
        ///                    / | \
        ///                   2  3  4
        ///                  / \     \
        ///                 5   6     7
        ///                     |
        ///                     8   
        ///                   
        ///    result = { 1, 2, 3, 4, 5, 6, 7, 8 } 
        /// </example>
        public static IEnumerable<T> WidthTraversalTree<T>(ITreeNode<T> root)
        {
            if (root == null)
                throw new ArgumentNullException();
            List<T> ListToReturn = new List<T>();
            LinkedList<ITreeNode<T>> Nodes = new LinkedList<ITreeNode<T>>();
            Nodes.AddLast(root);
            var CurentNode = Nodes.First;
            while (CurentNode != null)
            {
                ListToReturn.Add(CurentNode.Value.Data);
                if (CurentNode.Value.Children != null)
                    foreach (var item in CurentNode.Value.Children)
                        Nodes.AddLast(item);
                CurentNode = CurentNode.Next;
            }
            return ListToReturn;
        }



        /// <summary>
        ///   Generates all permutations of specified length from source array
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source">source array</param>
        /// <param name="count">permutation length</param>
        /// <returns>
        ///    All permuations of specified length
        /// </returns>
        /// <exception cref="System.InvalidArgumentException">count is less then 0 or greater then the source length</exception>
        /// <example>
        ///   source = { 1,2,3,4 }, count=1 => {{1},{2},{3},{4}}
        ///   source = { 1,2,3,4 }, count=2 => {{1,2},{1,3},{1,4},{2,3},{2,4},{3,4}}
        ///   source = { 1,2,3,4 }, count=3 => {{1,2,3},{1,2,4},{1,3,4},{2,3,4}}
        ///   source = { 1,2,3,4 }, count=4 => {{1,2,3,4}}
        ///   source = { 1,2,3,4 }, count=5 => ArgumentOutOfRangeException
        /// </example>
        public static IEnumerable<T[]> GenerateAllPermutations<T>(T[] source, int count)
        {
            if (count < 0 || source.Length < count)
                throw new ArgumentOutOfRangeException();
            if (count == 0)
                return new List<T[]>();

            List<T[]> Result = new List<T[]>();
            T[] CurentValue = new T[count];
            Loop(0, 0);
            void Loop(int Counter, int StartPos)
            {
                int Maxi = source.Length - count + Counter;
                for (int i = StartPos; i <= Maxi; i++)
                {
                    CurentValue[Counter] = source[i];
                    if (Counter == count - 1)
                    {
                        Result.Add((T[])CurentValue.Clone());
                        continue;
                    }
                    Loop(Counter + 1, i + 1);
                }
            }
            return Result;
        }

    }

    public static class DictionaryExtentions
    {

        /// <summary>
        ///    Gets a value from the dictionary cache or build new value
        /// </summary>
        /// <typeparam name="TKey">TKey</typeparam>
        /// <typeparam name="TValue">TValue</typeparam>
        /// <param name="dictionary">source dictionary</param>
        /// <param name="key">key</param>
        /// <param name="builder">builder function to build new value if key does not exist</param>
        /// <returns>
        ///   Returns a value assosiated with the specified key from the dictionary cache. 
        ///   If key does not exist than builds a new value using specifyed builder, puts the result into the cache 
        ///   and returns the result.
        /// </returns>
        /// <example>
        ///   IDictionary<int, Person> cache = new SortedDictionary<int, Person>();
        ///   Person value = cache.GetOrBuildValue(10, ()=>LoadPersonById(10) );  // should return a loaded Person and put it into the cache
        ///   Person cached = cache.GetOrBuildValue(10, ()=>LoadPersonById(10) );  // should get a Person from the cache
        /// </example>
        public static TValue GetOrBuildValue<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, Func<TValue> builder)
        {
            if (!dictionary.ContainsKey(key))
                dictionary.Add(key, builder());
            return dictionary[key];
        }

    }
}
