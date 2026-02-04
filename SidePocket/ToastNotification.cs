using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace SidePocket
{
    public class ToastNotification
    {
        private static readonly List<Window> _activeToasts = new List<Window>();
        private const double TOAST_MARGIN = 10;
        private static readonly HashSet<Window> _hookedOwners = new HashSet<Window>();

        public static void Show(string message, Window owner)
        {
            var toast = new Window
            {
                WindowStyle = WindowStyle.None,
                AllowsTransparency = true,
                Background = System.Windows.Media.Brushes.Transparent,
                SizeToContent = SizeToContent.WidthAndHeight,
                ShowInTaskbar = false,
                Topmost = true,
                IsHitTestVisible = false,
                ShowActivated = false,
                Owner = owner,
                WindowStartupLocation = WindowStartupLocation.Manual
            };

            // 创建华丽的 UI
            var mainGrid = new Grid { Margin = new Thickness(20) };
            
            // 外层发光效果
            var glowBorder = new Border
            {
                Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(180, 20, 20, 20)),
                CornerRadius = new CornerRadius(15),
                Effect = new DropShadowEffect
                {
                    BlurRadius = 25,
                    ShadowDepth = 0,
                    Opacity = 0.6,
                    Color = System.Windows.Media.Colors.Black
                }
            };

            // 玻璃拟态层
            var glassBorder = new Border
            {
                CornerRadius = new CornerRadius(15),
                BorderThickness = new Thickness(1),
                BorderBrush = new LinearGradientBrush(
                    System.Windows.Media.Color.FromArgb(100, 255, 255, 255),
                    System.Windows.Media.Color.FromArgb(20, 255, 255, 255),
                    45),
                Padding = new Thickness(20, 12, 25, 12),
                Background = new LinearGradientBrush(
                    System.Windows.Media.Color.FromArgb(40, 255, 255, 255),
                    System.Windows.Media.Color.FromArgb(5, 255, 255, 255),
                    45)
            };

            var contentStack = new StackPanel { Orientation = System.Windows.Controls.Orientation.Horizontal };

            // 华丽的动画图形 (SVG Path 模拟)
            var iconGrid = new Grid { Width = 32, Height = 32, Margin = new Thickness(0, 0, 15, 0) };
            
            // 背景圆环
            var circle = new Ellipse
            {
                Stroke = new SolidColorBrush(System.Windows.Media.Color.FromRgb(0, 120, 212)),
                StrokeThickness = 2,
                Width = 30,
                Height = 30,
                Opacity = 0.5
            };
            
            // 打钩图形 (SVG)
            var checkPath = new Path
            {
                Data = Geometry.Parse("M7,15 L13,21 L25,9"),
                Stroke = System.Windows.Media.Brushes.White,
                StrokeThickness = 3,
                StrokeStartLineCap = PenLineCap.Round,
                StrokeEndLineCap = PenLineCap.Round,
                StrokeLineJoin = PenLineJoin.Round,
                Effect = new DropShadowEffect { BlurRadius = 8, Color = System.Windows.Media.Color.FromRgb(0, 120, 212), ShadowDepth = 0 },
                RenderTransformOrigin = new System.Windows.Point(0.5, 0.5),
                RenderTransform = new ScaleTransform(0, 0)
            };

            iconGrid.Children.Add(circle);
            iconGrid.Children.Add(checkPath);

            var textStack = new StackPanel { VerticalAlignment = VerticalAlignment.Center };
            
            var titleText = new TextBlock
            {
                Text = "SidePocket",
                Foreground = System.Windows.Media.Brushes.White,
                FontSize = 14,
                FontWeight = FontWeights.Bold,
                Opacity = 0.9
            };

            var msgText = new TextBlock
            {
                Text = message,
                Foreground = System.Windows.Media.Brushes.White,
                FontSize = 12,
                Opacity = 0.7,
                Margin = new Thickness(0, 2, 0, 0)
            };

            textStack.Children.Add(titleText);
            textStack.Children.Add(msgText);

            contentStack.Children.Add(iconGrid);
            contentStack.Children.Add(textStack);

            glassBorder.Child = contentStack;
            mainGrid.Children.Add(glowBorder);
            mainGrid.Children.Add(glassBorder);

            toast.Content = mainGrid;

            // 动画效果
            toast.Loaded += (s, e) =>
            {
                UpdatePositions();
                
                // 进场动画
                var sb = new Storyboard();
                
                // 窗口整体淡入和滑动
                var fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(600)) { EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut } };
                Storyboard.SetTarget(fadeIn, mainGrid);
                Storyboard.SetTargetProperty(fadeIn, new PropertyPath(UIElement.OpacityProperty));

                var slideIn = new DoubleAnimation(50, 0, TimeSpan.FromMilliseconds(800)) { EasingFunction = new BackEase { Amplitude = 0.5, EasingMode = EasingMode.EaseOut } };
                var translate = new TranslateTransform();
                
                // 缩放动画
                var scale = new ScaleTransform(0.8, 0.8);
                mainGrid.RenderTransformOrigin = new System.Windows.Point(0.5, 0.5);
                var group = new TransformGroup();
                group.Children.Add(translate);
                group.Children.Add(scale);
                mainGrid.RenderTransform = group;

                Storyboard.SetTarget(slideIn, mainGrid);
                Storyboard.SetTargetProperty(slideIn, new PropertyPath("RenderTransform.Children[0].X"));

                // 打钩路径动画
                checkPath.StrokeDashArray = new DoubleCollection { 10, 10 };
                var pathAnim = new DoubleAnimation(20, 0, TimeSpan.FromMilliseconds(1000)) { BeginTime = TimeSpan.FromMilliseconds(400) };
                Storyboard.SetTarget(pathAnim, checkPath);
                Storyboard.SetTargetProperty(pathAnim, new PropertyPath(Shape.StrokeDashOffsetProperty));

                var scaleAnimX = new DoubleAnimation(0.8, 1, TimeSpan.FromMilliseconds(600)) { EasingFunction = new ElasticEase { Oscillations = 1, Springiness = 5 } };
                Storyboard.SetTarget(scaleAnimX, mainGrid);
                Storyboard.SetTargetProperty(scaleAnimX, new PropertyPath("RenderTransform.Children[1].ScaleX"));
                
                var scaleAnimY = new DoubleAnimation(0.8, 1, TimeSpan.FromMilliseconds(600)) { EasingFunction = new ElasticEase { Oscillations = 1, Springiness = 5 } };
                Storyboard.SetTarget(scaleAnimY, mainGrid);
                Storyboard.SetTargetProperty(scaleAnimY, new PropertyPath("RenderTransform.Children[1].ScaleY"));

                // 图标弹出动画
                var iconScaleX = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(500)) { BeginTime = TimeSpan.FromMilliseconds(200), EasingFunction = new BackEase { Amplitude = 1, EasingMode = EasingMode.EaseOut } };
                Storyboard.SetTarget(iconScaleX, checkPath);
                Storyboard.SetTargetProperty(iconScaleX, new PropertyPath("RenderTransform.ScaleX"));

                var iconScaleY = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(500)) { BeginTime = TimeSpan.FromMilliseconds(200), EasingFunction = new BackEase { Amplitude = 1, EasingMode = EasingMode.EaseOut } };
                Storyboard.SetTarget(iconScaleY, checkPath);
                Storyboard.SetTargetProperty(iconScaleY, new PropertyPath("RenderTransform.ScaleY"));

                sb.Children.Add(fadeIn);
                sb.Children.Add(iconScaleX);
                sb.Children.Add(iconScaleY);
                sb.Children.Add(slideIn);
                sb.Children.Add(pathAnim);
                sb.Children.Add(scaleAnimX);
                sb.Children.Add(scaleAnimY);
                sb.Begin();

                // 自动关闭
                var timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(3.5) };
                timer.Tick += (s2, e2) =>
                {
                    timer.Stop();
                    var sbOut = new Storyboard();
                    var fadeOut = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(500));
                    Storyboard.SetTarget(fadeOut, mainGrid);
                    Storyboard.SetTargetProperty(fadeOut, new PropertyPath(UIElement.OpacityProperty));
                    
                    var slideOut = new DoubleAnimation(0, 30, TimeSpan.FromMilliseconds(500)) { EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn } };
                    Storyboard.SetTarget(slideOut, mainGrid);
                    Storyboard.SetTargetProperty(slideOut, new PropertyPath("RenderTransform.Children[0].X"));

                    sbOut.Completed += (s3, e3) => 
                    {
                        lock (_activeToasts)
                        {
                            _activeToasts.Remove(toast);
                            try { toast.Close(); } catch { }
                            UpdatePositions();
                        }
                    };
                    sbOut.Children.Add(fadeOut);
                    sbOut.Children.Add(slideOut);
                    sbOut.Begin();
                };
                timer.Start();
            };

            lock (_activeToasts)
            {
                _activeToasts.Add(toast);
            }
            toast.Show();
        }

        private static void UpdatePositions()
        {
            // 获取屏幕工作区（排除任务栏）
            var desktopWorkingArea = SystemParameters.WorkArea;

            lock (_activeToasts)
            {
                double currentBottom = desktopWorkingArea.Bottom - TOAST_MARGIN;
                
                // 从下往上排列
                for (int i = _activeToasts.Count - 1; i >= 0; i--)
                {
                    var t = _activeToasts[i];
                    try
                    {
                        if (t.IsLoaded)
                        {
                            t.Left = desktopWorkingArea.Right - t.ActualWidth - TOAST_MARGIN;
                            t.Top = currentBottom - t.ActualHeight;
                            
                            currentBottom -= (t.ActualHeight + TOAST_MARGIN);
                        }
                    }
                    catch { }
                }
            }
        }
    }
}
