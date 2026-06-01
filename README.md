# Sayo Device Reader

## Disclaimer

This is not guarnteed to work on every sayodevice type, this has only been tested on the SayoDevice CM51+ Firmware 1.4.12.
This repository is not affiliated with Saybot Technology. 

## Example of the reader working.

![Example of the SayoDevice reader running SayoCat on a Sayodevice CM51+](/ExampleImages/Sayocat.gif)

## What is a Sayo Device?

A SayoDevice is a keypad sold [here.](https://sayodevices.com/) They often come with magnetic switches, which this program is designed to read the analog signal from.

## Why create this project?

Unfortunately the SayoDevice uses a web based driver found [here](https://sayodevice.com/), their is no documentation on how to read these analog signals and once this web app ineviteably dissapears you can no longer read the signals.
Fortunately, I was able to reverse engineer the HID packets being sent back and forth between the device and driver. This program will never stop working unless a firmware change breaks it.

