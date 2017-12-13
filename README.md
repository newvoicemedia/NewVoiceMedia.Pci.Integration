# NewVoiceMedia PCI Integration

## Introduction
This project is created to help NewVoiceMedia customers integrate with payment api (PCI service). 
Can be used both as command line utility, as a library or as reference for own implementation. 

## Usage

    NewVoiceMedia.Pci.Integration <command>

Available commands (from public API `NewVoiceMedia.Pci.Integration.Utils`):

    Command          Description
    generate-keys    Writes new generated key pair to given paths
    check-keys       Checks whether given xml keys match each other (true) or not (false)
    sign-data        Signs given string with given private key and returns base64 encoded signature
    verify-signature Checks whether given signature matches given pair of data and public key (true) or not (false)
    convert-key      Convert RSA key from PEM to XML
    send-request     Sends payment request to server and returns response
    relay-request    Relays payment request from standard input to server and returns response
    check-ip         Checks external IP address of current machine
    help             Shows this help text

To see list of parameters for each command, run `NewVoiceMedia.Pci.Integration help` (or check file [Utils.cs](src/Utils.cs)). 
Parameters shown in `< >` are mandatory and have to be provided. Parameters in `[ ]` are optional - if omitted, default value shown after `=` is used. 

Download [NewVoiceMedia.Pci.Integration.zip](https://github.com/newvoicemedia/NewVoiceMedia.Pci.Integration/releases/download/v1.01/NewVoiceMedia.Pci.Integration.zip), unpack it and run the exe file from `Command Prompt`. For platforms different than 64bit Windows you need to use source files instead (see section [Build](#build)).

Example usages:

    NewVoiceMedia.Pci.Integration convert-key public.pem
    C:\_work\nvm\NewVoiceMedia.Pci.Integration.exe send-request agol4ebio20 SagePay ..\requests\payload-1.json "payments1.nvminternal.net"

In the first example, current directory is the one containing files `NewVoiceMedia.Pci.Integration.exe` and `public.pem`. 
In the second example, we passed paths (absolute and relative) to reference files from different directories.

## API documentation
Command `send-request` facilitates access to API, taking care for things such as cryptographic signatures and polling for final result. 
To make API requests, you need to provide payment request serialized as XML or JSON. General API documentation can be found on [our wiki](https://newvoicemedia.atlassian.net/wiki/spaces/DP/pages/15172092/Mid-Call+IVR+API+Specification). 
Content of request depends on chosen payment gateway. Details can be found here:
 - [Realex](https://newvoicemedia.atlassian.net/wiki/spaces/DP/pages/99354891/Taking+payments+via+Realex)
 - [SagePay](https://newvoicemedia.atlassian.net/wiki/spaces/DP/pages/72679688/Taking+payments+via+SagePay)
 - [SmartPay](https://newvoicemedia.atlassian.net/wiki/spaces/DP/pages/205193810/Taking+payments+via+SmartPay)
 - [WorldPay](https://newvoicemedia.atlassian.net/wiki/spaces/DP/pages/72679689/Taking+payments+via+WorldPay)
 - [Worldpay Online](https://newvoicemedia.atlassian.net/wiki/spaces/DP/pages/234288808/Taking+payments+via+Worldpay+Online)

## Build
To build application by yourself, you need `.NET Core 2.0+` installed (it is not required for running it).
On 64 bit Windows, execute `build.cmd` (no parameters needed). Application will be built to directory `/publish`. 
On other platforms, copy its content, change parameter `--runtime` and execute it in your shell. Available platforms:
 - win-x86
 - win-x64
 - linux-x64
 - osx-x64
 - android
