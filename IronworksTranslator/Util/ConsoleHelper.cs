using System;
using System.Runtime.InteropServices;

namespace IronworksTranslator.Util
{
    public static class ConsoleHelper
    {
        // P/Invoke 선언
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool AllocConsole();

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool FreeConsole();

        [DllImport("kernel32.dll")]
        private static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        private const int SW_HIDE = 0;
        private const int SW_SHOW = 5;

        public static void ShowConsoleWindow()
        {
            var handle = GetConsoleWindow();
            
            if (handle == IntPtr.Zero)
            {
                AllocConsole();
                Console.WriteLine("아이언웍스 번역기 디버그 콘솔");
                Console.WriteLine("=======================");
                Console.WriteLine("이 창을 닫지 마세요! 프로그램이 함께 종료됩니다.");
                Console.WriteLine("로그는 이 창에 실시간으로 표시됩니다.");
                Console.WriteLine("=======================");
            }
            else
            {
                ShowWindow(handle, SW_SHOW);
            }
        }

        public static void HideConsoleWindow()
        {
            var handle = GetConsoleWindow();
            if (handle != IntPtr.Zero)
            {
                ShowWindow(handle, SW_HIDE);
            }
        }
    }
} 