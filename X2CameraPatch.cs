using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Microsoft.Win32;

namespace X2CameraPatch
{
    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new PatcherForm());
        }
    }

    internal sealed class PatcherForm : Form
    {
        private const string StockHash = "7951124FB1EB3598D9F3D2469357D39BBC5E745EFF21D6E2A115E315F9D265D8";
        private const string PatchedHash = "30C50BF37A67A1C88270C1ADE90B7C344F177236C35212755109A6B907DCBD76";
        private readonly TextBox pathBox;
        private readonly Label statusLabel;

        public PatcherForm()
        {
            Text = "X2: The Threat — Third-Person Camera Patch";
            Width = 690;
            Height = 300;
            MinimumSize = new System.Drawing.Size(620, 280);
            StartPosition = FormStartPosition.CenterScreen;

            Label intro = new Label();
            intro.Left = 18;
            intro.Top = 16;
            intro.Width = 630;
            intro.Height = 56;
            intro.Text = "Community patch for the Steam X2: The Threat 1.5.0.0 executable. " +
                         "It creates a verified backup, then patches your own X2.exe. " +
                         "Close the game before applying or restoring the patch.";
            intro.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            Controls.Add(intro);

            Label pathLabel = new Label();
            pathLabel.Left = 18;
            pathLabel.Top = 86;
            pathLabel.Width = 100;
            pathLabel.Text = "Game executable:";
            Controls.Add(pathLabel);

            pathBox = new TextBox();
            pathBox.Left = 18;
            pathBox.Top = 108;
            pathBox.Width = 490;
            pathBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            Controls.Add(pathBox);

            Button browseButton = new Button();
            browseButton.Left = 518;
            browseButton.Top = 106;
            browseButton.Width = 130;
            browseButton.Text = "Browse…";
            browseButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            browseButton.Click += BrowseClicked;
            Controls.Add(browseButton);

            Button detectButton = new Button();
            detectButton.Left = 18;
            detectButton.Top = 150;
            detectButton.Width = 145;
            detectButton.Text = "Find X2 in Steam";
            detectButton.Click += DetectClicked;
            Controls.Add(detectButton);

            Button patchButton = new Button();
            patchButton.Left = 173;
            patchButton.Top = 150;
            patchButton.Width = 150;
            patchButton.Text = "Apply patch";
            patchButton.Click += PatchClicked;
            Controls.Add(patchButton);

            Button restoreButton = new Button();
            restoreButton.Left = 333;
            restoreButton.Top = 150;
            restoreButton.Width = 150;
            restoreButton.Text = "Restore original";
            restoreButton.Click += RestoreClicked;
            Controls.Add(restoreButton);

            statusLabel = new Label();
            statusLabel.Left = 18;
            statusLabel.Top = 195;
            statusLabel.Width = 630;
            statusLabel.Height = 54;
            statusLabel.Text = "Ready. The patcher will only modify the exact supported original build.";
            statusLabel.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            Controls.Add(statusLabel);

            Shown += delegate { AutoDetect(false); };
        }

        private void BrowseClicked(object sender, EventArgs e)
        {
            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                dialog.Title = "Select X2.exe";
                dialog.Filter = "X2 executable (X2.exe)|X2.exe|Executable files (*.exe)|*.exe";
                dialog.CheckFileExists = true;
                if (dialog.ShowDialog(this) == DialogResult.OK)
                    pathBox.Text = dialog.FileName;
            }
        }

        private void DetectClicked(object sender, EventArgs e)
        {
            AutoDetect(true);
        }

        private void AutoDetect(bool showMessage)
        {
            try
            {
                string found = FindGameExecutable();
                if (found != null)
                {
                    pathBox.Text = found;
                    string hash = HashFile(found);
                    statusLabel.Text = hash == PatchedHash
                        ? "Found X2.exe. The patch is already installed."
                        : "Found X2.exe. Select Apply patch to install the fix.";
                    if (showMessage)
                        MessageBox.Show(this, "Found the game executable:\r\n" + found,
                                        "X2 found", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    statusLabel.Text = "Steam did not reveal the installation path. Use Browse… to select X2.exe.";
                    if (showMessage)
                        MessageBox.Show(this, "Could not locate X2 automatically. Use Browse… and select X2.exe.",
                                        "X2 not found", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                ShowError(ex);
            }
        }

        private void PatchClicked(object sender, EventArgs e)
        {
            string exePath = GetSelectedPath();
            if (exePath == null) return;
            try
            {
                EnsureGameClosed();
                string currentHash = HashFile(exePath);
                if (currentHash == PatchedHash)
                {
                    statusLabel.Text = "The supported patch is already installed.";
                    MessageBox.Show(this, "This X2.exe already has the supported patch.",
                                    "Already patched", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                if (currentHash != StockHash)
                    throw new InvalidDataException("This is not the supported original Steam 1.5.0.0 X2.exe.\r\n\r\n" +
                        "Use Steam → Properties → Installed Files → Verify integrity of game files, then try again.");

                string backupPath = exePath + ".backup-before-third-person-camera-patch";
                if (File.Exists(backupPath))
                {
                    if (HashFile(backupPath) != StockHash)
                        throw new InvalidDataException("A backup file already exists but does not match the original X2.exe. " +
                                                       "It was left untouched:\r\n" + backupPath);
                }
                else
                {
                    File.Copy(exePath, backupPath, false);
                    if (HashFile(backupPath) != StockHash)
                        throw new IOException("The original-file backup did not pass its hash check. No patch was applied.");
                }

                byte[] stock = File.ReadAllBytes(exePath);
                byte[] patched = BuildPatchedImage(stock);
                if (HashBytes(patched) != PatchedHash)
                    throw new InvalidDataException("The generated file did not match the verified patch build. No patch was applied.");

                string tempPath = exePath + "." + Guid.NewGuid().ToString("N") + ".tmp";
                try
                {
                    File.WriteAllBytes(tempPath, patched);
                    if (HashFile(tempPath) != PatchedHash)
                        throw new IOException("The temporary patch file failed its hash check.");
                    File.Replace(tempPath, exePath, null);
                }
                finally
                {
                    if (File.Exists(tempPath)) File.Delete(tempPath);
                }

                if (HashFile(exePath) != PatchedHash)
                    throw new IOException("The installed file did not pass the final hash check. Restore the original from the backup.");

                statusLabel.Text = "Patch installed. Original backup: " + Path.GetFileName(backupPath);
                MessageBox.Show(this,
                    "Patch installed successfully.\r\n\r\n" +
                    "You can now launch X2.exe directly.\r\n\r\n" +
                    "Original backup:\r\n" + backupPath,
                    "X2 patch installed", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (UnauthorizedAccessException)
            {
                ShowError(new UnauthorizedAccessException("Windows denied access to the game folder. Close Steam and the game, " +
                    "then right-click this patcher and choose Run as administrator."));
            }
            catch (Exception ex)
            {
                ShowError(ex);
            }
        }

        private void RestoreClicked(object sender, EventArgs e)
        {
            string exePath = GetSelectedPath();
            if (exePath == null) return;
            try
            {
                EnsureGameClosed();
                string backupPath = FindVerifiedBackup(exePath);
                if (backupPath == null)
                    throw new FileNotFoundException("A verified original backup was not found beside X2.exe.");

                string tempPath = exePath + "." + Guid.NewGuid().ToString("N") + ".restore";
                try
                {
                    File.Copy(backupPath, tempPath, false);
                    if (HashFile(tempPath) != StockHash)
                        throw new IOException("The temporary original file failed its hash check.");
                    File.Replace(tempPath, exePath, null);
                }
                finally
                {
                    if (File.Exists(tempPath)) File.Delete(tempPath);
                }

                statusLabel.Text = "Original X2.exe restored from the verified backup.";
                MessageBox.Show(this, "The original X2.exe has been restored.",
                                "Original restored", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (UnauthorizedAccessException)
            {
                ShowError(new UnauthorizedAccessException("Windows denied access to the game folder. Close Steam and the game, " +
                    "then right-click this patcher and choose Run as administrator."));
            }
            catch (Exception ex)
            {
                ShowError(ex);
            }
        }

        private string GetSelectedPath()
        {
            string exePath = pathBox.Text.Trim().Trim('"');
            if (Directory.Exists(exePath)) exePath = Path.Combine(exePath, "X2.exe");
            if (!File.Exists(exePath) || !String.Equals(Path.GetFileName(exePath), "X2.exe", StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show(this, "Select the game's X2.exe file.", "X2.exe required",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return null;
            }
            return Path.GetFullPath(exePath);
        }

        private static void EnsureGameClosed()
        {
            Process[] processes = Process.GetProcessesByName("X2");
            if (processes.Length > 0)
                throw new InvalidOperationException("X2 is still running. Close the game completely, then try again.");
        }

        private static string FindVerifiedBackup(string exePath)
        {
            string[] possible = new string[]
            {
                exePath + ".backup-before-third-person-camera-patch",
                exePath + ".original-before-codex-overflow-fix"
            };
            foreach (string backup in possible)
                if (File.Exists(backup) && HashFile(backup) == StockHash) return backup;
            return null;
        }

        private static string FindGameExecutable()
        {
            List<string> libraryRoots = new List<string>();
            AddLibraryRoot(libraryRoots, ReadSteamPath(Registry.CurrentUser, @"Software\Valve\Steam"));
            AddLibraryRoot(libraryRoots, ReadSteamPath(Registry.LocalMachine, @"SOFTWARE\WOW6432Node\Valve\Steam"));
            AddLibraryRoot(libraryRoots, ReadSteamPath(Registry.LocalMachine, @"SOFTWARE\Valve\Steam"));
            AddLibraryRoot(libraryRoots, Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86) + @"\Steam");
            AddLibraryRoot(libraryRoots, Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles) + @"\Steam");

            List<string> candidates = new List<string>();
            for (int rootIndex = 0; rootIndex < libraryRoots.Count; rootIndex++)
            {
                string root = libraryRoots[rootIndex];
                AddCandidate(candidates, Path.Combine(root, @"steamapps\common\X2 - The Threat\X2.exe"));
                string vdf = Path.Combine(root, @"steamapps\libraryfolders.vdf");
                if (File.Exists(vdf))
                {
                    string text = File.ReadAllText(vdf);
                    MatchCollection matches = Regex.Matches(text, "\\\"path\\\"\\s+\\\"([^\\\"]+)\\\"", RegexOptions.IgnoreCase);
                    foreach (Match match in matches)
                        AddLibraryRoot(libraryRoots, match.Groups[1].Value.Replace("\\\\", "\\"));
                    MatchCollection oldFormat = Regex.Matches(text, "\\\"[0-9]+\\\"\\s+\\\"([A-Za-z]:\\\\[^\\\"]+)\\\"");
                    foreach (Match match in oldFormat)
                        AddLibraryRoot(libraryRoots, match.Groups[1].Value.Replace("\\\\", "\\"));
                }
            }

            foreach (string candidate in candidates)
            {
                if (!File.Exists(candidate)) continue;
                string hash = HashFile(candidate);
                if (hash == StockHash || hash == PatchedHash) return candidate;
            }
            foreach (string candidate in candidates)
                if (File.Exists(candidate)) return candidate;
            return null;
        }

        private static string ReadSteamPath(RegistryKey root, string subKey)
        {
            try
            {
                using (RegistryKey key = root.OpenSubKey(subKey))
                {
                    if (key == null) return null;
                    object value = key.GetValue("SteamPath") ?? key.GetValue("InstallPath");
                    return value == null ? null : value.ToString();
                }
            }
            catch { return null; }
        }

        private static void AddLibraryRoot(List<string> roots, string path)
        {
            if (String.IsNullOrWhiteSpace(path)) return;
            path = Environment.ExpandEnvironmentVariables(path.Trim());
            try { path = Path.GetFullPath(path); }
            catch { return; }
            foreach (string existing in roots)
                if (String.Equals(existing, path, StringComparison.OrdinalIgnoreCase)) return;
            roots.Add(path);
        }

        private static void AddCandidate(List<string> candidates, string path)
        {
            foreach (string existing in candidates)
                if (String.Equals(existing, path, StringComparison.OrdinalIgnoreCase)) return;
            candidates.Add(path);
        }

        private static string HashFile(string path)
        {
            using (FileStream stream = File.OpenRead(path))
                return HashStream(stream);
        }

        private static string HashBytes(byte[] bytes)
        {
            using (SHA256 sha = SHA256.Create())
                return ToHex(sha.ComputeHash(bytes));
        }

        private static string HashStream(Stream stream)
        {
            using (SHA256 sha = SHA256.Create())
                return ToHex(sha.ComputeHash(stream));
        }

        private static string ToHex(byte[] bytes)
        {
            StringBuilder result = new StringBuilder(bytes.Length * 2);
            foreach (byte value in bytes) result.Append(value.ToString("X2"));
            return result.ToString();
        }

        private static void ShowError(Exception ex)
        {
            MessageBox.Show(ex.Message, "X2 patcher", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private static byte[] BuildPatchedImage(byte[] source)
        {
            if (HashBytes(source) != StockHash)
                throw new InvalidDataException("The input file is not the supported original X2.exe.");

            byte[][] expectedSites = new byte[][]
            {
                Hex("8B 4C 24 1C F7 EA F7 F9 8B 55 58"),
                Hex("8B 4C 24 1C F7 EA F7 F9 8B 55 58")
            };
            int[] patchSites = new int[] { 0x101BA, 0x101E7 };
            for (int i = 0; i < patchSites.Length; i++)
                if (!RangeEquals(source, patchSites[i], expectedSites[i]))
                    throw new InvalidDataException("Unexpected original bytes at patch location 0x" + patchSites[i].ToString("X") + ".");

            byte[] helper = MakeHelper();
            int helperRva;
            byte[] image = AddSection(source, helper, out helperRva);
            foreach (int site in patchSites)
            {
                byte[] call = Rel32(0xE8, site, helperRva);
                Array.Copy(call, 0, image, site, call.Length);
                for (int n = 5; n < 11; n++) image[site + n] = 0x90;
            }
            return image;
        }

        private static byte[] MakeHelper()
        {
            List<byte> code = new List<byte>();
            Dictionary<string, int> labels = new Dictionary<string, int>();
            List<KeyValuePair<int, string>> fixups = new List<KeyValuePair<int, string>>();

            Emit(code, "8B 4C 24 20");       // mov ecx,[esp+20h]
            Emit(code, "85 C9");             // test ecx,ecx
            Branch(code, fixups, 0x74, "fallback");
            Emit(code, "8B D1 C1 FA 1F 33 CA 2B CA");
            Emit(code, "81 F9 E8 03 00 00");
            Branch(code, fixups, 0x73, "safe");
            Emit(code, "53 50 8B D8 C1 FB 1F 31 D8 29 D8");
            Emit(code, "69 D1 9B C4 20 00");
            Emit(code, "3B C2 58 5B");
            Branch(code, fixups, 0x77, "fallback");

            labels["safe"] = code.Count;
            Emit(code, "BA E8 03 00 00");    // mov edx,1000
            Emit(code, "F7 EA");             // imul edx; dividend = EDX:EAX
            Emit(code, "8B 4C 24 20 F7 F9"); // reload divisor; idiv ecx
            Emit(code, "8B 55 58 C3");       // restore edx object base; ret

            labels["fallback"] = code.Count;
            Emit(code, "31 C0 8B 55 58 C3"); // eax=0; restore edx; ret

            foreach (KeyValuePair<int, string> fixup in fixups)
            {
                int displacement = labels[fixup.Value] - (fixup.Key + 1);
                if (displacement < -128 || displacement > 127)
                    throw new InvalidDataException("Helper branch is out of range.");
                code[fixup.Key] = unchecked((byte)displacement);
            }
            return code.ToArray();
        }

        private static void Branch(List<byte> code, List<KeyValuePair<int, string>> fixups, byte opcode, string label)
        {
            code.Add(opcode);
            code.Add(0);
            fixups.Add(new KeyValuePair<int, string>(code.Count - 1, label));
        }

        private static void Emit(List<byte> code, string bytes)
        {
            code.AddRange(Hex(bytes));
        }

        private static byte[] Hex(string text)
        {
            string[] parts = text.Split(new char[] { ' ', '\t', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            byte[] result = new byte[parts.Length];
            for (int i = 0; i < parts.Length; i++) result[i] = Convert.ToByte(parts[i], 16);
            return result;
        }

        private static bool RangeEquals(byte[] image, int offset, byte[] expected)
        {
            if (offset < 0 || offset + expected.Length > image.Length) return false;
            for (int i = 0; i < expected.Length; i++)
                if (image[offset + i] != expected[i]) return false;
            return true;
        }

        private static byte[] Rel32(byte opcode, int sourceRva, int targetRva)
        {
            byte[] result = new byte[5];
            result[0] = opcode;
            WriteInt32(result, 1, targetRva - (sourceRva + 5));
            return result;
        }

        private static byte[] AddSection(byte[] original, byte[] code, out int newRva)
        {
            int pe = ReadInt32(original, 0x3C);
            if (pe < 0 || pe + 24 > original.Length || !RangeEquals(original, pe, new byte[] { 0x50, 0x45, 0, 0 }))
                throw new InvalidDataException("Invalid PE executable.");
            int sectionCount = ReadUInt16(original, pe + 6);
            int optionalSize = ReadUInt16(original, pe + 20);
            int optional = pe + 24;
            if (ReadUInt16(original, optional) != 0x10B)
                throw new InvalidDataException("Only 32-bit PE executables are supported.");

            int sectionAlignment = ReadInt32(original, optional + 32);
            int fileAlignment = ReadInt32(original, optional + 36);
            int sizeOfHeaders = ReadInt32(original, optional + 60);
            int table = optional + optionalSize;
            int newHeader = table + sectionCount * 40;
            if (newHeader + 40 > sizeOfHeaders || newHeader + 40 > original.Length)
                throw new InvalidDataException("No room for a new PE section header.");
            for (int i = newHeader; i < newHeader + 40; i++)
                if (original[i] != 0) throw new InvalidDataException("The new PE section header area is not empty.");

            int lastVirtualEnd = 0;
            for (int i = 0; i < sectionCount; i++)
            {
                int off = table + i * 40;
                int virtualSize = ReadInt32(original, off + 8);
                int virtualAddress = ReadInt32(original, off + 12);
                int rawSize = ReadInt32(original, off + 16);
                lastVirtualEnd = Math.Max(lastVirtualEnd,
                    checked(virtualAddress + Align(Math.Max(virtualSize, rawSize), sectionAlignment)));
            }

            newRva = Align(lastVirtualEnd, sectionAlignment);
            int newRawPtr = Align(original.Length, fileAlignment);
            int newRawSize = Align(code.Length, fileAlignment);
            if (newRawSize <= 0) throw new InvalidDataException("Empty patch section.");

            byte[] image = new byte[checked(newRawPtr + newRawSize)];
            Array.Copy(original, image, original.Length);
            Array.Copy(code, 0, image, newRawPtr, code.Length);
            byte[] sectionName = Encoding.ASCII.GetBytes(".x2fix\0\0");
            Array.Copy(sectionName, 0, image, newHeader, 8);
            WriteInt32(image, newHeader + 8, code.Length);
            WriteInt32(image, newHeader + 12, newRva);
            WriteInt32(image, newHeader + 16, newRawSize);
            WriteInt32(image, newHeader + 20, newRawPtr);
            WriteInt32(image, newHeader + 24, 0);
            WriteInt32(image, newHeader + 28, 0);
            WriteUInt16(image, newHeader + 32, 0);
            WriteUInt16(image, newHeader + 34, 0);
            WriteInt32(image, newHeader + 36, 0x60000020);
            WriteUInt16(image, pe + 6, sectionCount + 1);
            WriteInt32(image, optional + 4, checked(ReadInt32(original, optional + 4) + newRawSize));
            WriteInt32(image, optional + 56, Align(checked(newRva + code.Length), sectionAlignment));
            WriteInt32(image, optional + 64, 0);
            return image;
        }

        private static int Align(int value, int alignment)
        {
            if (alignment <= 0 || (alignment & (alignment - 1)) != 0)
                throw new InvalidDataException("Invalid PE alignment.");
            return checked((value + alignment - 1) & ~(alignment - 1));
        }

        private static int ReadInt32(byte[] data, int offset)
        {
            if (offset < 0 || offset + 4 > data.Length) throw new InvalidDataException("Unexpected end of PE file.");
            return BitConverter.ToInt32(data, offset);
        }

        private static int ReadUInt16(byte[] data, int offset)
        {
            if (offset < 0 || offset + 2 > data.Length) throw new InvalidDataException("Unexpected end of PE file.");
            return BitConverter.ToUInt16(data, offset);
        }

        private static void WriteInt32(byte[] data, int offset, int value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            Array.Copy(bytes, 0, data, offset, 4);
        }

        private static void WriteUInt16(byte[] data, int offset, int value)
        {
            byte[] bytes = BitConverter.GetBytes((ushort)value);
            Array.Copy(bytes, 0, data, offset, 2);
        }
    }
}

