![Build pre-check](https://github.com/areas0/BackupChecker/workflows/Build%20pre-check/badge.svg)
# BackupChecker
Tool via network (or local) that does an integrity check for backups in C# using SHA256 hashes. 
.Net Core (latest version) is required! Made for windows, can be adapted to fit linux environment.
# Features
Tool available under V0.7 with basic features:
- Find the sha256 of a file
- Generate all files' sha256 in a folder and export it to a file (.json)
- Do a remote checkup between computers over network using Tcp networking: check if any file is missing and then check the data inside
- Check backup from 2 .json files from already exported folders in case remote failed
- A full logger
# BUGS
It is buggy? It might be, sure, but most of the time it should work. Open an issue if needed with the log file (last.log), and don't hesitate to fix it yourself!

# Why
- Because I had time to spare
- Because there was no such tool available online to my knowledge
- It allows to do a safe checkup on files on two distant computers and make sure that they have the same files without sending files over network

Created by Areas0 ~ 07/07/2020
