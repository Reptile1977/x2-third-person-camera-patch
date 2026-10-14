X2: The Threat — Third-Person Camera Patch
==========================================

ENGLISH
-------

This unofficial community patch addresses a crash when rotating the external
third-person ship inspection camera in the Steam build of X2: The Threat
1.5.0.0. The author confirmed the camera scenario worked in two checks after
installing the patch. This is not an Egosoft update.

The patcher is a Windows GUI executable that uses the .NET Framework included
with Windows 10/11. Players do not need Python or a separate download. It finds
the Steam installation automatically when possible,
checks that X2.exe is the supported original build, creates and verifies a
backup, then applies the patch. It does not include or redistribute the game's
original executable.

Installation
------------
1. Close X2: The Threat.
2. Run X2-Third-Person-Camera-Patch.exe.
3. Check the detected X2.exe path. If the game was not found, click Browse… and
   select X2.exe in the game's installation folder.
4. Click Apply patch. The patcher creates this backup next to X2.exe:
   X2.exe.backup-before-third-person-camera-patch
5. Launch X2.exe directly.

If Windows denies access to the game folder, close Steam and the game, then
right-click the patcher and choose Run as administrator.

To undo the patch, open the patcher and click Restore original. It also
recognizes the older backup name X2.exe.original-before-codex-overflow-fix.
Steam's Verify integrity of game files option also restores the original file.

Compatibility and file hashes
-----------------------------
Supported stock X2.exe SHA-256:
7951124FB1EB3598D9F3D2469357D39BBC5E745EFF21D6E2A115E315F9D265D8

Patched X2.exe SHA-256:
30C50BF37A67A1C88270C1ADE90B7C344F177236C35212755109A6B907DCBD76

Patcher SHA-256:
3454A52C35B85B8F97230B4B737D533574138FF8DD464C40D147EC5A4E5150FB

The patcher only accepts the supported original hash or reports that the patch
is already installed. If your file is another version or has been modified,
restore it through Steam and try again. The patcher is unsigned; Windows may
show an unknown-publisher or SmartScreen notice. Verify the SHA-256 and only
run a copy obtained from a source you trust.

The game source code and a full crash dump were not available. This is a
binary-level fix for the recorded integer-division overflow signature, not a
general fix for every X2 crash. Older reports with a different access-violation
signature at RVA 0x6D539 are outside its scope.


РУССКИЙ
-------

Неофициальный пользовательский патч исправляет вылет при вращении внешней
камеры осмотра корабля от третьего лица в Steam-версии X2: The Threat 1.5.0.0.
Автор проверил этот режим дважды после установки патча — камера работала без
вылета. Это не официальное обновление Egosoft.

Патчер — Windows-программа с графическим окном, использующая .NET Framework,
входящий в состав Windows 10/11. Python и отдельные загрузки игрокам не нужны.
Патчер по возможности сам находит установленную через Steam
игру, проверяет исходный X2.exe, создаёт и проверяет резервную копию, затем
устанавливает исправление. Оригинальный исполняемый файл игры в патчер не
включён и не распространяется.

Установка
---------
1. Закройте X2: The Threat.
2. Запустите X2-Third-Person-Camera-Patch.exe.
3. Проверьте путь к найденному X2.exe. Если игра не найдена, нажмите Browse… и
   укажите X2.exe в папке установки игры.
4. Нажмите Apply patch. Рядом с X2.exe будет создана резервная копия:
   X2.exe.backup-before-third-person-camera-patch
5. Запускайте игру напрямую через X2.exe.

Если Windows не разрешит изменить файл в папке игры, закройте Steam и игру,
затем нажмите на патчер правой кнопкой мыши и выберите «Запуск от имени
администратора».

Чтобы отменить патч, запустите патчер и нажмите Restore original. Он также
распознаёт резервную копию со старым именем
X2.exe.original-before-codex-overflow-fix. Исходный файл можно восстановить
через Steam: «Проверить целостность файлов игры».

Совместимость и контрольные суммы
---------------------------------
SHA-256 поддерживаемого оригинального X2.exe:
7951124FB1EB3598D9F3D2469357D39BBC5E745EFF21D6E2A115E315F9D265D8

SHA-256 исправленного X2.exe:
30C50BF37A67A1C88270C1ADE90B7C344F177236C35212755109A6B907DCBD76

SHA-256 патчера:
3454A52C35B85B8F97230B4B737D533574138FF8DD464C40D147EC5A4E5150FB

Патчер принимает только поддерживаемый оригинальный файл либо сообщает, что
исправление уже установлено. Если у вас другая версия или изменённый файл,
восстановите оригинальные файлы через Steam и запустите патчер снова. Патчер
не подписан цифровым сертификатом; Windows может показать предупреждение о
неизвестном издателе или SmartScreen. Сверьте SHA-256 и запускайте патчер,
только если доверяете источнику.

Исходный код игры и полный дамп сбоя недоступны. Это исправление на уровне
исполняемого файла для обнаруженного переполнения при целочисленном делении,
а не универсальное средство от всех вылетов X2. Старые отчёты с другим типом
ошибки доступа по адресу RVA 0x6D539 этим патчем не исправляются.

Included source: X2CameraPatch.cs

