
using System;
using System.Collections.Generic;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;

namespace DesktopFolders
{
    public sealed partial class MainPage : Page
    {
        private readonly List<DesktopItem> _items = new()
        {
            new DesktopItem("FOLDER", true, 40, 50,
                new List<DesktopItem>
                {
                    new DesktopItem("Item-1"),
                    new DesktopItem("Item-2"),
                    new DesktopItem("Item-3"),
                    new DesktopItem("Subfolder", true,
                        children: new List<DesktopItem>
                        {
                            new DesktopItem("Document.txt"),
                            new DesktopItem("Image.png"),
                            new DesktopItem("Another.txt")
                        })
                }),

            new DesktopItem("WORK", true, 40, 180,
                new List<DesktopItem>
                {
                    new DesktopItem("Project.cs"),
                    new DesktopItem("Notes.txt"),
                    new DesktopItem("Assets", true,
                        children: new List<DesktopItem>
                        {
                            new DesktopItem("logo.png"),
                            new DesktopItem("icon.png")
                        })
                }),

            new DesktopItem("Document.txt", false, 40, 310)
        };

        public MainPage()
        {
            InitializeComponent();

            // Add the desktop items when the page is created.
            foreach (var item in _items)
            {
                AddDesktopItem(item);
            }
        }

        private void AddDesktopItem(DesktopItem item)
        {
            var view = CreateItemView(item);

            Canvas.SetLeft(view, item.X);
            Canvas.SetTop(view, item.Y);

            DesktopCanvas.Children.Add(view);
        }

        private UIElement CreateItemView(DesktopItem item)
        {
            var container = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                VerticalAlignment = VerticalAlignment.Top,
                Spacing = 0
            };

            // Folder or file icon.
            var icon = new TextBlock
            {
                Text = item.IsFolder ? "\uD83D\uDCC1" : "\uD83D\uDCC4",
                FontSize = 36,
                HorizontalAlignment = HorizontalAlignment.Center
            };

            var label = new TextBlock
            {
                Text = item.Name,
                FontSize = 12,
                TextAlignment = TextAlignment.Center,
                TextWrapping = TextWrapping.Wrap,
                MaxWidth = 88,
                Foreground = new SolidColorBrush(Colors.White)
            };

            var itemContent = new StackPanel
            {
                Width = 100,
                Spacing = 4,
                HorizontalAlignment = HorizontalAlignment.Center
            };

            itemContent.Children.Add(icon);
            itemContent.Children.Add(label);

            var button = new Button
            {
                Content = itemContent,
                Width = 110,
                MinHeight = 76,
                Padding = new Thickness(4),
                Background = new SolidColorBrush(Colors.Transparent),
                BorderThickness = new Thickness(0),
                HorizontalContentAlignment = HorizontalAlignment.Center
            };
            
            button.Click += (sender, args) =>
            {
                if (!item.IsFolder || item.IsAnimating)
                    return;

                if (item.IsExpanded)
                {
                    AnimateFolderClose(item);
                }
                else
                {
                    item.IsExpanded = true;
                    RefreshItem(item);
                }
            };

            container.Children.Add(button);
            item.Button = button;

            // Add folder contents horizontally to the right.
            if (item.IsFolder && item.IsExpanded)
            {
                var childrenPanel = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    VerticalAlignment = VerticalAlignment.Top,
                    Spacing = 0
                };

                for (int i = 0; i < item.Children.Count; i++)
                {
                    var child = item.Children[i];
                    var childView = CreateItemView(child);

                    childrenPanel.Children.Add(childView);

                    // Stagger the items for a cascading opening effect.
                    AnimateItemIn(childView, i);
                }

                container.Children.Add(childrenPanel);
                item.ChildrenPanel = childrenPanel;
            }
            else
            {
                item.ChildrenPanel = null;
            }

