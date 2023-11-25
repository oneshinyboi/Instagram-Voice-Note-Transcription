from selenium import webdriver
from selenium.webdriver.common.by import By
from selenium.webdriver.common.keys import Keys
from selenium.webdriver.chrome.options import Options
from selenium.webdriver.chrome.service import Service
from selenium.webdriver.common import action_chains


import time
import os
import speech_recognition as sr
import sys

audioFilePath = os.path.abspath("curr.wav")

#opening and closing to ensure file is created
f = open(audioFilePath,"a")
f.close()

doNextVoiceMessage = True
r = sr.Recognizer()

options = Options()
#options.add_argument("--headless=new")
service = Service(executable_path='/usr/bin/chromedriver')
browser = webdriver.Chrome(service=service)
#load webpage
browser.get(sys.argv[1])



browser.implicitly_wait(10)

def SendMessage(message, voiceNote):
    
    action = webdriver.ActionChains(browser)
    action.move_to_element_with_offset(voiceNote.location, 180, 0)
    action.click()
    messageBox = browser.find_element(By.XPATH, "//div[contains(text(),'Message...')]")
    action.move_to_element(messageBox)
    action.click()
    action.send_keys(message, Keys.RETURN)
    action.perform()

#login page
username = browser.find_element(By.NAME, "username")
username.click()
username.send_keys(sys.argv[2])
password = browser.find_element(By.NAME, "password")
password.click()
password.send_keys(sys.argv[3])
password.send_keys(Keys.RETURN)

#saveLogin info and notification page
notNow = browser.find_element(By.XPATH, "//div[text()='Not Now']")
notNow.click()
time.sleep(2)
notNow = browser.find_element(By.XPATH, "//button[text()='Not Now']")
notNow.click()

#detect audio message loop
prevMessages = browser.find_elements(By.XPATH, "//div[@aria-label='Double tap to like']")


print("starting mmessage detection loop")
while True:
    try:
        time.sleep(0.5)
        Messages = browser.find_elements(By.XPATH, "//div[@aria-label='Double tap to like']")
        newMessages = [item for item in Messages if item not in prevMessages]

        
        #for i in range(len(newMessages)):
        for i in range(len(newMessages),0, -1):
            print(Messages[-i].location)
            print(Messages[-i])
            
            #if message is close to the left, it is incoming and we need to process it
            if Messages[-i].location['x'] == 522:
                print("message detected")
            
                #if the element is a text message
                textMessage = Messages[-i].find_elements(By.XPATH, ".//div[@dir='auto']")
                if len(textMessage) != 0:
                    
                    print("text message is: "+str(textMessage[0].text))
                    if textMessage[0].text.lower() == "/donttldr":
                        doNextVoiceMessage = False
                
                #if element is a voice message
                if len(audioButton := Messages[-i].find_elements(By.XPATH, ".//div[@aria-label='Play']")) != 0:
                    print("transcribing voice note...")
                    if not doNextVoiceMessage:
                        doNextVoiceMessage = True
                    else:
                        voiceNoteTime = audioButton[0].find_element(By.XPATH, "./../../div[contains(text(),':')]").text
                        voiceNoteTime = voiceNoteTime.split(":")
                        voiceNoteTime = int(voiceNoteTime[0])*60 + int(voiceNoteTime[1])
                        audioButton[0].click()
                        os.system("pw-record --target "+str(sys.argv[4])+" $PWD/curr.wav & sleep "+str(voiceNoteTime)+"s ; kill $!")
                        time.sleep(1)
                        
                        file=sr.AudioFile(audioFilePath)
                        with file as source:
                            audio = r.record(source)
                        try:
                            s = r.recognize_google(audio)
                            SendMessage(str(s), Messages[-i])
                            print(s)
                        except Exception as e:
                            print(e)
    except Exception as e:
        print(e)
    prevMessages = Messages
       

    
    
        
