using System.Collections.Concurrent;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Interactions;

namespace VoiceNoteTranscription;

public class InstagramClient : Client
{
    public static ConcurrentDictionary<string, string> MessageBuffer;
    private readonly ChromeDriver _browser;
    private readonly NetworkManager _manager;

    private DateTime _timeStarted;
    private readonly TimeSpan _timeToIgnore = new TimeSpan(0, 0, 3);
    private readonly TimeSpan _refreshInterval = new TimeSpan(4, 1, 39);

    private string _lastVNlink = "";

    private Task _currentHandleRequest;

    public InstagramClient(string filepath, bool logging) : base(filepath, logging)
    {
        var opt = new ChromeOptions();
        MessageBuffer = new ConcurrentDictionary<string, string>();

        opt.AddArgument("user-data-dir=google-chrome");
        opt.AddArgument($"profile-directory={System.Environment.GetEnvironmentVariable("PROFILE_NAME")}");
        opt.AddArgument("user-agent=Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36");
        
        opt.AddExcludedArguments(new string[]{
            "enable-automation",
            "enable-logging",
            "use-mock-keychain",
            "password-store",
            "allow-pre-commit-input",
            "disable-background-networking",
            "disable-blink-features",
            "disable-client-side-phishing-detection",
            "disable-default-apps",
            "disable-hang-monitor",
            "disable-popup-blocking",
            "disable-prompt-on-repost",
            "disable-sync",
            "log-level",
            "no-first-run",
            "no-service-autorun",
            "test-type"
        });
        opt.AddAdditionalOption("useAutomationExtension", false);
        #if RELEASE
            opt.AddArgument("headless");
        #endif
        
        _browser = new ChromeDriver(opt);
        _manager = new NetworkManager(_browser);
    }

    public override async Task Start()
    {
        #if RELEASE
            _browser.Navigate().GoToUrl("https://www.instagram.com/direct/t/104910557574628/");
        #else
            _browser.Navigate().GoToUrl("https://www.instagram.com/direct/t/259648092797288/");
        #endif
        
        _manager.NetworkRequestSent += (sender, e) =>
        {
            _currentHandleRequest = HandleRequest(sender!, e);
        };
        _timeStarted = DateTime.Now;
        
        var monitorService = _manager.StartMonitoring();
        var listenService = Task.Run(ListenForText);
        var periodicRefresh = Task.Run(StartPeriodicRefresh);
        
        _browser.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);
        
        await monitorService;
        await listenService;
        await periodicRefresh;
    }
    private async Task StartPeriodicRefresh()
    {
        while (true)
        {
            await Task.Delay(_refreshInterval);
            if (_currentHandleRequest != null)
            {
                await _currentHandleRequest;
            }
            try
            {
                Console.WriteLine($"Refreshing browser at {DateTime.Now}");
                _timeStarted = DateTime.MaxValue;
                await _browser.Navigate().RefreshAsync();
                _timeStarted = DateTime.Now;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during refresh: {ex.Message}");
            }
        }
    }

    private async Task HandleRequest(object sender, NetworkRequestSentEventArgs e)
    {
        if (DateTime.Now - _timeStarted < _timeToIgnore) return;
        if (e.RequestUrl.Contains("cdn.fbsbx.com") && e.RequestUrl != _lastVNlink)
        {
            Console.WriteLine($"recived voice note at {e.RequestUrl}");
            _lastVNlink = e.RequestUrl;

            if (DoNextVoiceMessage)
                try
                {
                    IWebElement voiceNote = _browser
                        .FindElements(
                            By.XPath("//div[@aria-label='Double tap to like']//div[contains(@style, 'clip-path:')]"))
                        .Last();

                    MessageInfo transcribed =
                        await new AudioFileHandler(FilePath).ProcessDownloadUrl(e.RequestUrl, "mp4");
                    ReplyToMessage(transcribed, voiceNote);
                }
                catch (Exception except)
                {
                    Console.WriteLine(except);
                    Console.WriteLine("Couldn't process voice note");
                }
        }
    }

    private void ListenForText()
    {
        var lastText = "";
        while (true)
        {
            try
            {
                // Console.WriteLine(_browser.FindElements(By.XPath("//div[@aria-label='Double tap to like']")));
                var message = _browser.FindElements(By.XPath("//div[@aria-label='Double tap to like']")).Last()
                    .FindElement(By.XPath(".//div[@dir='auto']"));
                if (message.Text != lastText)
                {
                    Console.WriteLine($"message text: {message.Text}");
                    lastText = message.Text;
                }

                var lowerText = message.Text.ToLower();
                if (lowerText == "donttldr" || lowerText == "/donttldr")
                {
                    Console.WriteLine("donttldr recieved");
                    DoNextVoiceMessage = false;
                }
                else if (lowerText == "dontdonttldr" || lowerText == "/dontdonttldr")
                {
                    Console.WriteLine("dontdonttldr recieved");
                    DoNextVoiceMessage = true;
                }
                else if (lowerText.Contains("good bot"))
                {
                    SendMessage("thank U :3");
                }
                else if (lowerText.Contains("bad bot") || lowerText.Contains("stupid ass bot"))
                {
                    SendMessage("kys (insert slur here)");
                }
            }
            catch
            {
            }

            Thread.Sleep(100);
        }
    }

    private void ReplyToMessage(MessageInfo message, IWebElement voiceNote)
    {
        if (Logging)
        {
            var parentElement = voiceNote.FindElement(By.XPath("ancestor::div[@aria-label='Double tap to like']"));
            var messageParentElement = parentElement.FindElement(By.XPath("../.."));
            message.Sender = messageParentElement.FindElement(By.XPath(".//a[@role='link']")).GetDomAttribute("href").TrimStart('/');
            
            Log(message, "Instagram.json");
            
        };

        var messageBox = _browser.FindElement(By.XPath("//div[contains(text(),'Message...')]"));
        new Actions(_browser)
            .MoveToElement(voiceNote)
            .MoveByOffset(180, 0)
            .Click()
            .MoveToElement(messageBox)
            .Click()
            .SendKeys(message.Message)
            .SendKeys(Keys.Return)
            .Pause(TimeSpan.FromSeconds(1))
            .Perform();
    }
    
    private void SendMessage(string message)
    {
        var messageBox = _browser.FindElement(By.XPath("//div[contains(text(),'Message...')]"));
        new Actions(_browser)
            .MoveToElement(messageBox)
            .Click()
            .SendKeys(message)
            .SendKeys(Keys.Return)
            .Pause(TimeSpan.FromSeconds(0.5))
            .Perform();
    }
}