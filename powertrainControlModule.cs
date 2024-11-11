#region Copyright (c) 2024, Jack Leighton
// /////     __________________________________________________________________________________________________________________
// /////
// /////                  __                   __              __________                                      __   
// /////                _/  |_  ____   _______/  |_  __________\______   \_______   ____   ______ ____   _____/  |_ 
// /////                \   __\/ __ \ /  ___/\   __\/ __ \_  __ \     ___/\_  __ \_/ __ \ /  ___// __ \ /    \   __\
// /////                 |  | \  ___/ \___ \  |  | \  ___/|  | \/    |     |  | \/\  ___/ \___ \\  ___/|   |  \  |  
// /////                 |__|  \___  >____  > |__|  \___  >__|  |____|     |__|    \___  >____  >\___  >___|  /__|  
// /////                           \/     \/            \/                             \/     \/     \/     \/      
// /////                                                          .__       .__  .__          __                    
// /////                               ____________   ____   ____ |__|____  |  | |__| _______/  |_                  
// /////                              /  ___/\____ \_/ __ \_/ ___\|  \__  \ |  | |  |/  ___/\   __\                 
// /////                              \___ \ |  |_> >  ___/\  \___|  |/ __ \|  |_|  |\___ \  |  |                   
// /////                             /____  >|   __/ \___  >\___  >__(____  /____/__/____  > |__|                   
// /////                                  \/ |__|        \/     \/        \/             \/                         
// /////                                  __                         __  .__                                        
// /////                   _____   __ ___/  |_  ____   _____   _____/  |_|__|__  __ ____                            
// /////                   \__  \ |  |  \   __\/  _ \ /     \ /  _ \   __\  \  \/ // __ \                           
// /////                    / __ \|  |  /|  | (  <_> )  Y Y  (  <_> )  | |  |\   /\  ___/                           
// /////                   (____  /____/ |__|  \____/|__|_|  /\____/|__| |__| \_/  \___  >                          
// /////                        \/                         \/                          \/                           
// /////                                                  .__          __  .__                                      
// /////                                       __________ |  |  __ ___/  |_|__| ____   ____   ______                
// /////                                      /  ___/  _ \|  | |  |  \   __\  |/  _ \ /    \ /  ___/                
// /////                                      \___ (  <_> )  |_|  |  /|  | |  (  <_> )   |  \\___ \                 
// /////                                     /____  >____/|____/____/ |__| |__|\____/|___|  /____  >                
// /////                                          \/                                      \/     \/                 
// /////                                   Tester Present Specialist Automotive Solutions
// /////     __________________________________________________________________________________________________________________
// /////      |--------------------------------------------------------------------------------------------------------------|
// /////      |       https://github.com/jakka351/| https://testerPresent.com.au | https://facebook.com/testerPresent        |
// /////      |--------------------------------------------------------------------------------------------------------------|
// /////      | Copyright (c) 2022/2023/2024 Benjamin Jack Leighton                                                          |          
// /////      | All rights reserved.                                                                                         |
// /////      |--------------------------------------------------------------------------------------------------------------|
// /////        Redistribution and use in source and binary forms, with or without modification, are permitted provided that
// /////        the following conditions are met:
// /////        1.    With the express written consent of the copyright holder.
// /////        2.    Redistributions of source code must retain the above copyright notice, this
// /////              list of conditions and the following disclaimer.
// /////        3.    Redistributions in binary form must reproduce the above copyright notice, this
// /////              list of conditions and the following disclaimer in the documentation and/or other
// /////              materials provided with the distribution.
// /////        4.    Neither the name of the organization nor the names of its contributors may be used to
// /////              endorse or promote products derived from this software without specific prior written permission.
// /////      _________________________________________________________________________________________________________________
// /////      THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES,
// /////      INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
// /////      DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
// /////      SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
// /////      SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY,
// /////      WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE
// /////      USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
// /////      _________________________________________________________________________________________________________________
// /////
// /////       This software can only be distributed with my written permission. It is for my own educational purposes and  
// /////       is potentially dangerous to ECU health and safety. Gracias a Gato Blancoford desde las alturas del mar de chelle.                                                        
// /////      _________________________________________________________________________________________________________________
// /////
// /////
// ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
#endregion License
// ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using J2534;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.IO;
using System.IO.Ports;
using System.Net;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using Newtonsoft.Json.Linq;
using System.Linq;
using System.Diagnostics;
using System.Reflection;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
// ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
namespace FGCOM
{
    public partial class Orion
    {
        // ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        bool flagNoDataAbort = false; bool successfulFlashRead = false;
        /////////////////////////////////////////////////////////////////////////////////////
        // READ FLASH PCM ROUTINE
        async void readPcmFlash()
        {
            try
            {
                //1,048,576 bytes 1024 kb
                byte[] flashMemory = new byte[0x100000]; uint blockSize = 0x400; //This is the largest size (0x800)we can request that is an even divisible number, 0x900 is supported but then we need an odd request at the end // 0x400 is a qquicker readflash than 0x800
                int attempts = 3; int attemptCount = 0;
                textBox1.Text = ""; progressBarPcmFlash.Value = 0; labelFlashRead.Visible = false; labelFlashWrite.Visible = false;
                var ErrorResult = J2534Port.Functions.ClearRxBuffer((int)ChannelID); // CLEAR THE RX BUFFER
                if (ErrorResult != J2534Err.STATUS_NOERROR) { addTxt1(ErrorResult.ToString() + "\r\n"); addTxt1("Error Clearing RX Buffer) \r\n"); }
                else { addTxt1("Cleared RX Buffer\r\n");  }
                ErrorResult = J2534Port.Functions.ClearTxBuffer((int)ChannelID); // CLEAER THE TX BUFFER
                if (ErrorResult != J2534Err.STATUS_NOERROR) { addTxt1(ErrorResult.ToString() + "\r\n"); addTxt1("Error Clearing TX Buffer) \r\n"); }
                else { addTxt1("Cleared TX Buffer\r\n"); }
                var saveFileDialog2 = new SaveFileDialog(); saveFileDialog2.Filter = "binary files (*.bin)|*.bin"; saveFileDialog2.FilterIndex = 2;
                if (saveFileDialog2.ShowDialog() == DialogResult.OK)
                {
    				string filePath = saveFileDialog2.FileName;
    				if (new[] { "11V", "10V", "9V", "8V", "7V", "6V" }.Contains(textBoxVolt.Text))                  //check battery voltage before continuing.. otherwise you will have a bad time getting security access
                    {
                        addTxt1("Warning - Battery Voltage Low");
                        DialogResult result = MessageBox.Show("FoA Orion Comms has detected low battery voltage - PCM Flash Memory Read may not be successful... Are you sure you want to continue?",
                                                             "Warning - Battery Voltage Low",
                                                             MessageBoxButtons.YesNoCancel,
                                                             MessageBoxIcon.Question);
                        if (result != DialogResult.Yes) { return; }
                    }
                    if(comboBoxPcmType.SelectedIndex == 0x01) { flashMemory = new byte[0x170000]; }
                    //textBoxPcmFlash.Text = "";
                    //Set PCM RX AND TX CAN IDS
                    addTxt1("Connecting to Powertrain Control Module...\r\n"); reset2Pcm(); setProgrammingVoltage0(); controlDtcSetting(); addTxt1("Enabling FEPS...\r\n");
					FileStream fileStream = File.Open(filePath, FileMode.Create);
                    AttemptSecurity:
                        attemptCount++; setProgrammingVoltage18();
    			        System.Windows.Forms.MessageBox.Show("Turn ignition off for 3 seconds, then turn back on.", 
                                                             "FoA Orion Comms - Powertrain Control Module", 
                                                             MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                        if(requestSecurityAccess0x27(0x08, 0x30, 0x61, 0xA4, 0xC5) == true)
                        {
                            Thread.Sleep(1000);
            				//read flash memory by DMR 0x23 readMemoryByAddress
                            if(comboBoxPcmType.SelectedIndex == 0x01)
                            {
                                for (uint i = 0; i <= 0x600000; i+= blockSize)
                                {
                                    addTxt1("Recieving Flash Memory... ");
                                    flashMemory = readMemoryByAddress(i, blockSize); fileStream.Write(flashMemory, 0, flashMemory.Length);
                                    if (i == 458752) { i = 5242880; } if (i == 0x600000) { successfulFlashRead = true; }
                                }
                            }
                            else
                            {
                                int j = 0;
           		                for (uint i = 0x0; i <= 0x100000; i+= blockSize)
                                {
                                    addTxt1("Recieving Flash Memory... "); progressBarPcmFlash.Value = (int)i;
                                    flashMemory = readMemoryByAddress(i, blockSize); fileStream.Write(flashMemory, 0, flashMemory.Length);
                                    if (textBox1.Text.Contains("ERR_BUFFER_EMPTY")) { textBox1.Text = ""; j++; if(j == 10) { flagNoDataAbort = true; } }
                                    if(flagNoDataAbort == true) { successfulFlashRead = false; break; }
            						if (i == 0x100000) { successfulFlashRead = true; }
                				}                            
                            }
                            if(successfulFlashRead == false) { flagNoDataAbort = false; Form flashReadFail = new FGCOM.lib.forms.flashreadfail(); flashReadFail.Activate(); flashReadFail.Show(); return; }
                            if(successfulFlashRead == true)
                            {
        						fileStream.Close();
        						setProgrammingVoltage0(); labelFlashRead.Visible = true; removeFirstByteFromPcmBin(filePath);
                                Form flashRead = new FGCOM.lib.forms.flashread(); flashRead.Activate(); flashRead.Show();                                              
                                checkPcmBinSize(filePath); readPcmBinStrategy(filePath); readPcmBinVin(filePath); readHardwareIdFromPcmBin(filePath); readOsIdFromPcmBin(filePath); readPartNoFromPcmBin(filePath); readSerialNoFromPcmBin(filePath);
                                ErrorResult = J2534Port.Functions.ClearRxBuffer((int)ChannelID); // CLEAR THE RX BUFFER
                                if (ErrorResult != J2534Err.STATUS_NOERROR) { addTxt1(ErrorResult.ToString() + "\r\n"); addTxt1("Error Clearing RX Buffer) \r\n"); }
                                else { addTxt1("Cleared RX Buffer\r\n");  }
                                ErrorResult = J2534Port.Functions.ClearTxBuffer((int)ChannelID); // CLEAER THE TX BUFFER
                                if (ErrorResult != J2534Err.STATUS_NOERROR) { addTxt1(ErrorResult.ToString() + "\r\n"); addTxt1("Error Clearing TX Buffer) \r\n"); }
                                else { addTxt1("Cleared TX Buffer\r\n"); }
                                byte[] clearDtc = new byte[] { 0, 0, ecuRxIdentifier1, ecuRxIdentifier2, serviceClearDiagnosticInformation, 0xFF, 0x00}; string clearDtcMsg = sendPassThruMsg(clearDtc);
        					}
            			    return;
                        }
                        else
                        {
                            if (attemptCount < attempts) { setProgrammingVoltage0(); addTxt1($"Security Access Attempt: {attemptCount} failed, retrying..."); goto AttemptSecurity; }
                            else { addTxt1("Flash Read Failed \r\n"); setProgrammingVoltage0(); Form flashReadFail = new FGCOM.lib.forms.flashreadfail(); flashReadFail.Activate(); flashReadFail.Show(); return; }
                        }
                }
            }
            catch (Exception ex) { setProgrammingVoltage0(); addTxt1("FLASH Read Error: " + ex.Message); return; }
        }
		//////////////////////////////////////////////////////////
		// WRITE FLASH PCM ROUTINE
		//////////////////////////
        //development time: 
		bool successfulFlashWrite = false;
        async void writePcmFlash()
        {
            try
            {
                var ErrorResult = J2534Port.Functions.ClearRxBuffer((int)ChannelID); // CLEAR THE RX BUFFER
                if (ErrorResult != J2534Err.STATUS_NOERROR) { addTxt1(ErrorResult.ToString() + "\r\n"); addTxt1("Error Clearing RX Buffer) \r\n"); }
                else { addTxt1("Cleared RX Buffer\r\n");  }
                ErrorResult = J2534Port.Functions.ClearTxBuffer((int)ChannelID); // CLEAER THE TX BUFFER
                if (ErrorResult != J2534Err.STATUS_NOERROR) { addTxt1(ErrorResult.ToString() + "\r\n"); addTxt1("Error Clearing TX Buffer) \r\n"); }
                else { addTxt1("Cleared TX Buffer\r\n"); }
                progressBarPcmFlash.Value = 0; labelFlashRead.Visible = false;labelFlashWrite.Visible = false; // Hide flash completed label //
                int attempts = 3; int attemptCount = 0; int attempts2 = 3; int attemptCount2 = 0;
                textBox1.Text = ""; // Clear all text from the textbox1 //
                if (new[] { "11V", "10V", "9V", "8V", "7V", "6V" }.Contains(textBoxVolt.Text)) // Check our battery voltage before continuing and prompt the user whether to continue or not //
                {
                    addTxt1("Warning - Battery Voltage Low");
                    DialogResult result = MessageBox.Show(
                        "FoA Orion Comms has detected low battery voltage - PCM Flash Memory Write may not be successful... Are you sure you want to continue?",
                        "Warning - Battery Voltage Low",
                        MessageBoxButtons.YesNoCancel,
                        MessageBoxIcon.Question
                    );
                    if (result != DialogResult.Yes) { return; } // return if the user does not want to continue the flashing process //
                }
                using (OpenFileDialog openFileDialog = new OpenFileDialog())  // prompt the user to select a .bin file to flash to the PCM //
                {
                    openFileDialog.InitialDirectory = "\\"; openFileDialog.Filter = "Binary files (*.*)|*.*|Bin files (*.bin)|*.bin"; openFileDialog.FilterIndex = 1; openFileDialog.RestoreDirectory = true;
                    if (openFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        string filePath = openFileDialog.FileName; addTxt1("Parsing Binary File for flashing...\r\n"); byte[] flashData = File.ReadAllBytes(filePath);  addTxt1("Connecting to Powertrain Control Module...\r\n");
                        reset2Pcm(); controlDtcSetting(); // turn off fault code logging so we dont log fault codes //
                        while (attemptCount < attempts) // Security access and flash erase attempts //
                        {
                            attemptCount = attemptCount + 5; addTxt1("Enabling FEPS...\r\n"); setProgrammingVoltage18(); addTxt1("Turn ignition off for 3 seconds, then turn back on.\r\n");  // prompt the user to turn the key off and back on again so that PCM sees the FEPS voltage signal //
                            MessageBox.Show(
                                "Turn ignition off for 3 seconds, then turn back on.",
                                "FoA Orion Comms - Powertrain Control Module",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Asterisk
                            );
                            if (requestSecurityAccess0x27(0x08, 0x30, 0x61, 0xA4, 0xC5)) // unlock the controller //
                            {
                                addTxt1("Erasing Flash...please wait 5 seconds...\r\n"); flashErasePcm(); // Erase the Flash Memory of the PCM //
                                byte[] spanishOak = { 0, 0, 0x07, 0xE0, 0x34, 0x00, 0x01, 0x00, 0x00, 0x00, 0x0F, 0x00, 0x00 }; byte[] blackOak = { 0, 0, 0x07, 0xE0, 0x34, 0x00, 0x50, 0x00, 0x00, 0x00, 0x00, 0x10, 0x00 }; // PCM Request Download Message Bytes Black Oak //
                                // Black Oak Request Download
                                //  can0  RX - -  7E0   [8]  10 09 34 00 50 00 00 00   '..4.P...'
                                //  can0  RX - -  7E8   [8]  30 00 00 00 00 00 00 00   '0.......'
                                //  can0  RX - -  7E0   [8]  21 10 00 00 00 00 00 00   '!.......'
                                addTxt1("Erasing Flash...please wait...\r\n"); Thread.Sleep(2000); // sleep 2 sec //
                                if(comboBoxPcmType.SelectedIndex == 0x01) { addTxt1("Erasing Black Oak Flash Memory...please wait 60 seconds...\r\n"); Thread.Sleep(60000); } // black oak takes a minute to erase the flash so we wait //
                                addTxt1("Preparing to Flash.\r\n"); int erase = 0;
                                if(comboBoxPcmType.SelectedIndex == 0x01)
                                {
                                    string targetString = "Rx: 00  00  07  E8  F1  00  B2  "; // Check if the text in textBox1 contains the target string //
                                    checkForFlashEraseFinish:
                                        erase++;
                                        if (textBox1.Text.Contains(targetString))
                                        {
                                            Thread.Sleep(2000);
                                            if (requestDownload(blackOak))
                                            {
                                                addTxt1("Preparing to Flash..\r\n");
                                                addTxt1("Writing Data to Flash Memory...\r\n");
                                                writeFlash(flashData); // write the data to flash memory //
                                                attemptCount = 5;
                                                if (successfulFlashWrite) // successfulFlashWrite boolean returns from flash write function //
                                                {
                                                    setProgrammingVoltage0(); labelFlashWrite.Visible = true; addTxt1("Finished Writing Flash Memory\r\n"); 
                                                    MessageBox.Show(
                                                        "PCM Flash Memory Write Successful!",
                                                        "FoA Orion Comms - Powertrain Control Module",
                                                        MessageBoxButtons.OK,
                                                        MessageBoxIcon.Asterisk
                                                    );
                                                    return;
                                                }
                                            }
                                            else
                                            {
                                                if(erase <= 2) { goto checkForFlashEraseFinish; }
                                                else 
                                                {
                                                    setProgrammingVoltage0(); addTxt1("Flash Write Failed at Request Download \r\n");
                                                    MessageBox.Show(
                                                        "Flash Write Failed - Request Download Error.",
                                                        "FoA Orion Comms - Powertrain Control Module",
                                                        MessageBoxButtons.OK,
                                                        MessageBoxIcon.Asterisk);
                                                    return;                                            
                                                }
                                            }
                                        }
                                }
                                else
                                {
                                    requestDownload(spanishOak);
                                    if (requestDownload(spanishOak))
                                    {
                                        addTxt1("Preparing to Flash..\r\n"); Thread.Sleep(1000); testerPresent(suppressResponse); Thread.Sleep(1000); addTxt1("Preparing to Flash...\r\n"); testerPresent(suppressResponse); Thread.Sleep(1000);
                                        addTxt1("Writing Data to Flash Memory...\r\n"); writeFlash(flashData); attemptCount = 5;
                                        if (successfulFlashWrite)
                                        {
                                            setProgrammingVoltage0(); labelFlashWrite.Visible = true; addTxt1("Finished Writing Flash Memory\r\n");
                                            Form flashWrite = new FGCOM.lib.forms.flashcomplete(); flashWrite.Activate(); flashWrite.Show();
                                            byte[] clearDtc = new byte[] { 0, 0, ecuRxIdentifier1, ecuRxIdentifier2, serviceClearDiagnosticInformation, 0xFF, 0x00}; string clearDtcMsg = sendPassThruMsg(clearDtc);
                                            return;
                                        }
                                    }
                                    else
                                    {
                                        setProgrammingVoltage0(); addTxt1("Flash Write Failed at Request Download \r\n");
            							MessageBox.Show(
            								"Flash Write Failed - Request Download Error. Please try Flash Write again.",
            								"FoA Orion Comms - Powertrain Control Module",
            								MessageBoxButtons.OK,
            								MessageBoxIcon.Asterisk);
                                        return;
                                    }
                                }
                            }
                            else { setProgrammingVoltage0(); addTxt1($"Security Access Attempt: {attemptCount} failed, retrying..."); }
                        }
                        setProgrammingVoltage0(); addTxt1("Flash Write Failed - Security Access Not Granted\r\n");
                        MessageBox.Show(
                            "Flash Write Failed - Security Access Not Granted.",
                            "FoA Orion Comms - Powertrain Control Module",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Asterisk
                        );
                    }
                }
            }
            catch (Exception ex) { setProgrammingVoltage0(); addTxt1("FLASH Write ERROR: " + ex.Message); }
        }
        private void writeFlash(byte[] data)
        {
            try
            {
                int blocksize = 0x400;
                if (data.Length < 100000) { addTxt1("ERROR data length < 100000 \r\n"); return; }
                if (comboBoxPcmType.SelectedIndex == 0x01) // for Black Oak PCMs
                {
                    for (int i = 0x10000; i <= 0x600000; i += blocksize)
                    {
                        addTxt1("Uploading Flash Memory...\r\n");
                        progressBarPcmFlash.Invoke((MethodInvoker)delegate {
                           progressBarPcmFlash.Value = (int)i;
                        });
                        transferDataUDSFord(data, i, blocksize); 
                        if (i == 458752) { i = 5242880; }
                        if (i == 0x600000) 
                        { 
                            successfulFlashWrite = true; addTxt1("Finalizing Download to ECU...\r\n"); Thread.Sleep(2000); requestTransferExit();
                            var ErrorResult = J2534Port.Functions.ClearRxBuffer((int)ChannelID); // CLEAR THE RX BUFFER
                            if (ErrorResult != J2534Err.STATUS_NOERROR) { addTxt1(ErrorResult.ToString() + "\r\n"); addTxt1("Error Clearing RX Buffer) \r\n"); }
                            else { addTxt1("Cleared RX Buffer\r\n");  }
                            ErrorResult = J2534Port.Functions.ClearTxBuffer((int)ChannelID); // CLEAER THE TX BUFFER
                            if (ErrorResult != J2534Err.STATUS_NOERROR) { addTxt1(ErrorResult.ToString() + "\r\n"); addTxt1("Error Clearing TX Buffer) \r\n"); }
                            else { addTxt1("Cleared TX Buffer\r\n"); }
                            return;
                        }
                    }
                }
                else //for Spanish Oak PCMs
                {
                    for (int i = 0x10000; i <= 0xFFC00; i += blocksize)
                    {
                        addTxt1("Uploading Flash Memory...\r\n");
                        progressBarPcmFlash.Invoke((MethodInvoker)delegate {
                           progressBarPcmFlash.Value = (int)i;
                        });
                        transferDataUDSFord(data, i, blocksize);
                        if (i == 0xFFC00)
                        {
                            successfulFlashWrite = true; addTxt1("Finalizing Download to ECU...\r\n"); Thread.Sleep(2000); 
    						requestTransferExit();
                            var ErrorResult = J2534Port.Functions.ClearRxBuffer((int)ChannelID); // CLEAR THE RX BUFFER
                            if (ErrorResult != J2534Err.STATUS_NOERROR) { addTxt1(ErrorResult.ToString() + "\r\n"); addTxt1("Error Clearing RX Buffer) \r\n"); }
                            else { addTxt1("Cleared RX Buffer\r\n");  }
                            ErrorResult = J2534Port.Functions.ClearTxBuffer((int)ChannelID); // CLEAER THE TX BUFFER
                            if (ErrorResult != J2534Err.STATUS_NOERROR) { addTxt1(ErrorResult.ToString() + "\r\n"); addTxt1("Error Clearing TX Buffer) \r\n"); }
                            else { addTxt1("Cleared TX Buffer\r\n"); }
                            return;
    					}
    				}
                }
            }
            catch (Exception ex) { addTxt1("Transfer Data ERROR: " + ex.Message); }
        }
        //thanks rolando
        public void transferDataUDSFord(byte[] data, int offset, int length, int blockSize = -1)
        {
            if (blockSize == -1) blockSize = length;  byte[] transferData = new byte[length + 5];
            transferData[0] = 0x00; transferData[1] = 0x00; transferData[2] = ecuRxIdentifier1; transferData[3] = ecuRxIdentifier2; transferData[4] = serviceTransferData;
            Buffer.BlockCopy(data, offset, transferData, 5, blockSize);
            sendPassThruMsg(transferData);
        }
        /////////////////////////////////////////////////////////////////////////////////////
        // FLASH ERASE
        // erase the PCM Flash with this 0xB1 Diagnostic Command
        // ////////////////////////////////////////////////////
        byte[] flashErase = new byte[] { 0x00, 0xB2, 0xAA };
        void flashErasePcm()
        {
            diagnosticCommand(flashErase);
        }
        // ////////////////////////////////////////////////////
        static byte[] RemoveFirstNBytes(byte[] originalArray, int n)
        {
            // Check if the original array is large enough
            if (originalArray == null || originalArray.Length <= n)
            {
                throw new ArgumentException("The original array is too small or null.");
            }
            // Create a new array with a size reduced by n
            byte[] newArray = new byte[originalArray.Length - n];
            // Copy the data from the original array to the new array, starting from index n
            Array.Copy(originalArray, n, newArray, 0, newArray.Length);
            return newArray;
        }
        // ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public byte[] readMemoryByAddress(uint address, uint blockSize)
        {
            //Send the read memory request
            byte blockSizeUpper = (byte)((blockSize >> 8) & 0xFF);
            byte blockSizeLower = (byte)(blockSize & 0xFF);
            byte[] readMemoryByAddress = new byte[] { 0, 0, ecuRxIdentifier1, ecuRxIdentifier2, serviceReadMemoryByCommonAddress, 0x00, (byte)((address >> 16) & 0xFF), (byte)((address >> 8) & 0xFF), (byte)((address) & 0xFF), blockSizeUpper, blockSizeLower };
            int NumberOfMsgs = 1;
            PassThruMsg WriteMsg = new PassThruMsg(ProtocolID.ISO15765, TxFlag.ISO15765_FRAME_PAD, readMemoryByAddress); 
            IntPtr WritePtr = WriteMsg.ToIntPtr();
            var ErrorResult = J2534Port.Functions.PassThruWriteMsgs((int)ChannelID, WritePtr, ref NumberOfMsgs, 0);//timeout of 0 means just send it and dont care how long.
            if (ErrorResult != J2534Err.STATUS_NOERROR)
            {
                Log(ErrorResult.ToString() + "\r\n");
                flagNoDataAbort = true;
                //Shits fucked, fauled writing.
            }
            else
            {
                //Log("PassThru Write Msg Success \r\n");
            }
            // ///////////////////////////////////////////////////////////////
            //8) Read Respnse
            //byte[] nodata = new byte[1];
            bool SearchForResponse = true;
            while (SearchForResponse == true)
            {
                int NumReadMsgs = 1;
                IntPtr MyRXMsg = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(PassThruMsg)) * NumReadMsgs);
                ErrorResult = J2534Port.Functions.PassThruReadMsgs((int)ChannelID, MyRXMsg, ref NumReadMsgs, 20); //this is your timeout here.
                if (ErrorResult != J2534Err.STATUS_NOERROR) //if no frames received, it goes here.
                {
                    Log(ErrorResult.ToString() + "\r\n");
                    Log("Failed to read PassThru Msg \r\n");
                    //Shits fucked, fauled reading!!!
                    break;
                }
                else
                {
                    //Log("PassThru Read Msg Success \r\n");
                }
                //Convert the memory pointer back to a PassThruMsg Object
                PassThruMsg FoundFrame = MyRXMsg.AsMsgList(1).Last();
                if (((int)FoundFrame.RxStatus == ((int)J2534.RxStatus.TX_INDICATION_SUCCESS ^ (int)J2534.RxStatus.TX_MSG_TYPE)) ||
                    ((int)FoundFrame.RxStatus == ((int)J2534.RxStatus.TX_INDICATION_SUCCESS ^ (int)J2534.RxStatus.TX_MSG_TYPE ^ (int)J2534.RxStatus.ISO15765_ADDR_TYPE)) ||
                    ((int)FoundFrame.RxStatus == ((int)J2534.RxStatus.START_OF_MESSAGE))
                    )
                {
                    //We dont want any of this, continue!
                    Marshal.FreeHGlobal(MyRXMsg);
                    continue;
                }
                Marshal.FreeHGlobal(MyRXMsg);
                //This should have our bytes!
                byte[] MyRXDBytes = FoundFrame.GetBytes();
                string DataToString = "";
                for (int i = 0; i < MyRXDBytes.Length; i++)
                {
                    DataToString += MyRXDBytes[i].ToString("X2") + "  ";
                }
                Log("Rx: " + DataToString + "\r\n");
                //DataToString = DataToString.Replace(" ", "");
                //DataToString = DataToString.Substring(10);
                //return DataToString;
                // 00 00 07 28 63 11 22 33 44 55 66
                // Here we are removing the first five bytes of the passthru message, and returning on the flash memory bytes //
                byte[] flashBytes = RemoveFirstNBytes(MyRXDBytes, 5);
                return flashBytes;
            }
            //string noData = "";
            //return noData;
            byte[] nodata = new byte[] {0x00};
            return nodata;
        }
        // ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //  _______         _________________                                              __   _________                          .__  __            _____                                    
        //  \   _  \ ___  __\_____  \______  \_______   ____  ________ __   ____   _______/  |_/   _____/ ____   ____  __ _________|__|/  |_ ___.__. /  _  \   ____  ____  ____   ______ ______
        //  /  /_\  \\  \/  //  ____/   /    /\_  __ \_/ __ \/ ____/  |  \_/ __ \ /  ___/\   __\_____  \_/ __ \_/ ___\|  |  \_  __ \  \   __<   |  |/  /_\  \_/ ___\/ ___\/ __ \ /  ___//  ___/
        //  \  \_/   \>    </       \  /    /  |  | \/\  ___< <_|  |  |  /\  ___/ \___ \  |  | /        \  ___/\  \___|  |  /|  | \/  ||  |  \___  /    |    \  \__\  \__\  ___/ \___ \ \___ \ 
        //   \_____  /__/\_ \_______ \/____/   |__|    \___  >__   |____/  \___  >____  > |__|/_______  /\___  >\___  >____/ |__|  |__||__|  / ____\____|__  /\___  >___  >___  >____  >____  >
        //         \/      \/       \/                     \/   |__|           \/     \/              \/     \/     \/                       \/            \/     \/    \/    \/     \/     \/ 
        bool requestSecurityAccess0x27(int seedkey0, int seedkey1, int seedkey2, int seedkey3, int seedkey4)
        {
            if (elm327Flag == true)
            {
                var num = 0;
                Log("[0x27 reqSecurityAccess]\r\n");
                
                waitfor = 0x67;
                Write("2701\r");
                if (timeout == 0)
                {
                    delayloop(250);
                    int sidByte = rxBuf[1];
                    switch (sidByte)
                    {
                        case 0x67:
                            var num22 = 0;
                            num22 += (int)rxBuf[3] << 0x10;
                            num22 += (int)rxBuf[4] << 8;
                            num22 += (int)rxBuf[5];
                            var s = "2702" + KeyGenMkI(num22, seedkey0, seedkey1, seedkey2, seedkey3, seedkey4).ToString("X6") + "\r";
                            Write(s);
                            if (timeout == 0)
                            {
                                delayloop(250);
                                int sidByte2 = rxBuf[1];
                                switch (sidByte2)
                                {
                                    case 0x67:
                                        Log($"{textBoxEcu.Text} Security Access Granted...\r\n");
                                        return true;
                                    case 0x7F:
                                        Log($"{textBoxEcu.Text} Security Access Failed...\r\n");
                                        return false;
                                    default:
                                        Log($"No Response from {textBoxEcu.Text} ...\r\n");
                                        return false;
                                }
                            }
                            break;
                        case 0x7F:
                            Log($"{textBoxEcu.Text} Security Access Failed...\r\n");
                            Enabled = true;
                            return false;
                        case 0x00:
                            delayloop(250);
                            Log($"No Response from {textBoxEcu.Text} ...\r\n");
                            return false;
                    }
                }
                return false;               
            }
            if (j2534Flag == true)
            {
                try
                {
                    var num = 0;
                    Log("Service: [0x27 reqSecurityAccess]\r\n");
                    
                    byte[] requestSecurityAccess = new byte[] { 0, 0, ecuRxIdentifier1, ecuRxIdentifier2, serviceRequestSecurityAccess, requestLevelOneSeed };
                    string requestSecurityAccessMsg = sendPassThruMsg(requestSecurityAccess);
                    // parse response and build seed key algo into flow...
                    //00  00  07  AE  67  01  AF  BB  7F 
                    string responseData = requestSecurityAccessMsg.Replace(" ", "");//remove whitespaces from the string
                    string responseData1 = responseData.Substring(8, 2); //to grab the positive or negative response byte as a string
                    string responseErr = responseData.Substring(12, 2); // to grab the error code if we get a 0x7F response
                    int responseSec = int.Parse(responseData1, System.Globalization.NumberStyles.HexNumber);// convert the string to an int for the switch
                    switch (responseSec)
                    {
                        case 0x67:
                            Log($"{textBoxEcu.Text} Recieved Security Seed.\r\n");
                            
                            string seed = responseData.Substring(12, 6);
                            Log($"{textBoxEcu.Text}  Security Seed: " + seed + "\r\n");
                            
                            string rxbuf3 = responseData.Substring(12, 2);
                            string rxbuf4 = responseData.Substring(14, 2);
                            string rxbuf5 = responseData.Substring(16, 2);
                            int buf3 = Convert.ToInt32(rxbuf3, 16);
                            int buf4 = Convert.ToInt32(rxbuf4, 16);
                            int buf5 = Convert.ToInt32(rxbuf5, 16);
                            var num22 = 0;
                            num22 += buf3 << 0x10;
                            num22 += buf4 << 8;
                            num22 += buf5;
                            Log($"{textBoxEcu.Text} Calculating Response.. \r\n");
                            
                            string responseKey = KeyGenMkI(num22, seedkey0, seedkey1, seedkey2, seedkey3, seedkey4).ToString("X6");
                            string response1 = responseKey.Substring(0, 2);
                            string response2 = responseKey.Substring(2, 2);
                            string response3 = responseKey.Substring(4, 2);
                            byte responseByte1 = Convert.ToByte(response1, 16);
                            byte responseByte2 = Convert.ToByte(response2, 16);
                            byte responseByte3 = Convert.ToByte(response3, 16);
                            byte[] requestSecurityAccess02 = new byte[] { 0, 0, ecuRxIdentifier1, ecuRxIdentifier2, serviceRequestSecurityAccess, sendLevelOneKey, responseByte1, responseByte2, responseByte3 };
                            string requestSecurityAccess02Msg = sendPassThruMsg(requestSecurityAccess02);
                            string responseDataA = requestSecurityAccess02Msg.Replace(" ", "");
                            string responseDataB = responseDataA.Substring(8, 2);
                            //  00  00  07  2F  67  02  
                            int response = int.Parse(responseDataB, System.Globalization.NumberStyles.HexNumber);
                            switch (response)
                            {
                                case 0x67:
                                    Log($"{textBoxEcu.Text} Security Access Granted.\r\n");
                                    
                                    return true;
                                case 0x7F:
                                    string responseDataBErr = responseDataA.Substring(12, 2);
                                    Log($"{textBoxEcu.Text} Security Access Denied. \r\n");
                                    
                                    int responseErr2 = Convert.ToInt32(responseDataBErr, 16);
                                    Log(printerr(responseErr2)); // printing the error code definition from printerr(int)
                                    
                                    //Commented this out because it is breaking the security accesss bruteforcer 17/06/2024
                                    //System.Windows.Forms.MessageBox.Show(printerr(responseErr2), "FoA Orion Comms - 0x27 requestSecurityAccess", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);      
                                    return false;
                            }
                            break;
                        case 0x7F:
                            Log($"{textBoxEcu.Text} Security Access Failed \r\n");
                            
                            int responseErr1 = Convert.ToInt32(responseErr, 16); // coverting response2 string to an int
                            Log(printerr(responseErr1)); // printing the error code definition from printerr(int)       
                            
                            //System.Windows.Forms.MessageBox.Show(printerr(responseErr1), "FoA Orion Comms - 0x27 requestSecurityAccess", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                            return false;
                    }
                }
                catch (Exception ex)
                {
                    // Catching any exception of type Exception
                    // Handle the exception here, such as logging or displaying an error message
                    Log($"{textBoxEcu.Text} error occurred: " + ex.Message);
                    
                }
                return false;
            }
            return false;
        }   
         bool requestDownload(byte[] moduleSpecificMessage)
        {
            Log("[0x34 requestDownload]\r\n");
            if (elm327Flag == true)
            {
                System.Windows.Forms.MessageBox.Show("This functionality is not available with ELM327 interfaces.", "FoA Orion Comms - 0x34 requestDownload", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                return false;
            }
            if (j2534Flag == true)
            {
                try
                {
                    byte[] requestDownload = moduleSpecificMessage;
                    string requestDownloadMsg = sendPassThruMsg(requestDownload);
                    requestDownloadMsg = requestDownloadMsg.Replace(" ", "");
                    string response1 = requestDownloadMsg.Substring(8, 2); // for the protocol response byte
                    string response2 = requestDownloadMsg.Substring(12, 2); // for the negative response code error definition
                    // 00  00  07  AE  7F  35  11
                    int response = Convert.ToInt32(response1, 16);
                    switch (response)
                    {
                        case 0x74:
                            Log("Request Download Success.\r\n");
                            return true;
                        case 0x7F:
                            int responseErr = Convert.ToInt32(response2, 16);
                            Log("Request Download Pending.\r\n");
                            Log(printerr(responseErr));
                            if(responseErr == 0x78)
                            {
                                bool SearchForResponse = true;
                                while (SearchForResponse == true)
                                {
                                    pending:
                                        int NumReadMsgs = 1;
                                        IntPtr MyRXMsg = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(PassThruMsg)) * NumReadMsgs);
                                        var ErrorResult = J2534Port.Functions.PassThruReadMsgs((int)ChannelID, MyRXMsg, ref NumReadMsgs, 20); //this is your timeout here.
                                        if (ErrorResult != J2534Err.STATUS_NOERROR) //if no frames received, it goes here.
                                        {
                                            Log(ErrorResult.ToString() + "\r\n");
                                            Log("Failed to read PassThru Msg \r\n");
                                            //Shits fucked, fauled reading!!!
                                            break;
                                        }
                                        else
                                        {
                                            //Log("PassThru Read Msg Success \r\n");
                                        }
                                        //Convert the memory pointer back to a PassThruMsg Object
                                        PassThruMsg FoundFrame = MyRXMsg.AsMsgList(1).Last();
                                        if (((int)FoundFrame.RxStatus == ((int)J2534.RxStatus.TX_INDICATION_SUCCESS ^ (int)J2534.RxStatus.TX_MSG_TYPE)) ||
                                            ((int)FoundFrame.RxStatus == ((int)J2534.RxStatus.TX_INDICATION_SUCCESS ^ (int)J2534.RxStatus.TX_MSG_TYPE ^ (int)J2534.RxStatus.ISO15765_ADDR_TYPE)) ||
                                            ((int)FoundFrame.RxStatus == ((int)J2534.RxStatus.START_OF_MESSAGE))
                                            )
                                        {
                                            //We dont want any of this, continue!
                                            Marshal.FreeHGlobal(MyRXMsg);
                                            continue;
                                        }
                                        Marshal.FreeHGlobal(MyRXMsg);
                                        //This should have our bytes!
                                        byte[] MyRXDBytes = FoundFrame.GetBytes();
                                        string DataToString = "";
                                        for (int i = 0; i < MyRXDBytes.Length; i++)
                                        {
                                            DataToString += MyRXDBytes[i].ToString("X2") + "  ";
                                        }
                                        DataToString = DataToString.Replace(" ", "");
                                        string protocolResponse = DataToString.Substring(8, 2);
                                        string errorResponseCode = DataToString.Substring(12, 2);
                                        if(protocolResponse == "74");
                                        {
                                            return true;
                                        }
                                        if(protocolResponse == "7F")
                                        {
                                            if(errorResponseCode == "78");
                                            goto pending;
                                        }
                                        else
                                        {
                                            return false;
                                        }
                                }
                                
                            }
                            return false;
                            
                            //System.Windows.Forms.MessageBox.Show(printerr(responseErr), "FoA Orion Comms - 0x34 requestDownload", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                    }
                    test:
                        Thread.Sleep(5000);
                        return false;
                }
                catch (Exception ex)
                {
                    Log("0x34 requestDownload Error: " + ex.Message);
                    return false;
                }
            }
            return false;
        }
    }
}
