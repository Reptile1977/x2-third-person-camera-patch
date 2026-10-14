# X2: The Threat — Third-Person Camera Crash Patch

Unofficial community patch for the Steam release of X2: The Threat, version 1.5.0.0.

## Download

**To use the patch, download and run this single file:** [X2-Third-Person-Camera-Patch.exe](X2-Third-Person-Camera-Patch.exe?raw=1)

The patcher's C# code from X2CameraPatch.cs is compiled into the EXE. The source file, Python, ZIP archive, and additional DLL files are not required to run it. It uses the Windows .NET Framework system libraries. The game executable itself is not included; the patcher modifies the player's own X2.exe.

The optional [ZIP package](X2-Third-Person-Camera-Patch-1.0.zip) contains the same EXE together with the bilingual instructions and source code. See [full instructions](README-X2-overflow-patch.txt) or [source code](X2CameraPatch.cs).

## Reported issue

The reported crash occurs when rotating the external third-person ship inspection camera. The game is launched directly through X2.exe. The patch author confirmed the camera scenario worked in two checks after applying the fix. This is an unofficial community patch and is not an Egosoft update.

## Installation

1. Close X2: The Threat.
2. Download X2-Third-Person-Camera-Patch.exe from the link above and run it.
3. Confirm the detected X2.exe path, or use Browse to select the game executable.
4. Click Apply patch. The patcher verifies the supported original file and creates a backup next to it.
5. Start the game directly through X2.exe.

To revert the change, run the patcher and choose Restore original. Steam's Verify integrity of game files command also restores the original executable.

The patcher uses the .NET Framework included with Windows 10/11. It is unsigned, so Windows may display a SmartScreen or unknown-publisher warning. Check the SHA-256 below and only run a copy obtained from a source you trust. The original game executable is not included or redistributed.

## Compatibility and SHA-256

| File | SHA-256 |
|---|---|
| Supported original X2.exe | 7951124FB1EB3598D9F3D2469357D39BBC5E745EFF21D6E2A115E315F9D265D8 |
| Patched X2.exe | 30C50BF37A67A1C88270C1ADE90B7C344F177236C35212755109A6B907DCBD76 |
| Windows patcher EXE | 3454A52C35B85B8F97230B4B737D533574138FF8DD464C40D147EC5A4E5150FB |
| ZIP package | D3ECDEE5B0C0B58E8463A2D5BD0852951FC50166EBF510E34718656A43CA8D8 |

The patcher accepts only the supported original X2.exe or reports that the patch is already installed. If your executable has another version or has been modified, restore the original files through Steam first. The game source code and a full crash dump were not available; this binary-level fix addresses the recorded integer-division overflow signature and is not a general fix for every X2 crash. Older reports with a different access-violation signature at RVA 0x6D539 are outside its scope.

## Русский

Для установки достаточно скачать и запустить один файл: [X2-Third-Person-Camera-Patch.exe](X2-Third-Person-Camera-Patch.exe?raw=1). Исходный код X2CameraPatch.cs уже скомпилирован в EXE. Отдельный .cs, Python, архив ZIP или дополнительные DLL для запуска не нужны. Патчер использует системные библиотеки .NET Framework в Windows и изменяет установленный у игрока X2.exe; оригинальный файл игры в патчер не включён.

Неофициальный пользовательский патч для Steam-версии X2: The Threat 1.5.0.0 исправляет вылет при вращении внешней камеры осмотра корабля от третьего лица. Игру запускали напрямую через X2.exe. Автор проверил этот режим дважды после установки исправления — камера работала без вылета. Это не официальное обновление Egosoft.

### Установка

1. Закройте X2: The Threat.
2. Скачайте и запустите X2-Third-Person-Camera-Patch.exe.
3. Проверьте путь к найденному X2.exe или выберите его через Browse.
4. Нажмите Apply patch. Патчер проверит исходный файл и создаст рядом резервную копию.
5. Запустите игру напрямую через X2.exe.

Чтобы отменить исправление, запустите патчер и нажмите Restore original. Также исходный файл можно восстановить через Steam: «Проверить целостность файлов игры». Подробная инструкция на русском и английском находится в [README-X2-overflow-patch.txt](README-X2-overflow-patch.txt); ZIP с исходником и инструкцией — необязательный.

Патчер использует .NET Framework, входящий в состав Windows 10/11. Он не подписан цифровым сертификатом, поэтому Windows может показать предупреждение SmartScreen или об неизвестном издателе. Сверьте SHA-256 выше и запускайте только файл из источника, которому доверяете.

Патчер принимает только поддерживаемый оригинальный X2.exe либо сообщает, что исправление уже установлено. Если у вас другая версия или изменённый файл, сначала восстановите оригинал через Steam. Исходный код игры и полный дамп сбоя недоступны; исправление на уровне исполняемого файла направлено на обнаруженное переполнение при целочисленном делении и не является универсальным исправлением всех вылетов X2. Старые сообщения о другой ошибке доступа по RVA 0x6D539 находятся вне области этого патча.