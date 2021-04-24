# Gatekeeper

<img src="https://cdn.discordapp.com/app-icons/777189946930888715/6d3266cc4060dd0d0f7bf33e72f3be99.png" align="right" alt="Gatekeeper Logo" width="128" height="128">

Gatekeeper is the Discord Bot for the FModel Server used to verify user integrity by asking for a one time code that cannot be pasted. It works that way to avoid spam bots and use invisible characters to counter string replacement for selfbots.

## Table of Contents

- [Setup](#setup)
- [Commands](#commands)

## Setup

Your bot must have both `PRESENCE INTENT` and `SERVER MEMBERS INTENT` enabed from https://discord.com/developers/applications/{botId}/bot in order to not freeze at launch.
The setup is pretty simple, build the project using .NET Core 3.1. If no configuration file found, it will guide you through the process of making one.
What you need is :

1. `Bot Token` *cannot run without it*
2. `Bot Prefix`
3. `Server Id` *the id of the server the bot is in*
4. `Role Id` *the id of the role to give after sending a correct code*
5. `Verification Channel Id` *the id of the channel where codes will be sent*
6. `Log Channel Id` *the id of the channel where verified users will be logged*

## Commands

All commands should always be prefixed.

1. `generate` *will send the verification message in the channel (this include update of the Verification Channel Id and generation of a new code)*
2. `code` *will send the current code to enter*
