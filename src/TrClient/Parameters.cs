using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrClient
{
    public class Parameters
    {
        public Parameters(string[] args) {
            args = [ "127.0.0.1:7777", "100", "0", "Melvin" ];


            if (args.Length < 4) {
                Console.WriteLine("请输入目标地址（形如: vrax.cc:7777）:");
                string? address = Console.ReadLine();
                while (!TryParseAddress(address, out IP!, out Port)) {
                    Console.WriteLine("输入无效，请重新输入目标地址（形如: vrax.cc:7777）:");
                    address = Console.ReadLine();
                }

                Console.WriteLine("请输入连接数（形如: 30）:");
                while (!int.TryParse(Console.ReadLine(), out NumInGroup)) {
                    Console.WriteLine("输入无效，请重新输入连接数（形如: 30）:");
                }

                Console.WriteLine("请输入连接组号（形如: 0）:");
                while (!byte.TryParse(Console.ReadLine(), out GroupIndex)) {
                    Console.WriteLine("输入无效，请重新输入连接组号（形如: 0）:");
                }

                Console.WriteLine("请输入模拟目标（玩家名）：");
                while (string.IsNullOrEmpty(SimulateTarget = Console.ReadLine()!)) {
                    Console.WriteLine("输入无效，请重新输入模拟目标（玩家名）:");
                }

                // 如果用户输入成功则生成一个批处理.bat文件，以便下次利用已有参数直接运行
                string batContent = $"@echo off\nTrClient.exe {IP}:{Port} {NumInGroup} {GroupIndex} {SimulateTarget}";
                File.WriteAllText("run.bat", batContent);
            }
            else {
                if (!TryParseAddress(args[0], out IP!, out Port)) {
                    throw new ArgumentException("目标地址格式无效");
                }

                if (!int.TryParse(args[1], out NumInGroup)) {
                    throw new ArgumentException("连接数格式无效");
                }

                if (!byte.TryParse(args[2], out GroupIndex)) {
                    throw new ArgumentException("连接组号格式无效");
                }

                if ((SimulateTarget = args[3]) == null) {
                    throw new ArgumentException("模拟目标不能为空");
                }
            }
        }

        private static bool TryParseAddress(string? address, [NotNullWhen(true)] out string? ip, out ushort port) {
            ip = null;
            port = 0;
            var parts = address?.Split(':') ?? [];
            if (parts.Length != 2 || !ushort.TryParse(parts[1], out port)) {
                return false;
            }
            ip = parts[0];
            return true;
        }

        public readonly string IP;
        public readonly ushort Port;
        public readonly int NumInGroup;
        public readonly byte GroupIndex;
        public readonly string SimulateTarget;
    }
}
