# TextToSpeech
Easily Pass Strings to Microsoft Text To Speech Bots. Ideal for Virtual Cable and passing the speech to a voice chat program (Skype, Discord, etc.) Can be easily minimized and maximized with LCntrl + Space

Note that to use the program with voice chat systems, you'll need to install a Virtual Audio Cable. I recommend the free software from https://www.vb-audio.com/Cable/

After you have the Audio Cable installed, unzip TextToSpeech and install it. Then make certain that you choose the Virtual Cable as the input in your desired voice chat system. After that, you should be good to go to use TextToSpeech.

Eventually, I hope to write my own virtual microphone driver so that the extra step isn't needed, but for now it works.

Uses NAudio (https://github.com/naudio/NAudio) and low level keyboard hooking implementation from Dylan at http://www.dylansweb.com/2014/10/low-level-global-keyboard-hook-sink-in-c-net/


