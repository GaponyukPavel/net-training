using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Reflection.Tasks
{
    public class CodeGeneration
    {
        /// <summary>
        /// Returns the functions that returns vectors' scalar product:
        /// (a1, a2,...,aN) * (b1, b2, ..., bN) = a1*b1 + a2*b2 + ... + aN*bN
        /// Generally, CLR does not allow to implement such a method via generics to have one function for various number types:
        /// int, long, float. double.
        /// But it is possible to generate the method in the run time! 
        /// See the idea of code generation using Expression Tree at: 
        /// http://blogs.msdn.com/b/csharpfaq/archive/2009/09/14/generating-dynamic-methods-with-expression-trees-in-visual-studio-2010.aspx
        /// </summary>
        /// <typeparam name="T">number type (int, long, float etc)</typeparam>
        /// <returns>
        ///   The function that return scalar product of two vectors
        ///   The generated dynamic method should be equal to static MultuplyVectors (see below).   
        /// </returns>
        public static Func<T[], T[], T> GetVectorMultiplyFunction<T>() where T : struct
        {
            ParameterExpression parameterOne = Expression.Parameter(typeof(T));
            ParameterExpression parameterTwo = Expression.Parameter(typeof(T));
            BinaryExpression sum = Expression.Add(parameterOne, parameterTwo);
            BinaryExpression mult = Expression.Multiply(parameterOne, parameterTwo);
            Expression<Func<T, T, T>> sumExpresion = Expression.Lambda<Func<T, T, T>>(sum, parameterOne, parameterTwo);
            Expression<Func<T, T, T>> multExpresion = Expression.Lambda<Func<T, T, T>>(mult, parameterOne, parameterTwo);
            Func<T, T, T> multFunc = multExpresion.Compile();
            Func<T, T, T> sumFunc = sumExpresion.Compile();

            return (arrA, arrB) =>
            {
                int length = arrA.Length < arrB.Length ? arrA.Length : arrB.Length;
                if (length == 0)
                    return new T();
                T result = new T();
                for (int i = 0; i < length; i++)
                    result = sumFunc(result, multFunc(arrA[i], arrB[i]));
                return result;
            };

        }
        static Func<int, int> ETFact()

        {
            ParameterExpression value = Expression.Parameter(typeof(int), "value");
            ParameterExpression result = Expression.Parameter(typeof(int), "result");
            LabelTarget label = Expression.Label(typeof(int));
            BlockExpression block = Expression.Block(new[] { result },Expression.Assign(result, Expression.Constant(1)),
                Expression.Loop(Expression.IfThenElse(Expression.GreaterThan(value, Expression.Constant(1)),
                Expression.MultiplyAssign(result,Expression.PostDecrementAssign(value)),Expression.Break(label, result)),label));
           
            return Expression.Lambda<Func<int, int>>(block, value).Compile();
        }


        // Static solution to check performance benchmarks
        public static int MultuplyVectors(int[] first, int[] second)
        {
            int result = 0;
            for (int i = 0; i < first.Length; i++)
            {
                result += first[i] * second[i];
            }
            return result;
        }

    }
}
