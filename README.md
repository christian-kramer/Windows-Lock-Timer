# Windows-Lock-Timer
A utility for forcefully locking windows accounts after a set period of time.
___

## Installation:

1. Install the .NET Core Runtime (or SDK) [here](https://dotnet.microsoft.com/download)
2. Click [here](https://github.com/christian-kramer/Windows-Lock-Timer/raw/master/WindowsLockTimer.zip) to download the latest build .zip archive
2. Open the .zip archive
3. Drag the `WindowsLockTimer` folder into `C:\Program Files` or similar

## Command Line Arguments

<pre>
-t			number of minutes until computer will lock
-w			number of minutes prior to locking the warning will display
-m			the text of the warning message to display
</pre>

Example:

`C:\Program Files\WindowsLockTimer\WindowsLockTimer.exe -t 60 -w 2 -m "Sample Text"`

This will create a 60-minute timer, with a 2-minute warning that says "Sample Text".
