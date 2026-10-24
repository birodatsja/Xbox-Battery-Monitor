# Xbox-Battery-Monitor
Montor Battery on Xbox Controller

What’s new:

Bluetooth: XInput often can’t read the battery over Bluetooth, so the app now reads the controller’s standard Bluetooth battery service first. If that works you get an exact percentage like “72%”. If not, it falls back to the four-level XInput reading.
Tray icon: a battery icon that changes color and fill with the level, with the percentage in the tooltip. Closing or minimizing the window sends it to the tray. Right-click the icon for Open, Start with Windows, and Exit.
Low-battery notification: a Windows notification at 20% or below (or “Low” on the XInput fallback). It fires once per controller and resets after the battery recovers or the controller disconnects.
Start with Windows: a checkbox in the window and in the tray menu. It launches the app minimized to the tray. Put the xbox battery folder where you want to keep it before turning this on, because the startup entry points to the exe’s current location.
