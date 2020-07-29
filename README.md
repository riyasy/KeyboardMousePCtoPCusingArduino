# Software + Hardware solution for remote desktop for offline PC.

There can be PCs which are connected to specific VPNs which are not in normal network. Similarly there can be PCs which are kept offline for specific testing purposes. This repository proposes a solution for remote desktop to these offline PCs. This can help in work from home on these offline PCs and also test automation.

![alt text](https://github.com/riyasy/RemoteDesktopForOfflinePC/blob/master/Misc/Architecture.jpg?raw=true)


1. Monitor output from PC2 and PC1. There are various hardware encoder devices available for this purpose. These devices essentials have an input VGA or HDMI port which takes the VGA/HDMI input and encodes it and sends it over a network or internet.

2. Keyboard/mouse events from PC1 to PC2.
In this solution a custom software (available in EventCapturerForWindows folder) in PC1 captures all the mouse and keyboard events in PC1 and sends it to an arduino over serial communication. If no serial port is available, USB to serial converter can be used.
Arduino micro, leanardo etc have capability to present themselves as HID devices to PCs. This featue can be used to transfer the received keyboard mouse information as clicks, mouse moves and keyboard presses to PC2. Arduino code is present in ArduinoHIDsimulator folder.

In my case, I had an arduino UNO which doesnot have these feature.
So I installed Hoodloader1 from https://github.com/NicoHood/Hoodloader
This enables the HID feature for arduino UNO.
I used the HID lbirary from https://github.com/NicoHood/HID. (the last version compatible with Hoodloader1.
Used Arduino IDE (ther verison around 2015 compatible with the HID library.


TODO
Only Mouse part is implemented.
Keyboard relaying needs to be done
