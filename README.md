# Neptun
 
## Neptunhoz egy könnyebben használható, jobb teljesítményű program, már amennyire ez lehetséges. (a cél atleast..)

***Telepítő az Installer mappában található 'setup.exe'. Automatikusan frissíti a programot.***
# Warning! A jelszó bármiféle titkosítás nélkül van tárolva. Magadra vess, ha olyan gépen használod, ami esetleg más kezébe kerülhet.

**Jelenleg csak Windowson működik, más OSen talán valahogy megoldható a futtatás, de nem garancia, hogy stabil lesz. (Windowson sem eléggé az..)**  
ELTE Neptunnal kipróbálva, (IK/TTK TO adatbázisból is tölt be dolgokat, majd javítva lesz, hogy más karon ne tegye ezt), más egyetem Neptun verziójával nem biztos, hogy működik.  
Telepítés és indítás után feladatkezelő, folyamatok, Neptun jobb klikk, tulajdonságok, helynél írt elérési úton az 'appsettings.json' fájlban lehet átírni, hogy melyik egyetem Neptun szerveréhez csatlakozzon.
```json    
    "NeptunServer": {
        "HostUrl": "https://hallgato.neptun.elte.hu/" 
    }
```
```json "HostUrl": "https://hallgato.neptun.elte.hu/" ```  
**Ezt kell átírni. Pl: "HostUrl": "https://web8.neptun.u-szeged.hu/hallgato/"**


C#-al és WPF-el lett írva, aki esetleg elég elvetemült, hogy akár csak egy kis részében besegítsen, legyen az egy kis UI or anything, please do it.
Később szándékozom több nyelvre átfordítani, de csoda lesz, ha valaha elkészül. Inkább egy jobb rendszer kéne ennél a szaros Neptunnál.
