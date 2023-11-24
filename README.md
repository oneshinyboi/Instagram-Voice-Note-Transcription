# Instagram-Voice-Note-Transcription

<img src="/assets/example.png" align="right"/>
A python script for linux that automatically transcribes any voice note, in an instagram direct message or group chat, then sends it as a message to that dm/group chat.

If the "/donttldr" message is sent, then the next voice message will be ignored.  <br />

<br /><br /><br /><br /><br /><br />
  
# Dependencies
The following python modules: `pip install selenium speechRecognition`.

Google chrome and the chrome webdriver, which should be `/usr/bin/chromedriver`.

pipewire audio system (linux only), specifically access to the command `pw-record`.

# Steps
1. Set up an instagram account for the bot to use
2. Find your `object.serial` for the audio device you will use with the command `pw-cli list-objects`. It should be a monitor of an audio port, for example:
```sh
id 44, type PipeWire:Interface:Port/3
 		object.serial = "44"
 		object.path = "auto_null:monitor_1"
 		format.dsp = "32 bit float mono audio"
 		node.id = "36"
 		audio.channel = "FR"
 		port.id = "1"
 		port.name = "monitor_FR"
 		port.direction = "out"
 		port.monitor = "true"
 		port.alias = "Dummy Output:monitor_FR"
```
3. Then run the script with
   `python IHateVoiceNotes.py [link to instagram dm/group chat] [username of instagram account] [password of instagram account] [object serial from step 2]`

# TroubleShooting
If you send a voice note and the bot does not respond it is likely your object `object.serial` is wrong. try and record an audio source with it using the `pw-record --target [object.serial] [output file]` commmand.
