@echo off
start "" powershell -NoProfile -ExecutionPolicy Bypass -STA -WindowStyle Hidden -File "%~dp0kur-abxpilot.ps1" %*
