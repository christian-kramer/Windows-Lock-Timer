# Windows-Lock-Timer
A utility for forcefully locking windows accounts after a set period of time.
___

## Installation

1. Install the .NET Core Runtime (or SDK) [here](https://dotnet.microsoft.com/download)
2. Click [here](https://github.com/christian-kramer/Windows-Lock-Timer/raw/master/WindowsLockTimer.zip) to download the latest build .zip archive
2. Open the .zip archive
3. Drag the `WindowsLockTimer` folder into `C:\Program Files` or similar

## Command Line Arguments

<pre>
-t			number of minutes until computer will lock
-w			number of minutes prior to locking the warning will display
-c			number of minutes for the cooldown between sessions
-m			the text of the warning message to display
</pre>

Example:

`C:\Program Files\WindowsLockTimer\WindowsLockTimer.exe -t 60 -w 2 -c 15 -m "Sample Text"`

This will create a 60-minute timer, with a 2-minute warning that says "Sample Text", and a 15-minute cooldown where the user should go do something else.

## Notes

* It's important to consider that WindowsLockTimer.exe must be ran *as* the account you want to lock. For this reason, it's helpful to disable task manager in group policy to prevent the user from ending the task.
* When setting up a scheduled task to run WindowsLockTimer.exe when a user logs in, it's important to select "Run only when user is logged on". This ensures the account will lock properly.
