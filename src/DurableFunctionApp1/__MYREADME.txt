* HTTP Trigger:
http://localhost:7153/api/Function_HttpStart

* probablement démarrer le node local:
avec la notification icone du Cluster Manager (une sorte de pentagone avec des ronds aux angles, orange foncé tendance marron) : Start Local Cluster.
(voir Cluster Manager.png)

* Démarrer Azurite:
C:\Windows\system32>"J:\Program Files\Microsoft Visual Studio\2022\Community\Common7\IDE\Extensions\Microsoft\Azure Storage Emulator\azurite.exe" start
=>
Azurite Blob service is starting at http://127.0.0.1:10000
Azurite Blob service is successfully listening at http://127.0.0.1:10000
Azurite Queue service is starting at http://127.0.0.1:10001
Azurite Queue service is successfully listening at http://127.0.0.1:10001
Azurite Table service is starting at http://127.0.0.1:10002
Azurite Table service is successfully listening at http://127.0.0.1:10002



    "version": "2.0",
    "logging": {
        "logLevel": {
            "default": "Information"
        },
        "applicationInsights": {
            "samplingSettings": {
                "isEnabled": true,
                "excludedTypes": "Request"
            }
        }
    }
