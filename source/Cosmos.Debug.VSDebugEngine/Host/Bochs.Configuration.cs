﻿using System;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

using Microsoft.Win32;

using Cosmos.Build.Common;
using Cosmos.Debug.Common;

namespace Cosmos.Debug.VSDebugEngine.Host
{
    public partial class Bochs
    {
        private NameValueCollection defaultConfigs = new NameValueCollection();
        private void InitializeKeyValues() {
            string BochsDirectory = Path.GetDirectoryName(BochsSupport.BochsExe.FullName);
            string default_configuration =
"# configuration file generated by Bochs\n" +
"plugin_ctrl: unmapped=1, biosdev=1, speaker=1, extfpuirq=1, parallel=1, serial=1, gameport=1\n" +
"config_interface: win32config\n" +
"display_library: win32\n" +
"memory: host=32, guest=32\n" +
"romimage: file=\""+BochsDirectory+"/BIOS-bochs-latest\"\n" +
"vgaromimage: file=\""+BochsDirectory+"/VGABIOS-lgpl-latest\"\n" +
"boot: cdrom\n" +
"floppy_bootsig_check: disabled=0\n" +
"# no floppya\n" +
"# no floppyb\n" +
"ata0: enabled=1, ioaddr1=0x1f0, ioaddr2=0x3f0, irq=14\n" +
"ata0-master: type=cdrom, path=\"%CDROMBOOTPATH%\", status=inserted, model=\"Generic 1234\", biosdetect=auto\n" +
"ata0-slave: type=none\n" +
"ata1: enabled=0\n" +
"ata2: enabled=0\n" +
"ata3: enabled=0\n" +
"pci: enabled=1, chipset=i440fx\n" +
"vga: extension=vbe, update_freq=5, realtime=1\n" +
"cpu: count=1, ips=4000000, model=corei5_lynnfield_750, reset_on_triple_fault=1, cpuid_limit_winnt=0, ignore_bad_msrs=1, mwait_is_nop=0\n" +
"print_timestamps: enabled=0\n" +
"port_e9_hack: enabled=0\n" +
"private_colormap: enabled=0\n" +
"clock: sync=none, time0=local, rtc_sync=0\n" +
"# no cmosimage\n" +
"# no loader\n" +
"log: -\n" +
"logprefix: %t%e%d\n" +
"debug: action=ignore\n" +
"info: action=report\n" +
"error: action=report\n" +
"panic: action=ask\n" +
"keyboard: type=mf, serial_delay=250, paste_delay=100000, user_shortcut=none\n" +
"mouse: type=ps2, enabled=0, toggle=ctrl+mbutton\n" +
"sound: waveoutdrv=win, waveout=none, waveindrv=win, wavein=none, midioutdrv=win, midiout=none\n" +
"speaker: enabled=1, mode=sound\n" +
"parport1: enabled=1, file=none\n" +
"parport2: enabled=0\n" +
"com1: enabled=1, mode=pip-client, dev=\"%PIPESERVERNAME%\"\n" +
"com2: enabled=0\n" +
"com3: enabled=0\n" +
"com4: enabled=0";
            string[] Keys = default_configuration.Split(new char[] {'\n'}, StringSplitOptions.RemoveEmptyEntries);
            for(int i = 0; i < Keys.Length; i++) {
                string comboItem = Keys[i];
                int KeyValueIndex = comboItem.IndexOf(":");
                if (KeyValueIndex > -1)
                {
                    string Key = comboItem.Substring(0, KeyValueIndex);
                    string Value = comboItem.Substring(KeyValueIndex + 1, comboItem.Length - KeyValueIndex - 1);
                    defaultConfigs.Add(Key, Value);
                }
                else
                {
                    defaultConfigs.Add(comboItem, "");
                }
            }
            string xPort = mParams["VisualStudioDebugPort"];
            string[] xParts = xPort.Split(new char[] {' '});
            defaultConfigs.Set("com1", defaultConfigs.Get("com1").Replace("%PIPESERVERNAME%", xParts[1].ToLower()));
            defaultConfigs.Set("ata0-master", defaultConfigs.Get("ata0-master").Replace("%CDROMBOOTPATH%", mParams["ISOFile"]));
        }

        private void GenerateConfiguration(string filePath)
        {
            FileStream configFileHandler = File.Create(filePath);
            for (int i = 0; i < defaultConfigs.AllKeys.Length; i++)
            {
                string value = defaultConfigs.Get(i);
                string key = defaultConfigs.GetKey(i);
                if (value.Length < 1)
                {
                    byte[] lineData = ASCIIEncoding.ASCII.GetBytes(key + Environment.NewLine);
                    configFileHandler.Write(lineData, 0, lineData.Length);
                }
                else
                {
                    string configItem = key + ":" + value;
                    byte[] lineData = ASCIIEncoding.ASCII.GetBytes(configItem + Environment.NewLine);
                    configFileHandler.Write(lineData, 0, lineData.Length);
                }
            }
            configFileHandler.Flush();
            configFileHandler.Close();
        }
    }
}
