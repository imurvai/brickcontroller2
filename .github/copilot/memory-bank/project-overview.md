# Project overview

## What this app is

BrickController2 is a .NET MAUI application for controlling Lego-compatible creations with a Bluetooth gamepad.

## Current supported app targets

- Android
- iOS

## Current desktop objective

Add a macOS desktop build via **Mac Catalyst** that is practically usable, not just launchable.

Required phase-1 capabilities:

- desktop app starts on this Mac
- Bluetooth LE features work
- game controller input works

## High-level structure

- `BrickController2/BrickController2/`
  - MAUI `App`
  - XAML pages and controls
  - view models
  - shared business logic
  - database layer
  - device management
  - platform-service interfaces
- `BrickController2/BrickController2.iOS/`
  - Apple app bootstrap
  - Apple platform services
  - Apple UI handlers/renderers
- `BrickController2/BrickController2.Android/`
  - Android app bootstrap
  - Android platform services
  - Android UI handlers

## Key design fact

The shared MAUI app already contains most of the UI and logic. The desktop port is mainly about adding a Mac Catalyst app target and providing correct Apple desktop-compatible platform services.
