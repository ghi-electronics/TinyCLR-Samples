FEZ Wi-Fi
--------

This sample shows using the SPWF04Sx Wi-Fi module on the FEZ. It does use TLS so you will need to download the root certificate for the site you're connecting to (https://www.ghielectronics.com in this case). You can do this by navigating to the site in your browser and then using the browser's export to file function. Make sure to export in DER format (which commonly has a crt or cer extension on Windows) and make sure to get the root (or top-level) certificate, not the one for the site itself. You'll then want to save a copy of the certificate to the Resources folder of the project with the name `DigiCertGlobalRootCA.crt`.

Also make sure to add your own SSID and password for your Wi-Fi network.

You'll notice several calls to `WaitForButton` throughout the sample. This is to give you an opportunity to wait for the desired events to happen before proceeding. For example, before proceeding with the network request you need to wait for the event saying you've received an IP and NTP time.

You only need to set the TLS cert and SSID once, the module will remember it in its own flash. This can make startup a bit quicker.