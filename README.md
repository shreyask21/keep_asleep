# Keep Asleep!
Fixes Windows 10/11's "Modern Standby" random wake up issues on Laptops.  
Also prevents system wake up if mouse gets bumped when the lid is closed.  

## Quick Installation
* [Download the exe from releases section](https://github.com/shreyask21/keep_asleep/releases/latest)
* Open Windows Explorer and paste the following address in the address bar at the top:  
   `%APPDATA%\Microsoft\Windows\Start Menu\Programs\Startup`
* Copy and paste the downloaded exe in the above folder

## Methodology
This app hooks into the windows API for lid sensor and keeps the track of the sensor   
If the laptop wakes up from sleep and the sensor says the lid is closed,  
this app will re trigger the modern standby to enter sleep by sending a system message.
