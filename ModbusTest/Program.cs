using EasyModbus;
using Microsoft.Extensions.Configuration;
using System.Runtime.CompilerServices;

namespace ModbusTest
{
    internal class Program
    {
        private static readonly ModbusClient _client;

        static Program()
        {
            IConfigurationRoot configuration = new ConfigurationBuilder()
            .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

            string clientIp = configuration["ClientSettings:IP"] ?? throw new Exception("Client IP not configured.");
            int clientPort = Convert.ToInt32(configuration["ClientSettings:Port"] ?? throw new Exception("Client port not configured."));
            int baudRate = Convert.ToInt32(configuration["ClientSettings:BaudRate"] ?? throw new Exception("Client baud rate not configured."));

            _client = new(clientIp, clientPort)
            {
                Baudrate = baudRate
            };
        }

        static void Main(string[] args)
        {
            Console.WriteLine("Application started.");
            _client.Connect();
            Console.WriteLine($"Connected to Modbus server {_client.IPAddress}(port: {_client.Port})");

            PrintFunctionsList();

            int commandRead;
            do
            {
                try
                {
                    Console.WriteLine("\nWhich operation do you want to execute? (100 to print functions list)");

                    Console.Write("Type operation code: ");
                    string? commandChosenStr = Console.ReadLine();

                    Console.WriteLine("");

                    if (int.TryParse(commandChosenStr, out commandRead))
                    {
                        switch (commandRead)
                        {
                            case -1: Console.WriteLine("Invalid selection."); break;
                            case 0: break;
                            case 1: ReadCoils(); break;
                            case 2: ReadDiscreteInputs(); break;
                            case 3: ReadHoldingRegisters(); break;
                            case 4: ReadInputRegisters(); break;
                            case 5: WriteSingleCoil(); break;
                            case 6: WriteSingleRegister(); break;
                            case 15: WriteMultipleCoils(); break;
                            case 16: WriteMultipleRegisters(); break;
                            case 23: ReadWriteRegisters(); break;
                            case 100: PrintFunctionsList(); break;
                            default: Console.WriteLine("Unsupported function."); break;
                        }
                    }
                    else commandRead = -1;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    commandRead = -1;
                }
            } while (commandRead != 0);
        }

        public static void ReadCoils()
        {
            Console.Write("Start coil address: ");
            int startCoilAddr = Convert.ToInt32(Console.ReadLine());
            Console.Write("Number of coils: ");
            int coilsNumber = Convert.ToInt32(Console.ReadLine());

            Console.WriteLine($"\nReading coils...");
            bool[] coils = _client.ReadCoils(startCoilAddr, coilsNumber);
            for (int i = 0; i < coils.Length; i++)
            {
                Console.WriteLine($"Coil {startCoilAddr + i + 1}: {coils[i]}");
            }
        }

        public static void ReadDiscreteInputs()
        {
            Console.Write("Start discrete input address: ");
            int startInputAddr = Convert.ToInt32(Console.ReadLine());
            Console.Write("Number of discrete inputs: ");
            int inputsNumber = Convert.ToInt32(Console.ReadLine());

            Console.WriteLine("\nReading discrete inputs...");
            bool[] discreteInputs = _client.ReadDiscreteInputs(startInputAddr, inputsNumber);
            for (int i = 0; i < discreteInputs.Length; i++)
            {
                Console.WriteLine($"Discrete input {startInputAddr + i + 1}: {discreteInputs[i]}");
            }
        }

        public static void ReadHoldingRegisters()
        {
            Console.Write("Start register address: ");
            int startRegisterAddr = Convert.ToInt32(Console.ReadLine());
            Console.Write("Number of registers: ");
            int registersNumber = Convert.ToInt32(Console.ReadLine());

            Console.WriteLine("\nReading holding registers...");
            int[] holdingRegisters = _client.ReadHoldingRegisters(startRegisterAddr, registersNumber);
            for (int i = 0; i < holdingRegisters.Length; i++)
            {
                Console.WriteLine($"Holding register {startRegisterAddr + i + 1}: {holdingRegisters[i]}");
            }
        }

        public static void ReadInputRegisters()
        {
            Console.Write("Start register address: ");
            int startRegisterAddr = Convert.ToInt32(Console.ReadLine());
            Console.Write("Number of registers: ");
            int registersNumber = Convert.ToInt32(Console.ReadLine());

            Console.WriteLine("\nReading input registers...");
            int[] inputRegisters = _client.ReadInputRegisters(startRegisterAddr, registersNumber);
            for (int i = 0; i < inputRegisters.Length; i++)
            {
                Console.WriteLine($"Input register {startRegisterAddr + i + 1}: {inputRegisters[i]}");
            }
        }

        public static void WriteSingleCoil()
        {
            Console.Write("Coil to write address: ");
            int coilToWriteAddr = Convert.ToInt32(Console.ReadLine());

            Console.WriteLine($"\nWrite a value in coil {coilToWriteAddr + 1} (0x00{PadAddress(coilToWriteAddr)}): ");
            int readCoilValue = Convert.ToInt32(Console.ReadLine());

            Console.WriteLine("Writing coil...");
            _client.WriteSingleCoil(coilToWriteAddr, readCoilValue == 1);
            Console.WriteLine("Done!");
        }

