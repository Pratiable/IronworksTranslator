using FontAwesome5;
using IronworksTranslator.Core;
using Serilog;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Controls.Primitives;

namespace IronworksTranslator
{
    /// <summary>
    /// DialogueWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class DialogueWindow : Window
    {
        private IronworksContext ironworksContext;
        private IronworksSettings ironworksSettings;
        private readonly Timer chatboxTimer;
        private static Regex regexItem = new Regex(@"&\u0003(.*)\u0002I\u0002");
        private bool isUIInitialized = false;
        private bool isAutoScrollEnabled = true;
        private const int MAX_TEXT_LINES = 1000;
        private ScrollViewer textBoxScrollViewer;
        private bool isTextChangedByCode = false;

        public DialogueWindow(MainWindow mainWindow)
        {
            Topmost = true;
            InitializeComponent();
            ironworksContext = mainWindow.ironworksContext;
            ironworksSettings = mainWindow.ironworksSettings;
            LoadUISettings();
            isUIInitialized = true;

            DialogueTextBox.TextChanged += DialogueTextBox_TextChanged;

            DialogueTextBox.Loaded += (s, e) => {
                textBoxScrollViewer = FindVisualChild<ScrollViewer>(DialogueTextBox);
                if (textBoxScrollViewer != null)
                {
                    textBoxScrollViewer.ScrollChanged += DialogueTextBox_ScrollChanged;
                }
                else
                {
                    Log.Warning("DialogueWindow: ScrollViewer not found");
                }
            };

            const int period = 500;
            chatboxTimer = new Timer(RefreshDialogueTextBox, null, 0, period);
            Log.Debug($"New RefreshChatbox timer with period {period}ms");
        }

        private static T FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(parent, i);
                if (child != null && child is T)
                {
                    return (T)child;
                }
                else
                {
                    T childOfChild = FindVisualChild<T>(child);
                    if (childOfChild != null)
                    {
                        return childOfChild;
                    }
                }
            }
            return null;
        }

        private void DialogueTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            LimitTextLines(DialogueTextBox);
        }

        private void LimitTextLines(TextBox textBox)
        {
            if (textBox == null) return;

            var lines = textBox.Text.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
            if (lines.Length > MAX_TEXT_LINES)
            {
                var excessLines = lines.Length - MAX_TEXT_LINES;
                var newText = string.Join(Environment.NewLine, lines.Skip(excessLines));
                
                bool wasScrolledToEnd = false;
                if (textBoxScrollViewer != null)
                {
                    wasScrolledToEnd = Math.Abs(textBoxScrollViewer.VerticalOffset - textBoxScrollViewer.ScrollableHeight) < 1.0;
                }
                
                isTextChangedByCode = true;
                
                textBox.TextChanged -= DialogueTextBox_TextChanged;
                textBox.Text = newText;
                textBox.TextChanged += DialogueTextBox_TextChanged;
                
                isTextChangedByCode = false;
                
                if (wasScrolledToEnd && isAutoScrollEnabled)
                {
                    textBox.ScrollToEnd();
                }
            }
        }

        private void DialogueTextBox_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (isTextChangedByCode) return;
            
            if (e.VerticalChange < 0)
            {
                isAutoScrollEnabled = false;
            }

            ScrollViewer scrollViewer = sender as ScrollViewer;
            if (scrollViewer != null && 
                Math.Abs(scrollViewer.VerticalOffset - scrollViewer.ScrollableHeight) < 1.0)
            {
                isAutoScrollEnabled = true;
            }
        }

        private void RefreshDialogueTextBox(object state)
        {
            if (ChatQueue.rq.Any())
            {
                var result = ChatQueue.rq.TryDequeue(out string msg);
                if(ironworksSettings.Translator.DefaultDialogueTranslationMethod == DialogueTranslationMethod.MemorySearch)
                {
                    if (result)
                    {
                        msg = Regex.Replace(msg, @"\uE03C", "[HQ]");
                        msg = Regex.Replace(msg, @"\uE06F", "⇒");
                        msg = Regex.Replace(msg, @"\uE0BB", string.Empty);
                        msg = Regex.Replace(msg, @"\uFFFD", string.Empty);
                        if (msg.IndexOf('\u0002') == 0)
                        {
                            var filter = regexItem.Match(msg);
                            if (filter.Success)
                            {
                                msg = filter.Groups[1].Value;
                            }
                        }
                        
                        if (!string.IsNullOrEmpty(msg))
                        {
                            string translatedText;
                            var colonIndex = msg.IndexOf(':');
                            
                            if (colonIndex > 0 && colonIndex < msg.Length - 1)
                            {
                                var speaker = msg.Substring(0, colonIndex);
                                var message = msg.Substring(colonIndex + 1);
                                
                                var (translatedSpeaker, translatedMessage) = ironworksContext.TranslateChatWithAuthor(
                                    speaker, 
                                    message, 
                                    ironworksSettings.Translator.DialogueLanguage,
                                    ChatCode.NPCDialog,
                                    true
                                );
                                
                                translatedText = $"{translatedSpeaker}:{translatedMessage}";
                            }
                            else
                            {
                                translatedText = ironworksContext.TranslateChat(msg, ironworksSettings.Translator.DialogueLanguage);
                            }
                            
                            if (!string.IsNullOrEmpty(translatedText) && translatedText != msg)
                            {
                                Application.Current.Dispatcher.Invoke(() =>
                                {
                                    isTextChangedByCode = true;
                                    DialogueTextBox.Text += $"{Environment.NewLine}{translatedText}";
                                    isTextChangedByCode = false;
                                    
                                    if (isAutoScrollEnabled)
                                    {
                                        DialogueTextBox.ScrollToEnd();
                                    }
                                });
                            }
                            else
                            {
                                Log.Warning($"NPC 대화 번역 실패: {msg}");
                            }
                        }
                    }
                }
            }
        }

        public void PushDialogueTextBox(string dialogue)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                isTextChangedByCode = true;
                DialogueTextBox.Text += $"{Environment.NewLine}{dialogue}";
                isTextChangedByCode = false;
                
                if (isAutoScrollEnabled)
                {
                    DialogueTextBox.ScrollToEnd();
                }
            });
        }

        private void LoadUISettings()
        {
            ContentBackgroundGrid.Opacity = ironworksSettings.UI.DialogueBackgroundOpacity;
            ContentOpacitySlider.Value = ironworksSettings.UI.DialogueBackgroundOpacity;

            double maxWidth = SystemParameters.PrimaryScreenWidth * 0.8;
            double maxHeight = SystemParameters.PrimaryScreenHeight * 0.8;
            
            if (ironworksSettings.UI.DialogueWindowWidth > maxWidth)
                ironworksSettings.UI.DialogueWindowWidth = maxWidth;
            
            if (ironworksSettings.UI.DialogueWindowHeight > maxHeight)
                ironworksSettings.UI.DialogueWindowHeight = maxHeight;
                
            dialogueWindow.Width = ironworksSettings.UI.DialogueWindowWidth;
            dialogueWindow.Height = ironworksSettings.UI.DialogueWindowHeight;

            if (ironworksSettings.UI.DialogueWindowPosTop < 0 || 
                ironworksSettings.UI.DialogueWindowPosTop > SystemParameters.PrimaryScreenHeight - 100)
            {
                ironworksSettings.UI.DialogueWindowPosTop = 400;
            }
            
            if (ironworksSettings.UI.DialogueWindowPosLeft < 0 || 
                ironworksSettings.UI.DialogueWindowPosLeft > SystemParameters.PrimaryScreenWidth - 200)
            {
                ironworksSettings.UI.DialogueWindowPosLeft = 100;
            }
            
            dialogueWindow.Top = ironworksSettings.UI.DialogueWindowPosTop;
            dialogueWindow.Left = ironworksSettings.UI.DialogueWindowPosLeft;

            var font = new FontFamily(ironworksSettings.UI.ChatTextboxFontFamily);
            DialogueTextBox.FontFamily = font;
            DialogueTextBox.FontSize = ironworksSettings.UI.ChatTextboxFontSize;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Visibility = Visibility.Hidden;
        }

        private void CollapseButton_Click(object sender, RoutedEventArgs e)
        {
            var icon = (sender as Button).Content as SvgAwesome;
            icon.Icon =
                icon.Icon.Equals(EFontAwesomeIcon.Solid_Bars) ?
                EFontAwesomeIcon.Solid_AngleDoubleUp
                : EFontAwesomeIcon.Solid_Bars;
            ToolbarGrid.Visibility =
                ToolbarGrid.Visibility.Equals(Visibility.Collapsed) ?
                 Visibility.Visible : Visibility.Collapsed;
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void ContentOpacitySlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (ironworksSettings != null)
            {
                var slider = sender as Slider;
                ChangeBackgroundOpacity(slider.Value);
            }
        }

        private void ChangeBackgroundOpacity(double opacity)
        {
            ContentBackgroundGrid.Opacity = opacity;
            ironworksSettings.UI.DialogueBackgroundOpacity = opacity;
        }

        private void ContentOpacitySlider_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (ironworksSettings != null)
            {
                var slider = sender as Slider;
                const double ORIGINAL = 0.75;
                slider.Value = ORIGINAL;
                ChangeBackgroundOpacity(ORIGINAL);
            }
        }

        private void ShowContentBackground_Click(object sender, RoutedEventArgs e)
        {
            ContentOpacitySlider.Value = 1;
            ChangeBackgroundOpacity(1);
        }

        private void HideContentBackground_Click(object sender, RoutedEventArgs e)
        {
            ContentOpacitySlider.Value = 0;
            ChangeBackgroundOpacity(0);
        }

        private void dialogueWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (ironworksSettings != null)
            {
                var window = sender as Window;
                ironworksSettings.UI.DialogueWindowWidth = window.Width;
                ironworksSettings.UI.DialogueWindowHeight = window.Height;
            }
            
            if (isAutoScrollEnabled)
            {
                DialogueTextBox.ScrollToEnd();
            }
        }

        private void MaskGrid_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            DialogueTextBox.RaiseEvent(e);
        }

        private void dialogueWindow_LocationChanged(object sender, EventArgs e)
        {
            if(isUIInitialized)
            {
                if (dialogueWindow.WindowState == System.Windows.WindowState.Normal)
                {
                    if (dialogueWindow.Top > 0 && dialogueWindow.Top < SystemParameters.PrimaryScreenHeight)
                    {
                        ironworksSettings.UI.DialogueWindowPosTop = dialogueWindow.Top;
                    }
                    if (dialogueWindow.Left > 0 && dialogueWindow.Left < SystemParameters.PrimaryScreenWidth)
                    {
                        ironworksSettings.UI.DialogueWindowPosLeft = dialogueWindow.Left;
                    }
                }
            }
        }
    }
}
