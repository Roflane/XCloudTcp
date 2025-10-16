static class Log {
    
    public static void Red(string msg, bool wait = false) {
        Console.Write($"\u001b[31m{msg}\u001b[0m");
        if (wait) Console.ReadLine();
    }
    
    public static void Green(string msg, bool wait = false) {
        Console.Write($"\u001b[32m{msg}\u001b[0m");
        if (wait) Console.ReadLine();
    }
    
    public static void Blue(string msg, bool wait = false) {
        Console.Write($"\u001b[34m{msg}\u001b[0m");
        if (wait) Console.ReadLine();
    }
    
    public static void Magenta(string msg, bool wait = false) {
        Console.Write($"\u001b[35m{msg}\u001b[0m");
        if (wait) Console.ReadLine();
    }
}