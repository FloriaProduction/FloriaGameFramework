using System;
using System.Diagnostics;
using DotGLFW;
using static DotGL.GL;

namespace FloriaGF
{
    /// <summary>
    /// Логи
    /// </summary>
    static class Log
    {
        static List<string> _logs = new ();
        static bool _save_logs = true;
        static bool _enable_logs = true;
        public static bool with_error { get; set; } = false;
        
        /// <summary>
        /// Вывести лог
        /// </summary>
        /// <param name="message">Сообщение</param>
        public static void write(string message, string? from = null)
        {
            string now = DateTime.Now.ToLongTimeString();

            if (_save_logs)
                _logs.Add($"[{now}] {message}");
            if (_enable_logs)
                Console.WriteLine($"[{now}]{(from != null ? $" [ {from.ToUpper()} ]" : "")}: {message}");
        }

        /// <summary>
        /// Вывести StackTrace Ошибки
        /// </summary>
        public static void write(Exception e)
        {
            string message = "ERROR!";
            message += $"\n    Message: {e.Message}";

            var trace = new StackTrace(e, true);

            foreach (var frame in trace.GetFrames())
            {
                message += $"\n    File: {Path.GetFileName(frame.GetFileName())}; Line: {frame.GetFileLineNumber()}; Method: {frame.GetMethod()}";
            }

            write(message);
        }

        /// <summary>
        /// Сохранить логи в файлы
        /// </summary>
        public static void save()
        {
            if (!_save_logs) return;

            DateTime now = DateTime.Now;
            FileGF.writeFile($"logs/{now.ToShortDateString().Replace('.', '-')} {now.ToLongTimeString().Replace(':', '_')}{(with_error ? " error" : "")}.log", string.Join('\n', _logs));
        }
    }
}
