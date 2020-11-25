﻿using System;

 namespace Chotiskazal.Bot
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
    }
}