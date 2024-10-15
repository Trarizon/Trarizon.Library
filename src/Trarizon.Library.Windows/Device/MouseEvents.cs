using System.Runtime.CompilerServices;
using Vanara.PInvoke;

namespace Trarizon.Library.Windows.Device;
public static class MouseEvents
{
    // PInvoke: User32.dll
    public static void MoveTo(int x, int y, bool absolutePosition = false)
        => User32.mouse_event(User32.MOUSEEVENTF.MOUSEEVENTF_MOVE.WithAbsolute(absolutePosition), x, y, 0, 0);

    public static void Click(Buttons buttons, int x = 0, int y = 0, bool absolutePosition = false)
        => User32.mouse_event(buttons.WithAction(Action.Click).WithAbsolute(absolutePosition), x, y, 0, 0);

    public static void Click(XButtons xButtons, int x = 0, int y = 0, bool absolutePosition = false)
    {
        User32.mouse_event(
            User32.MOUSEEVENTF.MOUSEEVENTF_XDOWN.WithAbsolute(absolutePosition),
            x, y, Unsafe.As<XButtons, int>(ref xButtons), 0);
    }

    public static void Down(Buttons buttons, int x = 0, int y = 0, bool absolutePosition = false)
        => User32.mouse_event(buttons.WithAction(Action.Down).WithAbsolute(absolutePosition), x, y, 0, 0);

    public static void Up(Buttons buttons, int x = 0, int y = 0, bool absolutePosition = false)
        => User32.mouse_event(buttons.WithAction(Action.Up).WithAbsolute(absolutePosition), x, y, 0, 0);

    public static void ClickLeftMouse()
        => User32.mouse_event(User32.MOUSEEVENTF.MOUSEEVENTF_LEFTDOWN | User32.MOUSEEVENTF.MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);

    public static void ScrollWheel(int amount)
        => User32.mouse_event(User32.MOUSEEVENTF.MOUSEEVENTF_WHEEL, 0, 0, amount, 0);

    public static void ScrollWheelByDelta(int multiple)
        => ScrollWheel(multiple * 120);

    private static User32.MOUSEEVENTF WithAbsolute(this User32.MOUSEEVENTF ev, bool absolutePosition)
        => absolutePosition ? ev | User32.MOUSEEVENTF.MOUSEEVENTF_ABSOLUTE : ev;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static User32.MOUSEEVENTF WithAction(this Buttons buttons, Action actions)
        => (User32.MOUSEEVENTF)(Unsafe.As<Buttons, int>(ref buttons) * Unsafe.As<Action, int>(ref actions));

    public enum Buttons
    {
        Left = User32.MOUSEEVENTF.MOUSEEVENTF_LEFTDOWN,
        Right = User32.MOUSEEVENTF.MOUSEEVENTF_RIGHTDOWN,
        Wheel = User32.MOUSEEVENTF.MOUSEEVENTF_WHEEL,
    }

    public enum XButtons { X1 = 1, X2 = 2, }

    private enum Action
    {
        Down = 1,
        Up = 2,
        Click = Down | Up,
    }
}
