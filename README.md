# Instagram-Voice-Note-Transcription

<img src="/assets/example.png" align="right"/>
A C# script that automatically transcribes any voice note, in an instagram direct message or group chat, then sends it as a message to that dm/group chat.

If the "/donttldr" message is sent, then the next voice message will be ignored.  <br />

<br /><br /><br /><br /><br /><br />
  
# Dependencies
The following nuget packages: `Google.Cloud.Speech.V1` and `Selenium.WebDriver`.

Google chrome and the chrome webdriver, which should be `/usr/bin/chromedriver`.


# Steps
1. Set up an instagram account for the bot to use
2. Add a new chrome profile for the bot and sign into the instagram account with it
3. setup a google cloud project for google speech https://cloud.google.com/speech-to-text/docs/before-you-begin
4. set the environment variable GOOGLE_APPLICATION_CREDENTIALS to the path of your google cloud credentials .json
5. Then run the script with
   `dotnet run [chrome profile directory eg /home/Diamond/.config/google-chrome/] [link to dm/group chat]`

