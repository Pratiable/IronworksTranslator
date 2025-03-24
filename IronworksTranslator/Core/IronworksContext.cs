using Sharlayan;
using Sharlayan.Models;
using Sharlayan.Models.ReadResults;
using System;
using System.Diagnostics;
using System.Threading;
using System.Windows;
using Serilog;
using System.Collections.Generic;
using IronworksTranslator.Util;
using Sharlayan.Enums;
using HtmlAgilityPack;
using PuppeteerSharp;
using System.Threading.Tasks;
using System.Linq;
using Newtonsoft.Json;
using System.Net.Http;
using System.Text;

namespace IronworksTranslator.Core
{
    public class IronworksContext
    {
        /* Web stuff */
        public Browser webBrowser = null;
        private async Task<Browser> initBrowser()
        {
            await new BrowserFetcher().DownloadAsync(BrowserFetcher.DefaultChromiumRevision);
            var _browser = await Puppeteer.LaunchAsync(new LaunchOptions
            {
                Headless = true,
                Args = new[] {
                    "--js-flags=\"--max-old-space-size=128\""
                },
            });
            return _browser;
        }
        private Page webPage = null;

        /* FFXIV stuff */
        public bool Attached { get; }
        public static MemoryHandler CurrentMemoryHandler { get; set; }
        private static Process[] processes;
        private readonly Timer chatTimer;
        private readonly Timer rawChatTimer;

        // For chatlog you must locally store previous array offsets and indexes in order to pull the correct log from the last time you read it.
        private static int _previousArrayIndex = 0;
        private static int _previousOffset = 0;


        /* Singleton context */
        private static IronworksContext _instance;

        public static IronworksContext Instance()
        {// make new instance if null
            return _instance ??= new IronworksContext();
        }
        protected IronworksContext()
        {
            Attached = AttachGame();
            if (Attached)
            {
                Log.Information("Creating PhantomJS");
                InitWebBrowser();

                const int period = 500;
                chatTimer = new Timer(RefreshChat, null, 0, period);
                Log.Debug($"New RefreshChat timer with period {period}ms");
                const int rawPeriod = 100;
                rawChatTimer = new Timer(RefreshMessages, null, 0, rawPeriod);
                Log.Debug($"New RefreshMessages timer with period {rawPeriod}ms");

                // Following code will watch automatically kill chromeDriver.exe
                // WatchDogMain.exe is from my repo: https://github.com/sappho192/WatchDogDotNet
                BootWatchDog();
            }
            else
            {
                Application.Current.Shutdown();
            }
        }

