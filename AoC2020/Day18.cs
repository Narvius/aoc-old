using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AoC2020
{
    public class Day18 : ISolution
    {
        // Evaluate all expressions without precedence, and sum the results.
        public string PartOne(string[] lines)
            => lines.Select(BadMathParser.Run).Sum().ToString();

        // Evaluate all expressions with addition having precedence over multiplication, and sum the results.
        public string PartTwo(string[] lines)
            => lines.Select(BadMathParser.RunWithPrecedence).Sum().ToString();
    }

    public static class BadMathParser
    {
        // Evaluates an infix expression without precedence rules.
        public static long Run(string expression)
            => RunRpn(ToRpn(expression, 1, 1));

        // Evaluates an infix rexpression with addition having precedence over multiplication.
        public static long RunWithPrecedence(string expression)
            => RunRpn(ToRpn(expression, 2, 1));

        /// <summary>
        /// Converts an infix expression into postfix expression using the shunting-yard algorith (https://en.wikipedia.org/wiki/Shunting-yard_algorithm).
        /// Supports addition and multiplication.
        /// </summary>
        /// <remarks>The shunting-yard algorithm used here is slightly simplified--all operators are left-associative.</remarks>
        /// <param name="expression">The infix expression.</param>
        /// <param name="additionPrecedence">Precedence of the addition operator.</param>
        /// <param name="multiplicationPrecedence">Precedence of the multiplication operator.</param>
        /// <returns>Tokenized RPN expression equivalent to the original infix expression.</returns>
        private static string[] ToRpn(string expression, int additionPrecedence, int multiplicationPrecedence)
        {
            var tokens = expression.Replace("(", "( ").Replace(")", " )").Split(' ').ToArray();

            int n = 0;
            var output = new string[tokens.Count(c => c != "(" && c != ")")];
            var operators = new Stack<string>();

            void Output(string token) => output[n++] = token;
            int Precendence(string op) => op == "+" ? additionPrecedence : multiplicationPrecedence;

            foreach (var token in tokens)
            {
                if (token.All(char.IsDigit))
                    Output(token);
                else if (token == "+" || token == "*")
                {
                    while (operators.Count > 0 && operators.Peek() != "(" && Precendence(operators.Peek()) >= Precendence(token))
                        Output(operators.Pop());
                    operators.Push(token);
                }
                else if (token == "(")
                    operators.Push(token);
                else if (token == ")")
                {
                    while (operators.Peek() != "(")
                        Output(operators.Pop());
                    operators.Pop();
                }
            }

            while (operators.Count > 0)
                Output(operators.Pop());

            return output;
        }

        /// <summary>
        /// Runs a reverse polish notation expression with addition and multiplication.
        /// </summary>
        /// <param name="tokens">Tokens in the expression.</param>
        /// <returns>The result of the expression.</returns>
        private static long RunRpn(string[] tokens)
        {
            var stack = new Stack<long>();
            foreach (var token in tokens)
                if (int.TryParse(token, out int number))
                    stack.Push(number);
                else if (token == "+")
                    stack.Push(stack.Pop() + stack.Pop());
                else if (token == "*")
                    stack.Push(stack.Pop() * stack.Pop());

            return stack.Pop();
        }
    }
}
