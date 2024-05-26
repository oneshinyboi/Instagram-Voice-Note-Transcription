using System.Collections.Concurrent;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Interactions;

namespace VoiceNoteTranscription.Clients;

public class InstagramClient : Client
{
    public static ConcurrentDictionary<string, string> MessageBuffer;
    private readonly ChromeDriver _browser;
    private readonly NetworkManager _manager;
    private readonly ChromeOptions _opt;

    private string _lastVNlink = "";
    private IWebElement _messageWindow;
    private IJavaScriptEngine _monitor;

    public InstagramClient(string filepath) : base(filepath)
    {
        Console.WriteLine(AppContext.BaseDirectory);

        _opt = new ChromeOptions();
        _browser = new ChromeDriver();
        MessageBuffer = new ConcurrentDictionary<string, string>();

        _opt.AddArgument("disable-extensions");
        _opt.AddArgument("user-data-dir=/home/Diamond/.config/google-chrome/");
        _opt.AddLocalStatePreference("browser",
            new { enabled_lab_experiments = new[] { "profile.managed_default_content_settings.images@2" } });
        _browser = new ChromeDriver(_opt);
        _manager = new NetworkManager(_browser);
        _monitor = new JavaScriptEngine(_browser);
    }

    public override async Task Start()
    {
        //testing
        //browser.Navigate().GoToUrl("https://www.instagram.com/direct/t/104910557574628/");

        //normal
        _browser.Navigate().GoToUrl("https://www.instagram.com/direct/t/259648092797288/");
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

                    var transcribed =
                        await new AudioFileHandler(FilePath).ProcessDownloadUrl(e.RequestUrl, "Instagram", "aac");
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

    private void SendMessage(string message, string uniqueWaveForm)
    {
        var voiceNote =
            _browser.FindElement(By.XPath($"//div[@aria-label='Double tap to like']//div[@style='{uniqueWaveForm}']"));
        var messageBox = _browser.FindElement(By.XPath("//div[contains(text(),'Message...')]"));
        new Actions(_browser)
            .MoveToElement(voiceNote)
            .MoveByOffset(180, 0)
            .Click()
            .MoveToElement(messageBox)
            .Click()
            .SendKeys(message)
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