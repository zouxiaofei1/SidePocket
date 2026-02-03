using SidePocket;
using System;
using System.Windows.Media;

public class PocketItem
{
    public IntPtr WindowHandle { get; set; }
    public string Title { get; set; } = string.Empty; // 给默认值
    public ImageSource? Icon { get; set; }           // 加问号
    public SideBallWindow? BallWindow { get; set; }  // 加问号
}
