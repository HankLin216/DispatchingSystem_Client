using System;
using CommunicateProvider;
using SampleService;
using SampleService.ExtensionMethod;

namespace DispatchingSystem_Client
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Program Start");

            // 模擬的sample數量
            uint sampleNum = getTheSampleNum();

            // 準備模擬的sample
            Console.WriteLine($"初始化{sampleNum}隻Sample");
            Factory sampleFactory = new();
            Repository.samplesList = sampleFactory.CreateSamples(sampleNum);

            // print current samples at first
            Repository.PrintSamples();

            // Communicat to server
            Repository.ServerCommnicator = new Communicator();

            // listen to user instruction
            Listen2UserCmd();

            //
            Console.WriteLine("Program End");
            Console.ReadLine();
        }

        static uint getTheSampleNum()
        {
            Console.WriteLine("請輸入愈模擬的sample數量(1~10):");
            while (true)
            {
                string _sampleNum = Console.ReadLine().Trim();
                bool parseSuccess = uint.TryParse(_sampleNum, out uint sampleNum);
                if (!parseSuccess)
                {
                    Console.WriteLine("不合法的數字，請重新輸入");
                }

                if (sampleNum > 10 || sampleNum < 1)
                {
                    Console.WriteLine($"輸入的數字為:{sampleNum}，請輸入介於1~10的數字");
                }
                else
                {
                    return sampleNum;
                }
            }
        }

        static void Listen2UserCmd()
        {
            while (true)
            {
                string cmd = Console.ReadLine().Trim();

                switch (cmd.ToUpper())
                {
                    case "P":
                        Repository.PrintSamples();
                        break;
                    case "Q":
                        Console.WriteLine("Program Stop");
                        return;
                    case "HR":
                    case "HIDE REFRESH":
                        Repository.ServerCommnicator.printStateOfRefreshSampleInfo2ServerTimer = false;
                        Console.WriteLine("Stop printing the sending message");
                        break;
                    case "SR":
                    case "SHOW REFRESH":
                        Repository.ServerCommnicator.printStateOfRefreshSampleInfo2ServerTimer = true;
                        Console.WriteLine("Start printing the sending message");
                        break;
                    default:
                        Console.WriteLine($"Error: {cmd} -> Unknown Command!");
                        break;
                }
            }
        }
    }
}
