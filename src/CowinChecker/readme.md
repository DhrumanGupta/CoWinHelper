
# CoWinChecker
A bot made to get you vaccine alerts on whatsapp!

## How to configure
 - Add a config.json file
 - The config.json file has the following format:

```
{
  "Interval": 6000,
  "PersonData": [
    {
      "Name": "VaccineAlerts",
      "Districts": [
        145
      ],
      "PinCodes": [],
      "MinimumSeats": 1,
      "CenterKeywords": [
        "(optional) center name to filter",
        "(optional) center name to filter 2"
      ],
      "VaccineType": ""
    }
  ]
}
```
- `Interval`: The time in ms the bot rests for after checking again. Set this to whatever you'd like, but a word of caution: do not set it below 4000, the  api  can start blocking you.
- `PersonData`: The data for the bot to run on. A list of groups the bot will check for.
	-  `Name`: The name of the whatsapp group to send the messages to
	- `Districts`: A list of district codes to check for (must have this or pincodes (can have both))
	 - `PinCodes`: A lit of pincodes to search for (must have this or districts (can have both))
	 - `MinimumSeats`: The minimum amount of available seats for it to send a message
	 - `VaccineType`: (Optional) The vaccine to check for. PLEASE ENSURE SPELLING IS RIGHT. (`covaxin`, `covishield`)

## Dependencies
Must have chromedriver in PATH variables or within the folder of `CowinChecker.exe`

## How to run
After configuring, run the `CowinChecker.exe` file. A chrome window will open up, where you will need to sign in with whatsapp (using the qr code shown). Once you log in, it will start working. Check the console for numbers. As long as you see numbers being printed (printed every time the bot checks), you are good to go. However, if the numbers stop appearing, and you only get a message: `Checked at ...`, your ip has been blocked. Simply open the `config.json` file and increase the interval. 