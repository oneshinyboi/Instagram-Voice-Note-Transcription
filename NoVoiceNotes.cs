using OpenQA.Selenium.Chrome;
using OpenQA.Selenium;
using Google.Cloud.Speech.V1;

using OpenQA.Selenium.Interactions;
using Google.Protobuf.Collections;

public class messageBuff
{
    public string message {get; set;}
    public string uniqueWaveform {get; set;}
}
public class NoVoiceNotes
{
    public static ChromeDriver browser = new ChromeDriver();
    public static List<messageBuff> messageBuffer = new List<messageBuff>();
    public static NetworkManager manager;
    public static RecognitionConfig config = new RecognitionConfig {
        Encoding = RecognitionConfig.Types.AudioEncoding.Mp3,
        SampleRateHertz = 48000,
        LanguageCode = LanguageCodes.English.UnitedStates,
    };
    public static bool doNextVoiceMessage = true;
    
    public static SpeechClient client = SpeechClient.Create();
    public static string lastVNlink = "";
    public static IJavaScriptEngine monitor;
    public static IWebElement messageWindow;
    public static string lastText = "";


    public static async Task Main(string[] args)
    {
        //Console.WriteLine(AppContext.BaseDirectory);
        if (args.Length != 2)
        {
            Console.WriteLine("Too little/many arguments, try again");
            Environment.Exit(0);
        }
        
        ChromeOptions opt = new ChromeOptions();
        opt.AddArgument("disable-extensions");
        opt.AddArgument($"user-data-dir={args[0]}");
        opt.AddLocalStatePreference("browser", new {enabled_lab_experiments = new string[] { "profile.managed_default_content_settings.images@2" }});
        browser = new ChromeDriver(opt);
        manager = new NetworkManager(browser);
        monitor = new JavaScriptEngine(browser);

        config.AlternativeLanguageCodes.Add(LanguageCodes.Spanish.Spain);
        config.AlternativeLanguageCodes.Add(LanguageCodes.French.France);

        Startup(args[1]);
        Action messageCheck = () => checkForMessages();
        Task.Factory.StartNew(messageCheck);

        manager.NetworkRequestSent += handleRequest;
        manager.StartMonitoring();
        
        
        
        while (true)
        {
            
            //Console.WriteLine("monitoring finished");
            
            //send text/ set donttldr when response recieved
            for (int i = 0; i < messageBuffer.Count; i++)
            {
                sendMessage(messageBuffer[i].message, messageBuffer[i].uniqueWaveform);
                messageBuffer.Remove(messageBuffer[i]);
            }

        }
        

        

    }
    public static void Startup(string address)
    {

        browser.Navigate().GoToUrl(address);
        
        browser.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);


            
    }
    public static void checkForMessages()
    {
        while (true)
        {
            try
            {
                var message = browser.FindElements(By.XPath("//div[@aria-label='Double tap to like']")).Last().FindElement(By.XPath(".//div[@dir='auto']"));
                if (message.Text != lastText)
                {
                    Console.WriteLine($"message text: {message.Text}");
                    lastText= message.Text;
                }
                
                var lowerText = message.Text.ToLower();
                if (lowerText == "donttldr" || lowerText == "/donttldr") {
                    Console.WriteLine("set doNextVoiceMessage to false");
                    doNextVoiceMessage = false;
                } else if (lowerText == "dontdonttldr" || lowerText == "/dontdonttldr")
                {
                    Console.WriteLine("set doNextVoiceMessage to true");
                    doNextVoiceMessage = true;
                }
            } catch {}
            
        }
    }
    public static void sendMessage(string message, string uniqueWaveForm)
    {
        var voiceNote = browser.FindElement(By.XPath($"//div[@aria-label='Double tap to like']//div[@style='{uniqueWaveForm}']"));
        var messageBox = browser.FindElement(By.XPath("//div[contains(text(),'Message...')]"));
        new Actions(browser)
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

    public static void handleRequest(object sender, NetworkRequestSentEventArgs e)
    {
        if (e.RequestUrl.Contains("cdn.fbsbx.com") && e.RequestUrl != lastVNlink)
        {
            
            try
            {                
            lastVNlink = e.RequestUrl;
            Task.Factory.StartNew(() => {
                
                if (doNextVoiceMessage)
                {
                    Console.WriteLine("executing voice to text");
                    var voiceNote = browser.FindElements(By.XPath("//div[@aria-label='Double tap to like']//div[contains(@style, 'clip-path:')]")).Last();
                    
                    
                    //identifies voice notes by the wavedform image to avoid StaleElementReference Exception
                    var uniqueWaveform = voiceNote.GetAttribute("style");
                    //Console.WriteLine(uniqueWaveform);
                    Console.WriteLine(e.RequestUrl);

                    Console.WriteLine("starting audio stuff");
                    RecognitionAudio audio = RecognitionAudio.FetchFromUri(e.RequestUrl);
                    
                    RecognizeResponse response = client.Recognize(config, audio);
                    Console.WriteLine(response);
                    var message = "";
                    for (int i = 0; i < response.Results.Count; i++)
                    {
                        message += response.Results[i].Alternatives[0].Transcript;
                    }
                    var mbuff = new messageBuff() {message = message, uniqueWaveform = uniqueWaveform};
                    messageBuffer.Add(mbuff);
                    Console.WriteLine("finished audio stuff");
                }
                else
                {
                    doNextVoiceMessage = true;
                    Console.WriteLine("donextVoiceMessage set to true");
                }
                
                
            });
            }
            catch (Exception except) {Console.WriteLine(except);}
        }
    }

}