        private static void BootWatchDog()
        {
            var pid = Process.GetCurrentProcess().Id;
            ProcessStartInfo info = new()
            {
                FileName = "WatchDogMain.exe",
                Arguments = $"{pid}",
                WorkingDirectory = System.AppDomain.CurrentDomain.BaseDirectory,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            Process watchdogProcess = Process.Start(info);
        }

        private void InitWebBrowser()
        {
            const int waitFor = 0;

            var browserTask = Task.Run(async () => await initBrowser());
            webBrowser = browserTask.GetAwaiter().GetResult();
            var pageTask = Task.Run(async () => await webBrowser.NewPageAsync());
            webPage = pageTask.GetAwaiter().GetResult();
            webPage.DefaultTimeout = waitFor;

            Log.Debug($"PhantomJS created, page load wait time: {waitFor}s");
        }

        private void RefreshMessages(object state)
        {
            var raw = AdvancedReader.getMessage();
            if (raw.Equals(""))
            {
                return;
            }
            lock (ChatQueue.rq)
            {
                if (ChatQueue.rq.TryPeek(out string lastMsg))
                {
                    if (!lastMsg.Equals(raw))
                    {
                        Log.Debug("Enqueue new message: {@message}", raw);
                        ChatQueue.rq.Enqueue(raw);
                        lock (ChatQueue.lastMsg)
                        {
                            ChatQueue.lastMsg = raw;
                        }
                    }
                }
                else
                {
                    if (!ChatQueue.lastMsg.Equals(raw))
                    {
                        Log.Debug("Enqueue new message: {@message}", raw);
                        ChatQueue.rq.Enqueue(raw);
                        lock (ChatQueue.lastMsg)
                        {
                            ChatQueue.lastMsg = raw;
                        }
                    }
                }
            }
        }

        private void RefreshChat(object state)
        {
            UpdateChat();
        }
        private static bool AttachGame()
        {
            string processName = "ffxiv_dx11";
            Log.Debug($"Finding process {processName}");

            // ko client filtering
            processes = Process.GetProcessesByName(processName).Where( x => { try { return System.IO.File.Exists(x.MainModule.FileName.Replace("game\\ffxiv_dx11.exe", "boot\\ffxivboot.exe")); } catch { return false; } }).ToArray();

            if (processes.Length > 0)
            {
                // supported: English, Chinese, Japanese, French, German, Korean
                GameRegion gameRegion = GameRegion.Global;
                GameLanguage gameLanguage = GameLanguage.English;
                // whether to always hit API on start to get the latest sigs based on patchVersion, or use the local json cache (if the file doesn't exist, API will be hit)
                bool useLocalCache = true;
                // patchVersion of game, or latest
                string patchVersion = "latest";
                Process process = processes[0];
                ProcessModel processModel = new()
                {
                    Process = process
                };

                var configuration = new SharlayanConfiguration
                {
                    ProcessModel = processModel,
                    GameLanguage = gameLanguage,
                    GameRegion = gameRegion,
                    PatchVersion = patchVersion,
                    UseLocalCache = useLocalCache
                };

                CurrentMemoryHandler = SharlayanMemoryManager.Instance.AddHandler(configuration);
                var signatures = new List<Signature>();
                // typical signature
                signatures.Add(new Signature
                {
                    Key = "ALLMESSAGES",

                    PointerPath = HermesAddress.GetLatestAddress().Address
                    /*PointerPath = new List<long>
                {
                    0x01EB7478,
                    0x8L,
                    0x18L,
                    0x20L,
                    0x100L,
                    0x0L
                }
                });*/
                });
                //signatures.Add(new Signature
                //{
                //    Key = "ALLMESSAGES2",
                //    PointerPath = new List<long>
                //{
                //    0x01E7FD80,
                //    0x108L,
                //    0x68L,
                //    0x240L,
                //    0x0L
                //}
                //});

                // adding parameter scanAllMemoryRegions as true makes huge memory leak and CPU usage.Why?
                CurrentMemoryHandler.Scanner.LoadOffsets(signatures.ToArray());

                ChatQueue.rq.Enqueue("Dialogue window");
                ChatQueue.lastMsg = "Dialogue window";
                Log.Debug($"Attached {processName}.exe ({gameLanguage})");
                MessageBox.Show($"아이언웍스 번역기를 실행합니다.");

                return true;
            }
            else
            {
                Log.Fatal($"Can't find {processName}.exe");
                MessageBox.Show($"파판을 먼저 켠 다음에 번역기를 실행해주세요.");
                return false;
            }
        }

        public bool UpdateChat()
        {
            ChatLogResult readResult = CurrentMemoryHandler.Reader.GetChatLog(_previousArrayIndex, _previousOffset);
            _previousArrayIndex = readResult.PreviousArrayIndex;
            _previousOffset = readResult.PreviousOffset;
            if (readResult.ChatLogItems.Count > 0)
            {
                foreach (var item in readResult.ChatLogItems)
                {
                    ChatCode code = (ChatCode)int.Parse(item.Code, System.Globalization.NumberStyles.HexNumber);
                    //ProcessChatMsg(readResult.ChatLogItems[i]);
                    if ((int)code < 0x9F) // Skips battle log
                    {
                        if (code == ChatCode.GilReceive || code == ChatCode.Gather || code == ChatCode.FieldAttack || code == ChatCode.EmoteCustom) continue;
                        ChatQueue.q.Add(item);
                    }
                }
                return true;
            }
            return false;
        }

        private async Task<string> RequestTranslate(string url)
        {
            await webPage.GoToAsync(url, WaitUntilNavigation.Networkidle2);
            var content = await webPage.GetContentAsync();

            var doc = new HtmlDocument();
            doc.LoadHtml(content);
            string translated = string.Empty;
            try
            {
                var pathElement = doc.GetElementbyId("txtTarget");
                translated = pathElement.InnerText.Trim();
            }
            catch (Exception e)
            {
                Log.Error($"Exception {e.Message} when translating the sentence.");
            }
            return translated;
        }

        public string TranslateChat(string sentence, ClientLanguage from)
        {
            if (IronworksSettings.Instance == null)
            {
                throw new Exception("IronworksSettings is null");
            }

            if (IronworksSettings.Instance.Translator.DefaultTranslatorEngine == TranslatorEngine.Gemini)
            {
                return TranslateChatWithGemini(sentence, from);
            }
            else
            {
                return TranslateChatWithPapago(sentence, from);
            }
        }

        public (string authorTranslated, string messageTranslated) TranslateChatWithAuthor(string author, string message, ClientLanguage from, ChatCode chatCode, bool forceTranslateAuthor = false)
        {
            if (IronworksSettings.Instance == null)
            {
                throw new Exception("IronworksSettings is null");
            }

            if (chatCode == ChatCode.NPCDialog)
            {
                bool translateNpcAuthor = true;
                
                if (IronworksSettings.Instance.Translator.DefaultTranslatorEngine == TranslatorEngine.Gemini)
                {
                    return TranslateChatWithAuthorGemini(author, message, translateNpcAuthor, from);
                }
                else
                {
                    return TranslateChatWithAuthorPapago(author, message, translateNpcAuthor, from);
                }
            }

            bool shouldTranslateAuthor = forceTranslateAuthor;
            
            if (!forceTranslateAuthor)
            {
                if (chatCode == ChatCode.Party || chatCode == ChatCode.Say || chatCode == ChatCode.Shout || 
                    chatCode == ChatCode.Yell || chatCode == ChatCode.Alliance)
                {
                    shouldTranslateAuthor = false;
                }
                else if (IronworksSettings.Instance.Chat.SpeakerTranslation.TryGetValue(chatCode, out bool translateSpeaker))
                {
                    shouldTranslateAuthor = translateSpeaker;
                }
            }

            if (IronworksSettings.Instance.Translator.DefaultTranslatorEngine == TranslatorEngine.Gemini)
            {
                return TranslateChatWithAuthorGemini(author, message, shouldTranslateAuthor, from);
            }
            else
            {
                return TranslateChatWithAuthorPapago(author, message, shouldTranslateAuthor, from);
            }
        }

        private (string authorTranslated, string messageTranslated) TranslateChatWithAuthorPapago(string author, string message, bool shouldTranslateAuthor, ClientLanguage from)
        {
            if (!shouldTranslateAuthor)
            {
                return (author, TranslateChatWithPapago(message, from));
            }

            return (TranslateChatWithPapago(author, from), TranslateChatWithPapago(message, from));
        }

        private (string authorTranslated, string messageTranslated) TranslateChatWithAuthorGemini(string author, string message, bool shouldTranslateAuthor, ClientLanguage from)
        {
            try
            {
                if (string.IsNullOrEmpty(message))
                {
                    return (author, message);
                }
                
                string apiKey = IronworksSettings.Instance.Translator.GeminiApiKey;
                if (string.IsNullOrEmpty(apiKey))
                {
                    return (author, $"[API 키 없음] {message}");
                }
                
                if (!shouldTranslateAuthor)
                {
                    return (author, TranslateChatWithGemini(message, from));
                }

                string sourceLanguage = from.ToString();
                string targetLanguage = IronworksSettings.Instance.Translator.NativeLanguage.ToString();
                
                string modelName = "gemini-2.0-flash";
                if (IronworksSettings.Instance.Translator.DefaultGeminiModel == GeminiModel.Flash)
                {
                    modelName = "gemini-2.0-flash";
                }
                else
                {
                    modelName = "gemini-2.0-flash-lite";
                }

                string apiUrl = $"https://generativelanguage.googleapis.com/v1beta/models/{modelName}:generateContent?key={apiKey}";

                var requestBody = new
                {
                    contents = new[] 
                    {
                        new 
                        {
                            role = "user",
                            parts = new[] 
                            {
                                new 
                                {
                                    text = $"Translate the following speaker name and message from {sourceLanguage} to {targetLanguage}. This is from the MMORPG game Final Fantasy XIV. Preserve the game's lore and character names, but make the translation sound natural in the target language. Return only the translated content. Speaker: '{author}', Message: '{message}'"
                                }
                            }
                        }
                    },
                    generationConfig = new
                    {
                        response_mime_type = "application/json",
                        response_schema = new
                        {
                            type = "OBJECT",
                            properties = new
                            {
                                speaker = new { type = "STRING" },
                                message = new { type = "STRING" }
                            },
                            required = new[] { "speaker", "message" }
                        }
                    }
                };
                
                var json = JsonConvert.SerializeObject(requestBody);
                var client = new HttpClient();
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                
                int maxRetries = 3;
                int currentRetry = 0;
                
                while (currentRetry < maxRetries)
                {
                    try
                    {
                        var responseTask = Task.Run(async () => 
                        {
                            var response = await client.PostAsync(apiUrl, content);
                            response.EnsureSuccessStatusCode();
                            
                            var responseBody = await response.Content.ReadAsStringAsync();
                            dynamic responseObject = JsonConvert.DeserializeObject(responseBody);
                            
                            string jsonContent = responseObject.candidates[0].content.parts[0].text;
                            dynamic translationObject = JsonConvert.DeserializeObject(jsonContent);
                            
                            return new 
                            { 
                                Speaker = (string)translationObject.speaker, 
                                Message = (string)translationObject.message 
                            };
                        });
                        
                        var result = responseTask.GetAwaiter().GetResult();
                        if (result != null)
                        {
                            return (result.Speaker, result.Message);
                        }
                        
                        currentRetry++;
                        if (currentRetry < maxRetries)
                        {
                            Thread.Sleep(1000);
                        }
                    }
                    catch (Exception e)
                    {
                        Log.Error($"Exception {e.Message} when translating with Gemini (attempt {currentRetry + 1})");
                        currentRetry++;
                        if (currentRetry < maxRetries)
                        {
                            Thread.Sleep(1000);
                        }
                    }
                }
                
                return (author, $"{message}");
            }
            catch (Exception e)
            {
                Log.Error($"Exception {e.Message} when translating with Gemini");
                return (author, $"{message}");
            }
        }

        private string TranslateChatWithPapago(string sentence, ClientLanguage from)
        {
            // 빈 문자열이나 null이면 그대로 반환
            if (string.IsNullOrEmpty(sentence))
            {
                return sentence;
            }

            string tk = "ko";
            foreach (var item in LanguageCodeList.papago)
            {
                if (IronworksSettings.Instance.Translator.NativeLanguage.ToString().Equals(item.NameEnglish))
                {
                    tk = item.Code;
                }
            }
            string sk = "ja";
            foreach (var item in LanguageCodeList.papago)
            {
                if (from.ToString().Equals(item.NameEnglish))
                {
                    sk = item.Code;
                }
            }
            string testUrl = $"https://papago.naver.com/?sk={sk}&tk={tk}&st={Uri.EscapeDataString(sentence)}";
            lock (webPage)
            {
                Log.Debug($"Locked web browser for {sentence}");
                string translated = sentence;
                int maxRetries = 3;
                int currentRetry = 0;
                
                while (currentRetry < maxRetries)
                {
                    try
                    {
                        var translateTask = Task.Run(async () => await RequestTranslate(testUrl));
                        translated = translateTask.GetAwaiter().GetResult();
                        
                        if (!string.IsNullOrEmpty(translated) && !translated.Equals(sentence))
                        {
                            return translated;
                        }
                        
                        currentRetry++;
                        if (currentRetry < maxRetries)
                        {
                            Log.Warning($"Translation attempt {currentRetry} failed, retrying...");
                            Thread.Sleep(1000);
                        }
                    }
                    catch (Exception e)
                    {
                        Log.Error($"Exception {e.Message} when translating {sentence} (attempt {currentRetry + 1})");
                        currentRetry++;
                        if (currentRetry < maxRetries)
                        {
                            Thread.Sleep(1000);
                        }
                    }
                }
                
                return $"{sentence}";
            }
        }

        private string TranslateChatWithGemini(string sentence, ClientLanguage from)
        {
            try
            {
                if (string.IsNullOrEmpty(sentence))
                {
                    return sentence;
                }
                
                string apiKey = IronworksSettings.Instance.Translator.GeminiApiKey;
                if (string.IsNullOrEmpty(apiKey))
                {
                    return $"[API 키 없음] {sentence}";
                }

                string sourceLanguage = from.ToString();
                string targetLanguage = IronworksSettings.Instance.Translator.NativeLanguage.ToString();
                
                string modelName = "gemini-2.0-flash";
                if (IronworksSettings.Instance.Translator.DefaultGeminiModel == GeminiModel.Flash)
                {
                    modelName = "gemini-2.0-flash";
                }
                else
                {
                    modelName = "gemini-2.0-flash-lite";
                }

                string apiUrl = $"https://generativelanguage.googleapis.com/v1beta/models/{modelName}:generateContent?key={apiKey}";

                var requestBody = new
                {
                    contents = new[] 
                    {
                        new 
                        {
                            role = "user",
                            parts = new[] 
                            {
                                new 
                                {
                                    text = $"Translate the following text from {sourceLanguage} to {targetLanguage}. This is from the MMORPG game Final Fantasy XIV. Preserve the game's lore and character names, but make the translation sound natural in the target language. Avoid literal translations and use natural expressions that native speakers would use. If translating to Korean, use casual and natural Korean expressions rather than formal or literal translations. Return only the translation with no explanations: {sentence}"
                                }
                            }
                        }
                    }
                };
                
                var json = JsonConvert.SerializeObject(requestBody);
                var client = new HttpClient();
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                
                int maxRetries = 3;
                int currentRetry = 0;
                
                while (currentRetry < maxRetries)
                {
                    try
                    {
                        var responseTask = Task.Run(async () => 
                        {
                            var response = await client.PostAsync(apiUrl, content);
                            response.EnsureSuccessStatusCode();
                            
                            var responseBody = await response.Content.ReadAsStringAsync();
                            dynamic responseObject = JsonConvert.DeserializeObject(responseBody);
                            
                            return responseObject.candidates[0].content.parts[0].text;
                        });
                        
                        string translated = responseTask.GetAwaiter().GetResult();
                        if (!string.IsNullOrEmpty(translated) && !translated.Equals(sentence))
                        {
                            return translated;
                        }
                        
                        currentRetry++;
                        if (currentRetry < maxRetries)
                        {
                            Thread.Sleep(1000);
                        }
                    }
                    catch (Exception e)
                    {
                        Log.Error($"Exception {e.Message} when translating with Gemini: {sentence} (attempt {currentRetry + 1})");
                        currentRetry++;
                        if (currentRetry < maxRetries)
                        {
                            Thread.Sleep(1000);
                        }
                    }
                }
                
                return $"[번역 실패] {sentence}";
            }
            catch (Exception e)
            {
                Log.Error($"Exception {e.Message} when translating with Gemini: {sentence}");
                return $"[번역 오류] {sentence}";
            }
        }
    }
}