        public static void WriteSingleRegister()
        {
            Console.Write("Register to write address: ");
            int registerToWriteAddr = Convert.ToInt32(Console.ReadLine());

            Console.WriteLine($"\nWrite a value in register {registerToWriteAddr + 1} (0x00{PadAddress(registerToWriteAddr)}): ");
            int readRegisterValue = Convert.ToInt32(Console.ReadLine());

            Console.WriteLine("Writing register...");
            _client.WriteSingleRegister(registerToWriteAddr, readRegisterValue);
            Console.WriteLine("Done!");
        }

        public static void WriteMultipleCoils()
        {
            Console.Write("Start coil to write address: ");
            int startCoilAddr = Convert.ToInt32(Console.ReadLine());
            Console.Write("Number of coils to write: ");
            int coilsToWrite = Convert.ToInt32(Console.ReadLine());
            int lastCoilAddr = startCoilAddr + coilsToWrite - 1;
            bool[] coilsToWriteValues = new bool[coilsToWrite];

            Console.WriteLine($"\nWrite {coilsToWrite} values in coils {startCoilAddr + 1}-{lastCoilAddr + 1} (0x00{PadAddress(startCoilAddr)}-0x00{PadAddress(lastCoilAddr)}): ");
            for (int i = 0; i < coilsToWrite; i++)
            {
                coilsToWriteValues[i] = Convert.ToInt32(Console.ReadLine()) == 1;
            }

            Console.WriteLine("Writing coils...");
            _client.WriteMultipleCoils(startCoilAddr, coilsToWriteValues);
            Console.WriteLine("Done!");
        }

        public static void WriteMultipleRegisters()
        {
            Console.Write("Start register to write address: ");
            int startRegisterAddr = Convert.ToInt32(Console.ReadLine());
            Console.Write("Number of registers to write: ");
            int registersToWrite = Convert.ToInt32(Console.ReadLine());
            int lastRegisterAddr = startRegisterAddr + registersToWrite - 1;
            int[] registersToWriteValues = new int[registersToWrite];

            Console.WriteLine($"\nWrite {registersToWrite} values in registers {startRegisterAddr + 1}-{lastRegisterAddr + 1} (0x00{PadAddress(startRegisterAddr)}-0x00{PadAddress(lastRegisterAddr)}): ");
            for (int i = 0; i < registersToWrite; i++)
            {
                registersToWriteValues[i] = Convert.ToInt32(Console.ReadLine());
            }

            Console.WriteLine("Writing holding registers...");
            _client.WriteMultipleRegisters(startRegisterAddr, registersToWriteValues);
            Console.WriteLine("Done!");
        }

        public static void ReadWriteRegisters()
        {
            Console.Write("Start register to read address: ");
            int startRegisterToReadAddr = Convert.ToInt32(Console.ReadLine());
            Console.Write("Number of registers: ");
            int numberOfRegistersToRead = Convert.ToInt32(Console.ReadLine());

            Console.Write("Start register to write address: ");
            int startRegisterToWriteAddr = Convert.ToInt32(Console.ReadLine());
            Console.Write("Number of registers to write: ");
            int registersToWrite = Convert.ToInt32(Console.ReadLine());
            int lastRegisterToWrite = startRegisterToWriteAddr + registersToWrite - 1;
            int[] registersToWriteValues = new int[registersToWrite];

            Console.WriteLine($"\nWrite {registersToWrite} values in registers {startRegisterToWriteAddr + 1}-{lastRegisterToWrite + 1} (0x00{PadAddress(startRegisterToWriteAddr)}-0x00{PadAddress(lastRegisterToWrite)}): ");
            for (int i = 0; i < registersToWrite; i++)
            {
                registersToWriteValues[i] = Convert.ToInt32(Console.ReadLine());
            }

            Console.WriteLine("Writing and reading holding registers...");
            int[] holdingRegisters = _client.ReadWriteMultipleRegisters(startRegisterToReadAddr, numberOfRegistersToRead, startRegisterToWriteAddr, registersToWriteValues);
            for (int i = 0; i < holdingRegisters.Length; i++)
            {
                Console.WriteLine($"Holding register {startRegisterToReadAddr + i + 1}: {holdingRegisters[i]}");
            }
            Console.WriteLine("Done!");
        }

        public static string PadAddress(int addressNumber)
        {
            return (addressNumber).ToString().PadLeft(2, '0');
        }

        public static void PrintFunctionsList()
        {
            Console.WriteLine("\nFUNCTIONS");
            Console.WriteLine("1 - Read coils");
            Console.WriteLine("2 - Read discrete inputs");
            Console.WriteLine("3 - Read holding registers");
            Console.WriteLine("4 - Read input registers");
            Console.WriteLine("5 - Write single coil");
            Console.WriteLine("6 - Write single register");
            Console.WriteLine("8 - Diagnostics (Serial Line only - not yet supported)");
            Console.WriteLine("11 - Get comm event counter (Serial Line only - not yet supported)");
            Console.WriteLine("15 - Write multiple coils");
            Console.WriteLine("16 - Write multiple registers");
            Console.WriteLine("17 - Report Server ID (Serial Line only - not yet supported)");
            Console.WriteLine("22 - Mask write register (not yet supported)");
            Console.WriteLine("23 - Read/write multiple registers");
            Console.WriteLine("43/14 - Read device identification (not yet supported)");
            Console.WriteLine("0 - Quit");
        }
    }
}