            item.View = container;
            return container;
        }

        private void RefreshItem(DesktopItem item)
        {
            if (item.View == null)
                return;

            UIElement oldView = item.View;

            // Build the replacement visual.
            UIElement newView = CreateItemView(item);

            // Top-level items live on the Canvas.
            if (oldView is FrameworkElement oldElement &&
                oldElement.Parent is Canvas canvas)
            {
                double x = item.X;
                double y = item.Y;

                canvas.Children.Remove(oldView);

                Canvas.SetLeft(newView, x);
                Canvas.SetTop(newView, y);

                canvas.Children.Add(newView);
                return;
            }

            // Nested items must be replaced inside their parent's panel.
            var parentPanel = FindParentPanel(item);

            if (parentPanel != null)
            {
                int index = parentPanel.Children.IndexOf(oldView);

                if (index >= 0)
                {
                    parentPanel.Children.RemoveAt(index);
                    parentPanel.Children.Insert(index, newView);
                }
            }
        }

        private StackPanel? FindParentPanel(DesktopItem item)
        {
            foreach (var root in _items)
            {
                var result = FindParentPanelRecursive(root, item);

                if (result != null)
                    return result;
            }

            return null;
        }

        private StackPanel? FindParentPanelRecursive(
            DesktopItem current,
            DesktopItem target)
        {
            if (current.IsFolder &&
                current.IsExpanded &&
                current.ChildrenPanel != null)
            {
                foreach (var child in current.Children)
                {
                    if (child == target)
                        return current.ChildrenPanel;

                    var result = FindParentPanelRecursive(child, target);

                    if (result != null)
                        return result;
                }
            }

            return null;
        }

        private void AnimateItemIn(UIElement element, int index)
        {
            var transform = new TranslateTransform
            {
                X = -30
            };

            element.RenderTransform = transform;
            element.Opacity = 0;

            var storyboard = new Storyboard();

            // Slide the item into its final position.
            var slide = new DoubleAnimation
            {
                From = -30,
                To = 0,
                Duration = new Duration(
                    TimeSpan.FromMilliseconds(320)),
                BeginTime = TimeSpan.FromMilliseconds(index * 55),
                EasingFunction = new CubicEase
                {
                    EasingMode = EasingMode.EaseOut
                }
            };

            Storyboard.SetTarget(slide, transform);
            Storyboard.SetTargetProperty(slide, "X");

            // Fade the item in at the same time.
            var fade = new DoubleAnimation
            {
                From = 0,
                To = 1,
                Duration = new Duration(
                    TimeSpan.FromMilliseconds(220)),
                BeginTime = TimeSpan.FromMilliseconds(index * 55)
            };

            Storyboard.SetTarget(fade, element);
            Storyboard.SetTargetProperty(fade, "Opacity");

            storyboard.Children.Add(slide);
            storyboard.Children.Add(fade);

            storyboard.Begin();
        }

        private void AnimateFolderClose(DesktopItem item)
        {
            if (item.ChildrenPanel == null ||
                item.ChildrenPanel.Children.Count == 0)
            {
                item.IsExpanded = false;
                RefreshItem(item);
                return;
            }

            item.IsAnimating = true;

            var children = new List<UIElement>();

            foreach (UIElement child in item.ChildrenPanel.Children)
            {
                children.Add(child);
            }

            int completed = 0;

            for (int i = 0; i < children.Count; i++)
            {
                var child = children[i];

                var transform = new TranslateTransform
                {
                    X = 0
                };

                child.RenderTransform = transform;
                child.Opacity = 1;

                var storyboard = new Storyboard();

                var slide = new DoubleAnimation
                {
                    From = 0,
                    To = -30,
                    Duration = new Duration(
                        TimeSpan.FromMilliseconds(280)),
                    BeginTime = TimeSpan.FromMilliseconds(i * 45),
                    EasingFunction = new CubicEase
                    {
                        EasingMode = EasingMode.EaseIn
                    }
                };

                Storyboard.SetTarget(slide, transform);
                Storyboard.SetTargetProperty(slide, "X");

                var fade = new DoubleAnimation
                {
                    From = 1,
                    To = 0,
                    Duration = new Duration(
                        TimeSpan.FromMilliseconds(220)),
                    BeginTime = TimeSpan.FromMilliseconds(i * 45)
                };

                Storyboard.SetTarget(fade, child);
                Storyboard.SetTargetProperty(fade, "Opacity");

                storyboard.Children.Add(slide);
                storyboard.Children.Add(fade);

                storyboard.Completed += (s, e) =>
                {
                    completed++;

                    // Collapse only after every child has finished.
                    if (completed == children.Count)
                    {
                        item.IsExpanded = false;
                        item.IsAnimating = false;
                        RefreshItem(item);
                    }
                };

                storyboard.Begin();
            }
        }

        private sealed class DesktopItem
        {
            public string Name { get; }
            public bool IsFolder { get; }
            public List<DesktopItem> Children { get; }

            public double X { get; }
            public double Y { get; }

            public bool IsExpanded { get; set; }
            public bool IsAnimating { get; set; }

            public Button? Button { get; set; }
            public UIElement? View { get; set; }
            public StackPanel? ChildrenPanel { get; set; }

            public DesktopItem(
                string name,
                bool isFolder = false,
                double x = 0,
                double y = 0,
                List<DesktopItem>? children = null)
            {
                Name = name;
                IsFolder = isFolder;
                X = x;
                Y = y;
                Children = children ?? new List<DesktopItem>();
            }
        }
    }
}