using System.Collections.Concurrent;
using Google.Cloud.Speech.V1;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Interactions;

namespace VoiceNoteTranscription.Clients;

public class InstagramClient : Client
{
    public static ConcurrentDictionary<string, string> MessageBuffer;
    private readonly ChromeDriver _browser;
    private readonly NetworkManager _manager;

    private string _lastVNlink = "";
    //private IWebElement _messageWindow;
    //private IJavaScriptEngine _monitor;

    public InstagramClient(string filepath, bool logging) : base(filepath, logging)
    {
        Console.WriteLine(AppContext.BaseDirectory);

        var opt = new ChromeOptions();
        MessageBuffer = new ConcurrentDictionary<string, string>();

        //_opt.AddArgument("disable-extensions");
        //    new { enabled_lab_experiments = new[] { "profile.managed_default_content_settings.images@2" } });

        opt.AddArgument("user-data-dir=/home/diamond/Projects/Instagram-Voice-Note-Transcription/google-chrome");
        opt.AddArgument("profile-directory=Profile 1");
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
        
        _browser = new ChromeDriver(opt);
        _manager = new NetworkManager(_browser);
        //_monitor = new JavaScriptEngine(_browser);
    }

    public override async Task Start()
    {
        //testing
        _browser.Navigate().GoToUrl("https://www.instagram.com/direct/t/104910557574628/");

        //normal
        //browser.Navigate().GoToUrl("https://www.instagram.com/direct/t/259648092797288/");
        //browser.Navigate().GoToUrl("https://www.instagram.com");
        
        _manager.NetworkRequestSent += (sender, e) => _ = HandleRequest(sender!, e);
        
        var monitorService = _manager.StartMonitoring();
        var listenService = Task.Run(() => ListenForText());
        
        _browser.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);
        
        await monitorService;
        await listenService;
    }

    private async Task HandleRequest(object sender, NetworkRequestSentEventArgs e)
    {
        if (e.RequestUrl.Contains("cdn.fbsbx.com") && e.RequestUrl != _lastVNlink)
        {
            Console.WriteLine("I THIINK ITS A VOICE NOTE");
            _lastVNlink = e.RequestUrl;

            Console.WriteLine("Doing stuff");
            if (DoNextVoiceMessage)
                try
                {
                    var voiceNote = _browser
                        .FindElements(
                            By.XPath("//div[@aria-label='Double tap to like']//div[contains(@style, 'clip-path:')]"))
                        .Last();
                    //identifies voice notes by the wavedform image to avoid StaleElementReference Exception
                    var uniqueWaveForm = voiceNote.GetAttribute("style");

                    MessageInfo transcribed =
                        await new AudioFileHandler(FilePath).ProcessDownloadUrl(e.RequestUrl, "mp4");
                    Console.WriteLine($"Hellooo {transcribed.Duration} {transcribed.Message}");
                    SendMessage(transcribed, uniqueWaveForm);
                }
                catch (Exception except)
                {
                    Console.WriteLine(except);
                    Console.WriteLine("im a failure uwu");
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
                // Console.WriteLine("aaaah");
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
                    Console.WriteLine("Well now I am not doing it :( (voice notes)");
                    DoNextVoiceMessage = false;
                }
                else if (lowerText == "dontdonttldr" || lowerText == "/dontdonttldr")
                {
                    Console.WriteLine("Im gonna do the next voice note yippee");
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

    private void SendMessage(MessageInfo message, string uniqueWaveForm)
    {
        
        var voiceNote =
            _browser.FindElement(By.XPath($"//div[@aria-label='Double tap to like']//div[@style='{uniqueWaveForm}']"));

        if (Logging)
        {
            var parentElement = _browser.FindElement(By.XPath($"//div[@style='{uniqueWaveForm}']/ancestor::div[@aria-label='Double tap to like']"));
            var messageParentElement = parentElement.FindElement(By.XPath("../.."));
            message.Sender = messageParentElement.FindElement(By.XPath(".//a[@role='link']")).GetDomAttribute("href").TrimStart('/');
            
            Log(message);
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