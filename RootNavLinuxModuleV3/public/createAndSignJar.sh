keytool -genkey -keystore rootNavKeyStore -alias rootNav
#rootnav
jarsigner -keystore rootNavKeyStore RootNavInterface.jar rootNav