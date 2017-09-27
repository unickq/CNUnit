using System;
using System.Collections.Generic;
using System.Threading;
using CNUnit.Tools;
using ManyConsole;

namespace CNUnit
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Utils.BeginEndPrint();
            Console.ForegroundColor = ConsoleColor.Yellow;

            var commands = GetCommands();
            ConsoleCommandDispatcher.DispatchCommand(commands, args, Console.Out);

            Utils.PrintCopyright();
            Console.ResetColor();
        }

        private static IEnumerable<ConsoleCommand> GetCommands()
        {
            return ConsoleCommandDispatcher.FindCommandsInSameAssemblyAs(typeof(Program));
        }
    }
}