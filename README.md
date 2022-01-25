# SMTC_SongInfo

This tool retrieves the Information that the System Media Transport Control (SMTC) provides. For example the name of the song or the artist to display it in OBS.

![Alt text](https://docs.microsoft.com/en-us/uwp/api/windows.media/images/smtc.png?view=winrt-22000 "a title")

More information about the SMTC can be found here https://docs.microsoft.com/en-us/uwp/api/Windows.Media.SystemMediaTransportControls?view=winrt-22000.

# Usage

On startup the tool searches for any active Global SMT Control. On success it safes the information in a .txt file in the same directory as the .exe, if an image is provided it gets saved in the same location.
This can then be used to display the information or the image on your stream or video. But this only works if the program that plays the music also creates a SMT Control.

# Projects

The C# solution containst two projects, SMTC_SongInfo retrieves every the information of the SMT Control two seconds and updates the file. This is yet the better version of these two because it will not miss changes of the SMT Control even if a other program creates a new one. The project SMTC_SongInfoNew tries to acomplish the same results but by listening to the events that the SMT Control provides. But sometimes the events on a song change are fired multiple times and it can hapen that the program misses the changes.
