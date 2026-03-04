using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Serilog;

namespace TaskManager
{
    class Program
    {
        private static List<string> tasks = new List<string>();

        static void Main(string[] args)
        {
            ClearLogsFolder();

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console()
                .WriteTo.File("logs/taskmanager-.txt",
                    rollingInterval: RollingInterval.Day,
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
                .CreateLogger();

            try
            {
                Log.Information("Приложение TaskManager запущено");
                RunMainLoop();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Критическая ошибка в приложении");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        
        static void ClearLogsFolder()
        {
            string logsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");
            if (!Directory.Exists(logsPath))
            {
                Directory.CreateDirectory(logsPath);
                return;
            }

            try
            {
                var logFiles = Directory.GetFiles(logsPath, "*.txt");
                if (logFiles.Any())
                {
                    foreach (var file in logFiles)
                    {
                        File.Delete(file);
                    }
                }
            }
            catch (Exception ex)
            {
                
                Console.WriteLine($"Ошибка при очистке папки с логами: {ex.Message}");
            }
        }

        static void RunMainLoop()
        {
            while (true)
            {
                Console.WriteLine("\n--- Менеджер задач ---");
                Console.WriteLine("1. Добавить задачу");
                Console.WriteLine("2. Удалить задачу");
                Console.WriteLine("3. Показать все задачи");
                Console.WriteLine("4. Выход");
                Console.Write("Выберите действие: ");
                string input = Console.ReadLine();

                Log.Debug("Пользователь выбрал пункт меню: {Choice}", input);

                switch (input)
                {
                    case "1":
                        AddTask();
                        break;
                    case "2":
                        RemoveTask();
                        break;
                    case "3":
                        ListTasks();
                        break;
                    case "4":
                        Log.Information("Выход из приложения по команде пользователя");
                        return;
                    default:
                        Log.Warning("Неизвестная команда: {Choice}", input);
                        Console.WriteLine("Неверный ввод, попробуйте снова.");
                        break;
                }
            }
        }

        static void AddTask()
        {
            Console.Write("Введите описание задачи: ");
            string task = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(task))
            {
                Log.Warning("Попытка добавить пустую задачу");
                Console.WriteLine("Описание не может быть пустым.");
                return;
            }

            Log.Information("Добавление задачи: {Task}", task);
            tasks.Add(task);
            Log.Debug("Задача успешно добавлена. Всего задач: {Count}", tasks.Count);
            Console.WriteLine("Задача добавлена.");
        }

        static void RemoveTask()
        {
            if (tasks.Count == 0)
            {
                Log.Warning("Попытка удаления из пустого списка");
                Console.WriteLine("Список задач пуст.");
                return;
            }

            Console.Write("Введите номер задачи для удаления (1..{0}): ", tasks.Count);
            string input = Console.ReadLine();

            if (!int.TryParse(input, out int index) || index < 1 || index > tasks.Count)
            {
                Log.Error("Некорректный ввод индекса: {Input}", input);
                Console.WriteLine("Неверный номер задачи.");
                return;
            }

            try
            {
                string removedTask = tasks[index - 1];
                Log.Information("Удаление задачи: {Task} (индекс {Index})", removedTask, index);
                tasks.RemoveAt(index - 1);
                Log.Debug("Задача удалена. Осталось задач: {Count}", tasks.Count);
                Console.WriteLine("Задача удалена.");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Ошибка при удалении задачи с индексом {Index}", index);
                Console.WriteLine("Произошла ошибка при удалении.");
            }
        }

        static void ListTasks()
        {
            Log.Debug("Запрос на отображение списка задач");

            if (tasks.Count == 0)
            {
                Log.Warning("Список задач пуст");
                Console.WriteLine("Список задач пуст.");
            }
            else
            {
                Console.WriteLine("\nСписок задач:");
                for (int i = 0; i < tasks.Count; i++)
                {
                    Console.WriteLine($"{i + 1}. {tasks[i]}");
                }
                Log.Information("Отображено задач: {Count}", tasks.Count);
            }
        }
    }
}