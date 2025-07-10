# Development Container Template

This project creates an API wrapper for the Personal Voice service in Azure.

```bash

az login -t $TENANT_ID

mkdir src
cd src

dotnet new webapi -n PersonalVoiceApi

azd init

azd up

# Don't forget to assign the managed identity to the TTS endpoint

```
