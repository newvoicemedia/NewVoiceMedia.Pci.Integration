# Changelog
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/en/1.0.0/)
and this project adheres to [Semantic Versioning](http://semver.org/spec/v2.0.0.html).

## [1.0.1] - 2017-11-17
### Fixed
- Reporting client-side exception in Windows Command Prompt no longer triggers popup window
- "Accept" header is only added once to API requests

## [1.0.0] - 2017-10-13
### Added
- Generation of XML RSA key pairs
- Checking whether given keys match each other as private-public pair
- Signing arbitrary data with given private key
- Checking whether data and signature match given public key
- Conversion of RSA keys from PEM to XML
- Sending API requests to NewVoiceMedia PCI service
- Checking external IP of current machine (for whitelisting)
- Reflection based CLI with generated help section
