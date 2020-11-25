﻿using System;

 namespace SayWhat.Bll
{
    public static class Botlog{
        public static void Error(long? chatId, string msg)
        {
            var now = DateTime.Now;
            Console.WriteLine($"[{now.Hour}:{now.Minute}:{now.Second}.{now.Millisecond}] [{chatId}] {msg}");
        }
        public static void Write(string msg)
        {
            var now = DateTime.Now;
            Console.WriteLine($"[{now.Hour}:{now.Minute}:{now.Second}.{now.Millisecond}] {msg}");
        }

        public static void Metric(long? userTelegramId, string metricId, string param,
            TimeSpan swElapsed)
        {
            Console.WriteLine($"[{userTelegramId}] [{metricId}] [{param}] {swElapsed}]");
        }
    }
}