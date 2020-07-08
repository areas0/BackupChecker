# BackupChecker
Tool via network (or local) that does an integrity check for backups in C# using SHA1 hashes. 
.Net Core (latest version) is required!
# Features
Tool available under V0.6 with basic features:
- Find the sha256 of a file
- Generate all files' sha256 in a folder and export it to a file (.json)
- Do a remote checkup between computers over network using Tcp networking
- Check backup from 2 .json files from already exported folders in case remote failed
- A full logger
# BUGS
It is buggy? Yes, sure, but most of the time it should work. Open an issue if needed, and don't hesitate to fix it yourself!

# Why
- Because I had time to spare
- Because there was no such tool available online to my knowledge
- It allows to do a safe checkup on files on two distant computers and make sure that they have the same files without sending files over network

Created by Areas0 ~ 07/07/2020
