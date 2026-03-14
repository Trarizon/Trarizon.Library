using System.Runtime.CompilerServices;
using Windows.Win32;
using Windows.Win32.UI.Input.KeyboardAndMouse;

namespace Trarizon.Library.Windows.Device;

public static class MouseEvents
{
    // PInvoke: User32.dll
    public static void MoveTo(int x, int y, bool absolutePosition = false)
        => PInvoke.mouse_event(MOUSE_EVENT_FLAGS.MOUSEEVENTF_MOVE.WithAbsolute(absolutePosition), x, y, 0, 0);

    public static void Click(Buttons buttons, int x = 0, int y = 0, bool absolutePosition = false)
        => PInvoke.mouse_event(buttons.WithAction(Action.Click).WithAbsolute(absolutePosition), x, y, 0, 0);

    public static void Click(XButtons xButtons, int x = 0, int y = 0, bool absolutePosition = false)
    {
        PInvoke.mouse_event(
            MOUSE_EVENT_FLAGS.MOUSEEVENTF_XDOWN.WithAbsolute(absolutePosition),
            x, y, Unsafe.As<XButtons, int>(ref xButtons), 0);
    }

    public static void Down(Buttons buttons, int x = 0, int y = 0, bool absolutePosition = false)
        => PInvoke.mouse_event(buttons.WithAction(Action.Down).WithAbsolute(absolutePosition), x, y, 0, 0);

    public static void Up(Buttons buttons, int x = 0, int y = 0, bool absolutePosition = false)
        => PInvoke.mouse_event(buttons.WithAction(Action.Up).WithAbsolute(absolutePosition), x, y, 0, 0);

    public static void ClickLeftMouse()
        => PInvoke.mouse_event(MOUSE_EVENT_FLAGS.MOUSEEVENTF_LEFTDOWN | MOUSE_EVENT_FLAGS.MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);

    public static void ScrollWheel(int amount)
        => PInvoke.mouse_event(MOUSE_EVENT_FLAGS.MOUSEEVENTF_WHEEL, 0, 0, amount, 0);

    public static void ScrollWheelByDelta(int multiple)
        => ScrollWheel(multiple * 120);

    private static MOUSE_EVENT_FLAGS WithAbsolute(this MOUSE_EVENT_FLAGS ev, bool absolutePosition)
        => absolutePosition ? ev | MOUSE_EVENT_FLAGS.MOUSEEVENTF_ABSOLUTE : ev;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static MOUSE_EVENT_FLAGS WithAction(this Buttons buttons, Action actions)
        => (MOUSE_EVENT_FLAGS)(Unsafe.As<Buttons, uint>(ref buttons) * Unsafe.As<Action, uint>(ref actions));

    public enum Buttons : uint
    {
        Left = MOUSE_EVENT_FLAGS.MOUSEEVENTF_LEFTDOWN,
        Right = MOUSE_EVENT_FLAGS.MOUSEEVENTF_RIGHTDOWN,
        Wheel = MOUSE_EVENT_FLAGS.MOUSEEVENTF_WHEEL,
    }

    public enum XButtons { X1 = 1, X2 = 2, }

    private enum Action
    {
        Down = 1,
        Up = 2,
        Click = Down | Up,
    }
}
