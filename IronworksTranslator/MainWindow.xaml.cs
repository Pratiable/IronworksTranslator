using FontAwesome5;
using IronworksTranslator.Core;
using IronworksTranslator.Util;
using Serilog;
using Sharlayan.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Xceed.Wpf.Toolkit;
using Fonts = System.Windows.Media.Fonts;
using System.Windows.Controls.Primitives;

namespace IronworksTranslator
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {
        public IronworksContext ironworksContext;
        public IronworksSettings ironworksSettings;
        //private 
        private readonly Timer chatboxTimer;
        private DialogueWindow dialogueWindow;

        private bool isUiInitialized = false;
        private bool isAutoScrollEnabled = true;
        private const int MAX_TEXT_LINES = 1000;
        private ScrollViewer textBoxScrollViewer;
        private bool isTextChangedByCode = false;

        public MainWindow()
        {
            Topmost = true;
            InitializeComponent();
            isUiInitialized = true;
            InitializeGeneralSettingsUI();

            var version = Assembly.GetExecutingAssembly().GetName().Version;
            mainWindow.Title += $" v{version}";
            //AppHeaderLabel.Content += $" v{version}";
            Log.Information($"Current version: {version}");

            ironworksContext = IronworksContext.Instance();
            ironworksSettings = IronworksSettings.Instance;

            Welcome();
            LoadSettings();

            ShowDialogueWindow();

            const int period = 500;
            chatboxTimer = new Timer(UpdateChatbox, null, 0, period);
            Log.Debug($"New RefreshChatbox timer with period {period}ms");

            TranslatedChatBox.TextChanged += TranslatedChatBox_TextChanged;
            
            TranslatedChatBox.Loaded += (s, e) => {
                textBoxScrollViewer = FindVisualChild<ScrollViewer>(TranslatedChatBox);
                if (textBoxScrollViewer != null)
                {
                    textBoxScrollViewer.ScrollChanged += TranslatedChatBox_ScrollChanged;
                }
                else
                {
                    Log.Warning("MainWindow: ScrollViewer not found");
                }
            };
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

        private void TranslatedChatBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            LimitTextLines(TranslatedChatBox);
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
                
                if (textBox == TranslatedChatBox)
                {
                    TranslatedChatBox.TextChanged -= TranslatedChatBox_TextChanged;
                }
                
                textBox.Text = newText;
                
                if (textBox == TranslatedChatBox)
                {
                    TranslatedChatBox.TextChanged += TranslatedChatBox_TextChanged;
                }
                
                isTextChangedByCode = false;
                
                if (wasScrolledToEnd && isAutoScrollEnabled)
                {
                    textBox.ScrollToEnd();
                }
            }
        }

        private void ShowDialogueWindow()
        {
            dialogueWindow = new DialogueWindow(GetWindow(this) as MainWindow);
            dialogueWindow.Show();
        }

        private void InitializeGeneralSettingsUI()
        {
            exampleChatBox.Text = $"이프 저격하는 무작위 레벨링 가실 분~{Environment.NewLine}エキルレ行く方いますか？{Environment.NewLine}Mechanics are for Cars KUPO!";
            exampleChatBox.Text += $"{Environment.NewLine}제작자: 사포 (sappho192@gmail.com)";
            var cond = System.Windows.Markup.XmlLanguage.GetLanguage(System.Globalization.CultureInfo.CurrentUICulture.Name);
            Log.Debug($"System language: {cond}");
            var listFont = new List<string>();
            foreach (FontFamily font in Fonts.SystemFontFamilies)
            {
                if (font.FamilyNames.ContainsKey(cond))
                    listFont.Add(font.FamilyNames[cond]);
                else
                    listFont.Add(font.ToString());
            }
            listFont.Sort();
            ChatFontFamilyComboBox.ItemsSource = listFont;
            Log.Debug($"listFont: {listFont}, listFont.size: {listFont.Count}");


            try
            {
                var chatBoxFontName = exampleChatBox.FontFamily.FamilyNames[cond];
                Log.Debug($"chatBoxFontName: {chatBoxFontName}");
                foreach (string fontName in ChatFontFamilyComboBox.ItemsSource)
                {
                    try
                    {
                        if (fontName.Equals(chatBoxFontName))
                        {
                            ChatFontFamilyComboBox.SelectedValue = fontName;
                            break;
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Debug($"chatBoxFontName: {chatBoxFontName}, fontName: {fontName}");
                        Log.Debug(ex.Message);
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Debug(ex.Message);
            }
        }

        private void Welcome()
        {
            //TranslatedChatBox.Text += $"최근에 강제종료했다면 작업관리자에서 PhantomJS.exe도 종료해주세요.{Environment.NewLine}";
            if (App.newcomer)
            //if (true)
            {
                TranslatedChatBox.Text += $"프로그램을 처음 쓰시는군요.{Environment.NewLine}";
                TranslatedChatBox.Text += $"메뉴 버튼을 눌러 채널별 언어 설정을 마무리해주세요.{Environment.NewLine}";
                ironworksSettings.UI.ChatTextboxFontFamily = ChatFontFamilyComboBox.SelectedValue as string;
            }
        }

        private void LoadSettings()
        {
            Log.Debug("Applying settings from file");
            LoadUISettings();
            LoadChatSettings();

            Log.Debug("Settings applied");
        }

        private void LoadUISettings()
        {
            chatFontSizeSpinner.Value = ironworksSettings.UI.ChatTextboxFontSize;
            TranslatedChatBox.FontSize = ironworksSettings.UI.ChatTextboxFontSize;
            mainWindow.Width = ironworksSettings.UI.MainWindowWidth;
            mainWindow.Height = ironworksSettings.UI.MainWindowHeight;
            if(ironworksSettings.UI.MainWindowPosTop < 0 ||
                ironworksSettings.UI.MainWindowPosTop > System.Windows.SystemParameters.PrimaryScreenHeight)
            {
                ironworksSettings.UI.MainWindowPosTop = 100;
            }
            mainWindow.Top = ironworksSettings.UI.MainWindowPosTop;
            if(ironworksSettings.UI.MainWindowPosLeft < 0 ||
                ironworksSettings.UI.MainWindowPosLeft > System.Windows.SystemParameters.PrimaryScreenWidth)
            {
                ironworksSettings.UI.MainWindowPosLeft = 100;
            }
            mainWindow.Left = ironworksSettings.UI.MainWindowPosLeft;

            ClientLanguageComboBox.SelectedIndex = (int)ironworksSettings.Translator.NativeLanguage;
            DialogueLanguageComboBox.SelectedIndex = (int)ironworksSettings.Translator.DialogueLanguage;

            TranslatorEngineComboBox.SelectedIndex = (int)ironworksSettings.Translator.DefaultTranslatorEngine;
            GeminiApiKeyTextBox.Text = ironworksSettings.Translator.GeminiApiKey;
            GeminiModelComboBox.SelectedIndex = (int)ironworksSettings.Translator.DefaultGeminiModel;
            DialogueTranslateMethodComboBox.SelectedIndex = (int)ironworksSettings.Translator.DefaultDialogueTranslationMethod;

            ContentBackgroundGrid.Opacity = ironworksSettings.UI.ChatBackgroundOpacity;
            ContentOpacitySlider.Value = ironworksSettings.UI.ChatBackgroundOpacity;
            ChatFontFamilyComboBox.SelectedValue = ironworksSettings.UI.ChatTextboxFontFamily;
            var font = new FontFamily(ironworksSettings.UI.ChatTextboxFontFamily);
            exampleChatBox.FontFamily = font;
            TranslatedChatBox.FontFamily = font;
        }

        private void Window_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            Window window = (Window)sender;
            window.Topmost = true;
        }

        private void Window_PreviewTouchDown(object sender, TouchEventArgs e)
        {
            CaptureTouch(e.TouchDevice);
        }

        private void TranslatedChatBox_ScrollChanged(object sender, ScrollChangedEventArgs e)
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

        private void UpdateChatbox(object state)
        {
            if (ChatQueue.q.Any())
            {// Should q be locked?
                var chat = ChatQueue.q.Take();
                int.TryParse(chat.Code, System.Globalization.NumberStyles.HexNumber, null, out var intCode);
                ChatCode code = (ChatCode)intCode;
                if (code <= ChatCode.CWLinkShell8)
                {// For now, CWLinkShell8(0x6B) is upper bound of chat related code.
                    if (ironworksSettings.Chat.ChannelVisibility.TryGetValue(code, out bool show))
                    {
                        if (show)
                        {
                            string line = chat.Line;
                            ChatLogItem decodedChat = chat.Bytes.DecodeAutoTranslate();
                            Log.Debug("Chat: {@Chat}", decodedChat);

                            if (code == ChatCode.Recruitment || code == ChatCode.System || code == ChatCode.Error
                                || code == ChatCode.Notice || code == ChatCode.Emote || code == ChatCode.MarketSold)
                            {
                                if (!ContainsNativeLanguage(decodedChat.Line))
                                {
                                    var translated = ironworksContext.TranslateChat(decodedChat.Line, ironworksSettings.Chat.ChannelLanguage[code]);

                                    Application.Current.Dispatcher.Invoke(() =>
                                    {
                                        isTextChangedByCode = true;
                                        TranslatedChatBox.Text +=
#if DEBUG
                                        $"{translated}{Environment.NewLine}";
#else
                                        $"{translated}{Environment.NewLine}";
#endif
                                        isTextChangedByCode = false;
                                    });
                                }
                            }
                            else
                            {
                                if (!code.Equals(ChatCode.NPCDialog) && !code.Equals(ChatCode.NPCAnnounce))
                                {
                                    var author = decodedChat.Line.RemoveAfter(":");
                                    var sentence = decodedChat.Line.RemoveBefore(":");
                                    if (!ContainsNativeLanguage(decodedChat.Line))
                                    {
                                        var (translatedAuthor, translatedSentence) = ironworksContext.TranslateChatWithAuthor(
                                            author, 
                                            sentence, 
                                            ironworksSettings.Chat.ChannelLanguage[code],
                                            code
                                        );

                                        Application.Current.Dispatcher.Invoke(() =>
                                        {
                                            TranslatedChatBox.Text +=
#if DEBUG
                                        $"{translatedAuthor}: {translatedSentence}{Environment.NewLine}";
#else
                                        $"{translatedAuthor}: {translatedSentence}{Environment.NewLine}";
#endif
                                        });
                                    }
                                }
                                else
                                {
                                    if (ironworksSettings.Translator.DefaultDialogueTranslationMethod == DialogueTranslationMethod.MemorySearch)
                                    {
                                        // Skip update for chat
                                    }
                                    else if (ironworksSettings.Translator.DefaultDialogueTranslationMethod == DialogueTranslationMethod.ChatMessage)
                                    {
                                        string translatedText;
                                        
                                        var colonIndex = decodedChat.Line.IndexOf(':');
                                        if (colonIndex > 0 && colonIndex < decodedChat.Line.Length - 1)
                                        {
                                            var author = decodedChat.Line.Substring(0, colonIndex);
                                            var sentence = decodedChat.Line.Substring(colonIndex + 1);
                                            
                                            var (translatedAuthor, translatedSentence) = ironworksContext.TranslateChatWithAuthor(
                                                author, 
                                                sentence, 
                                                ironworksSettings.Chat.ChannelLanguage[code],
                                                code,
                                                true
                                            );
                                            
                                            translatedText = $"{translatedAuthor}: {translatedSentence}";
                                        }
                                        else
                                        {
                                            translatedText = ironworksContext.TranslateChat(
                                                decodedChat.Line, 
                                                ironworksSettings.Chat.ChannelLanguage[code]
                                            );
                                        }

                                        Application.Current.Dispatcher.Invoke(() =>
                                        {
                                            dialogueWindow.PushDialogueTextBox($"{translatedText}{Environment.NewLine}");
                                        });
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        Log.Information("Unexpected {@Code} when translating {@Message}", intCode, chat.Line);
                        //Application.Current.Dispatcher.Invoke(() =>
                        //{
                        //    TranslatedChatBox.Text +=
                        //$"[모르는 채널-제보요망][{chat.Code}]{chat.Line}{Environment.NewLine}";
                        //});
                    }
                }
                else
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        isTextChangedByCode = true;
                        TranslatedChatBox.Text +=
#if DEBUG
                            $"{chat.Line}{Environment.NewLine}";
#else
                            $"{chat.Line}{Environment.NewLine}";
#endif
                        isTextChangedByCode = false;
                    });
                }
                Application.Current.Dispatcher.Invoke(() =>
                {
                    if (isAutoScrollEnabled)
                    {
                        isTextChangedByCode = true;
                        TranslatedChatBox.ScrollToEnd();
                        isTextChangedByCode = false;
                    }
                    //TranslatedChatBox.ScrollToVerticalOffset(double.MaxValue);
                });
            }
        }

        private bool ContainsNativeLanguage(string sentence)
        {
            switch (ironworksSettings.Translator.NativeLanguage)
            {
                case ClientLanguage.Japanese:
                    return sentence.HasJapanese();
                case ClientLanguage.English:
                    return sentence.HasEnglish();
                case ClientLanguage.Korean:
                    return sentence.HasKorean();
                case ClientLanguage.German:
                case ClientLanguage.French:
                default:
                    return false;
            }
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            if (System.Windows.MessageBox.Show("정말 종료할까요?", "프로그램 종료", MessageBoxButton.YesNo).Equals(MessageBoxResult.Yes))
            {
                Application.Current.Shutdown();
            }
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
            ShowOnly(UI.SettingsTab.None); // Hide all settings grid
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.MainWindow.WindowState = System.Windows.WindowState.Minimized;
        }

        private void GeneralSettingsButton_Click(object sender, RoutedEventArgs e)
        {
            ToggleSettingsGrid(GeneralSettingsGrid, UI.SettingsTab.General);
        }

        private void LanguageSettingsButton_Click(object sender, RoutedEventArgs e)
        {
            ToggleSettingsGrid(LanguageSettingsGrid, UI.SettingsTab.Language);
        }

        private void ChatSettingsButton_Click(object sender, RoutedEventArgs e)
        {
            ToggleSettingsGrid(ChatSettingsGrid, UI.SettingsTab.Chat);
        }

        private void ToggleSettingsGrid(Grid settingsGrid, UI.SettingsTab setting)
        {
            if (settingsGrid.Visibility.Equals(Visibility.Hidden))
            {
                ShowOnly(setting);
            }
            else
            {
                settingsGrid.Visibility = Visibility.Hidden;
            }
        }

        private void ShowOnly(UI.SettingsTab active)
        {
            switch (active)
            {
                case UI.SettingsTab.General:
                    GeneralSettingsGrid.Visibility = Visibility.Visible;
                    LanguageSettingsGrid.Visibility = Visibility.Hidden;
                    ChatSettingsGrid.Visibility = Visibility.Hidden;
                    break;
                case UI.SettingsTab.Language:
                    GeneralSettingsGrid.Visibility = Visibility.Hidden;
                    LanguageSettingsGrid.Visibility = Visibility.Visible;
                    ChatSettingsGrid.Visibility = Visibility.Hidden;
                    break;
                case UI.SettingsTab.Chat:
                    GeneralSettingsGrid.Visibility = Visibility.Hidden;
                    LanguageSettingsGrid.Visibility = Visibility.Hidden;
                    ChatSettingsGrid.Visibility = Visibility.Visible;
                    break;
                case UI.SettingsTab.None:
                    GeneralSettingsGrid.Visibility = Visibility.Hidden;
                    LanguageSettingsGrid.Visibility = Visibility.Hidden;
                    ChatSettingsGrid.Visibility = Visibility.Hidden;
                    break;
            }
        }

        private void ChatFontSizeSpinner_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            IntegerUpDown spinner = sender as IntegerUpDown;
            if (ironworksSettings != null)
            {
                ironworksSettings.UI.ChatTextboxFontSize = spinner.Value ?? 6;
                TranslatedChatBox.FontSize = ironworksSettings.UI.ChatTextboxFontSize;
                exampleChatBox.FontSize = ironworksSettings.UI.ChatTextboxFontSize;
                if (dialogueWindow != null)
                {
                    dialogueWindow.DialogueTextBox.FontSize = ironworksSettings.UI.ChatTextboxFontSize;
                }
            }
        }

        private void ClientLanguageComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ironworksSettings != null)
            {
                ComboBox box = sender as ComboBox;
                ironworksSettings.Translator.NativeLanguage = (ClientLanguage)box.SelectedIndex;
            }
        }

        private void TranslatorEngineComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ironworksSettings != null)
            {
                ComboBox box = sender as ComboBox;
                ironworksSettings.Translator.DefaultTranslatorEngine = (TranslatorEngine)box.SelectedIndex;
            }
        }

        private void ChangeMajorLanguage(Settings.Channel channel, ClientLanguage language)
        {
            if (ironworksSettings != null)
            {
                channel.MajorLanguage = language;
            }
        }

        private void ChatFontFamilyComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ironworksSettings != null)
            {
                var box = sender as ComboBox;
                if (box.SelectedItem != null)
                {
                    ironworksSettings.UI.ChatTextboxFontFamily = box.SelectedItem as string;
                    var font = new FontFamily(ironworksSettings.UI.ChatTextboxFontFamily);
                    exampleChatBox.FontFamily = font;
                    TranslatedChatBox.FontFamily = font;
                    if (dialogueWindow != null)
                    {
                        dialogueWindow.DialogueTextBox.FontFamily = font;
                    }
                }
            }
        }

        private void MainWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (ironworksSettings != null)
            {
                var window = sender as Window;
                ironworksSettings.UI.MainWindowWidth = window.Width;
                ironworksSettings.UI.MainWindowHeight = window.Height;
            }
            
            if (isAutoScrollEnabled)
            {
                TranslatedChatBox.ScrollToEnd();
            }
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
            ironworksSettings.UI.ChatBackgroundOpacity = opacity;
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

        private void ToggleDialogueWindowButton_Click(object sender, RoutedEventArgs e)
        {
            if (dialogueWindow.Visibility.Equals(Visibility.Visible))
            {
                dialogueWindow.Visibility = Visibility.Hidden;
            }
            else
            {
                dialogueWindow.Visibility = Visibility.Visible;
            }
        }

        private void DialogueTranslateMethod_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ironworksSettings != null)
            {
                ComboBox box = sender as ComboBox;
                var idx = (DialogueTranslationMethod)box.SelectedIndex;
                ironworksSettings.Translator.DefaultDialogueTranslationMethod = idx;
                switch (idx)
                {
                    case DialogueTranslationMethod.ChatMessage:
                        lbDialogueTranslationMethodHint.Content = "  * (NPC 대사가 채팅에 나오게 설정해야합니다)";
                        DialogueLanguageMemoryGroup.Visibility = Visibility.Collapsed;
                        DialogueLanguageChatMessageGroup.Visibility = Visibility.Visible;
                        break;
                    case DialogueTranslationMethod.MemorySearch:
                        lbDialogueTranslationMethodHint.Content = "  * (매번 패치 이후에 동작하지 않을 수 있습니다)";
                        DialogueLanguageMemoryGroup.Visibility = Visibility.Visible;
                        DialogueLanguageChatMessageGroup.Visibility = Visibility.Collapsed;
                        break;
                    default:
                        break;
                }
            }
        }

        private void mainWindow_LocationChanged(object sender, EventArgs e)
        {
            if (isUiInitialized)
            {
                if (mainWindow.WindowState == System.Windows.WindowState.Normal)
                {
                    if (mainWindow.Top > 0 && mainWindow.Top < SystemParameters.PrimaryScreenHeight)
                    {
                        ironworksSettings.UI.MainWindowPosTop = mainWindow.Top;
                    }
                    if (mainWindow.Left > 0 && mainWindow.Left < SystemParameters.PrimaryScreenWidth)
                    {
                        ironworksSettings.UI.MainWindowPosLeft = mainWindow.Left;
                    }
                }
            }
        }

        private void DialogueLanguageComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ironworksSettings != null)
            {
                var languageIndex = ((ComboBox)sender).SelectedIndex;
                ironworksSettings.Translator.DialogueLanguage = (ClientLanguage)languageIndex;
            }
        }

        private void ChatDialogueLanguageComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ironworksSettings != null)
            {
                var languageIndex = ((ComboBox)sender).SelectedIndex;
                ironworksSettings.Chat.NPCDialog.MajorLanguage = (ClientLanguage)languageIndex;
                NPCDialogComboBox.SelectedIndex = languageIndex;
            }
        }

        private void GeminiApiKeyTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (ironworksSettings != null)
            {
                TextBox box = sender as TextBox;
                ironworksSettings.Translator.GeminiApiKey = box.Text;
            }
        }

        private void GeminiModelComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ironworksSettings != null)
            {
                ComboBox box = sender as ComboBox;
                ironworksSettings.Translator.DefaultGeminiModel = (GeminiModel)box.SelectedIndex;
            }
        }
    }
}
