keytool -genkey -keystore rootNavKeyStore -alias rootNav -validity 365
#rootnav
jarsigner -keystore rootNavKeyStore RootNavInterface.jar rootNav