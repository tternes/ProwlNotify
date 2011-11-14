# ProwlNotify
A simple .NET utility for posting notifications to [Prowl][prowlurl]. Commonly used at the end of long build scripts or for scheduled reminders. Prowl API key can be provided on command line, or stored in the registry to simplify calls.

## Usage
    Usage: ProwlNotify [OPTIONS] [ARGS]
       ARGS (required):
       -a app          Application name
       -e event        Event (e.g. "Build Complete")
       -d description  A description, generally terse
    
      OPTIONS:
       -k key          Prowl API key; if not specified, use registry key
       -p priority     -2, -1, 0, 1, 2 (very low, moderate, normal, high, emergency)

## Example

    ~ ProwlNotify.exe -a "My Cool Project" -e "Build Complete" -d "Build completed on %COMPUTERNAME%"
    
    Successfully posted notification

## Registry Key
To avoid having to specify the Prowl API key each time, you can set the following key in the Windows registry:

    HKEY_CURRENT_USER\Software\Bluetoo\ProwlNotify ProwlApiKey = "keyhere"

## License
This software is released into public domain.


[prowlurl]: http://www.prowlapp.com/

