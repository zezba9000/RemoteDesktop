using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace WinSound
{
    /// <summary>
    /// Player
    /// </summary>
    unsafe public class Player
    {
        /// <summary>
        /// Konstruktor
        /// </summary>
        public Player()
        {

            delegateWaveOutProc = new Win32.DelegateWaveOutProc(waveOutProc);
        }

        //Attribute
        private LockerClass Locker = new LockerClass();
        private LockerClass LockerCopy = new LockerClass();
        private IntPtr hWaveOut = IntPtr.Zero;
        private String WaveOutDeviceName = "";
        private bool IsWaveOutOpened = false;
        private bool IsThreadPlayWaveOutRunning = false;
        private bool IsClosed = false;
        private bool IsPaused = false;
        private bool IsStarted = false;
        private bool IsBlocking = false;
        private int SamplesPerSecond = 8000;
        private int BitsPerSample = 16;
        private int Channels = 1;
        private int BufferCount = 8;
        private int BufferLength = 1024;
        private Win32.WAVEHDR*[] WaveOutHeaders;
        private Win32.DelegateWaveOutProc delegateWaveOutProc;
        private System.Threading.Thread ThreadPlayWaveOut;
        private System.Threading.AutoResetEvent AutoResetEventDataPlayed = new System.Threading.AutoResetEvent(false);

        //Delegates bzw. Events
        public delegate void DelegateStopped();
        public event DelegateStopped PlayerClosed;
        public event DelegateStopped PlayerStopped;

        /// <summary>
        /// Paused
        /// </summary>
        public bool Paused
        {
            get
            {
                return IsPaused;
            }
        }
        /// <summary>
        /// Opened
        /// </summary>
        public bool Opened
        {
            get
            {
                return IsWaveOutOpened & IsClosed == false;
            }
        }
        /// <summary>
        /// Playing
        /// </summary>
        public bool Playing
        {
            get
            {
                if (Opened && IsStarted)
                {
                    foreach (Win32.WAVEHDR* pHeader in WaveOutHeaders)
                    {
                        if (IsHeaderInqueue(*pHeader))
                        {
                            return true;
                        }
                    }
                }
                return false;
            }
        }
        /// <summary>
        /// CreateWaveOutHeaders
        /// </summary>
        /// <returns></returns>
        private bool CreateWaveOutHeaders()
        {
            //Buffer anlegen
            WaveOutHeaders = new Win32.WAVEHDR*[BufferCount];
            int createdHeaders = 0;

            //Für jeden Buffer
            for (int i = 0; i < BufferCount; i++)
            {
                //Header allokieren
                WaveOutHeaders[i] = (Win32.WAVEHDR*)Marshal.AllocHGlobal(sizeof(Win32.WAVEHDR));

                //Header setzen
                WaveOutHeaders[i]->dwLoops = 0;
                WaveOutHeaders[i]->dwUser = IntPtr.Zero;
                WaveOutHeaders[i]->lpNext = IntPtr.Zero;
                WaveOutHeaders[i]->reserved = IntPtr.Zero;
                WaveOutHeaders[i]->lpData = Marshal.AllocHGlobal(BufferLength);
                WaveOutHeaders[i]->dwBufferLength = (uint)BufferLength;
                WaveOutHeaders[i]->dwBytesRecorded = 0;
                WaveOutHeaders[i]->dwFlags = 0;
  
                //Wenn der Buffer vorbereitet werden konnte
                Win32.MMRESULT hr = Win32.waveOutPrepareHeader(hWaveOut, WaveOutHeaders[i], sizeof(Win32.WAVEHDR));
                if (hr == Win32.MMRESULT.MMSYSERR_NOERROR)
                {
                    createdHeaders++;
                }
            }

            //Fertig
            return (createdHeaders == BufferCount);
        }
        /// <summary>
        /// FreeWaveInHeaders
        /// </summary>
        private void FreeWaveOutHeaders()
        {
            try
            {
                if (WaveOutHeaders != null)
                {
                    for (int i = 0; i < WaveOutHeaders.Length; i++)
                    {
                        //Handles freigeben
                        Win32.MMRESULT hr = Win32.waveOutUnprepareHeader(hWaveOut, WaveOutHeaders[i], sizeof(Win32.WAVEHDR));

                        //Warten bis fertig abgespielt
                        int count = 0;
                        while(count <= 100 && (WaveOutHeaders[i]->dwFlags & Win32.WaveHdrFlags.WHDR_INQUEUE) == Win32.WaveHdrFlags.WHDR_INQUEUE)
                        {
                            System.Threading.Thread.Sleep(20);
                            count++;
                        }

                        //Wenn Daten abgespielt  
                        if ((WaveOutHeaders[i]->dwFlags & Win32.WaveHdrFlags.WHDR_INQUEUE) != Win32.WaveHdrFlags.WHDR_INQUEUE)
                        {
                            //Daten freigeben
                            if (WaveOutHeaders[i]->lpData != IntPtr.Zero)
                            {
                                Marshal.FreeHGlobal(WaveOutHeaders[i]->lpData);
                                WaveOutHeaders[i]->lpData = IntPtr.Zero;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.Write(ex.Message);
            }
        }
        /// <summary>
        /// StartThreadRecording
        /// </summary>
        private void StartThreadPlayWaveOut()
        {
            if (IsThreadPlayWaveOutRunning == false)
            {
                ThreadPlayWaveOut = new System.Threading.Thread(new System.Threading.ThreadStart(OnThreadPlayWaveOut));
                IsThreadPlayWaveOutRunning = true;
                ThreadPlayWaveOut.Name = "PlayWaveOut";
                ThreadPlayWaveOut.Priority = System.Threading.ThreadPriority.Highest;
                ThreadPlayWaveOut.Start();
            }
        }
        /// <summary>
        /// PlayBytes. Bytes in gleich grosse Stücke teilen und einzeln abspielen
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        private bool PlayBytes(Byte[] bytes)
        {
            if (bytes.Length > 0)
            {
                //Grösse der Bytestücke 
                int byteSize = bytes.Length / BufferCount;
                int currentPos = 0;

                //Für jeden möglichen Buffer
                for (int count = 0; count < BufferCount; count++)
                {
                    //Nächsten freien Buffer ermitteln
                    int index = GetNextFreeWaveOutHeaderIndex();
                    if (index != -1)
                    {
                        try
                        {
                            //Teilstück kopieren
                            Byte[] partByte = new Byte[byteSize];
                            Array.Copy(bytes, currentPos, partByte, 0, byteSize);
                            currentPos += byteSize;

                            //Wenn unterschiedliche Datengrösse
                            if (WaveOutHeaders[index]->dwBufferLength != partByte.Length)
                            {
                                //Datenspeicher neu anlegen
                                Marshal.FreeHGlobal(WaveOutHeaders[index]->lpData);
                                WaveOutHeaders[index]->lpData = Marshal.AllocHGlobal(partByte.Length);
                                WaveOutHeaders[index]->dwBufferLength = (uint)partByte.Length;
                            }

                            //Daten kopieren
                            WaveOutHeaders[index]->dwUser = (IntPtr)index;
                            Marshal.Copy(partByte, 0, WaveOutHeaders[index]->lpData, partByte.Length);
                        }
                        catch (Exception ex)
                        {
                            //Fehler beim Kopieren
                            System.Diagnostics.Debug.WriteLine(String.Format("CopyBytesToFreeWaveOutHeaders() | {0}", ex.Message));
                            AutoResetEventDataPlayed.Set();
                            return false;
                        }

                        //Wenn noch geöffnet
                        if (hWaveOut != null)
                        {
                            //Abspielen
                            Win32.MMRESULT hr = Win32.waveOutWrite(hWaveOut, WaveOutHeaders[index], sizeof(Win32.WAVEHDR));
                            if (hr != Win32.MMRESULT.MMSYSERR_NOERROR)
                            {
                                //Fehler beim Abspielen
                                AutoResetEventDataPlayed.Set();
                                return false;
                            }
                        }
                        else
                        {
                            //WaveOut ungültig
                            return false;
                        }
                    }
                    else
                    {
                        //Nicht genügend freie Buffer vorhanden
                        return false;
                    }
                }
                return true;
            }
            //Keine Daten vorhanden
            return false;
        }
        /// <summary>
        /// OpenWaveOuz
        /// </summary>
        /// <returns></returns>
        private bool OpenWaveOut()
        {
            if (hWaveOut == IntPtr.Zero)
            {
                //Wenn nicht schon offen
                if (IsWaveOutOpened == false)
                {
                    //Format bestimmen
                    Win32.WAVEFORMATEX waveFormatEx = new Win32.WAVEFORMATEX();
                    waveFormatEx.wFormatTag = (ushort)Win32.WaveFormatFlags.WAVE_FORMAT_PCM;
                    waveFormatEx.nChannels = (ushort)Channels;
                    waveFormatEx.nSamplesPerSec = (ushort)SamplesPerSecond;
                    waveFormatEx.wBitsPerSample = (ushort)BitsPerSample;
                    waveFormatEx.nBlockAlign = (ushort)((waveFormatEx.wBitsPerSample * waveFormatEx.nChannels) >> 3);
                    waveFormatEx.nAvgBytesPerSec = (uint)(waveFormatEx.nBlockAlign * waveFormatEx.nSamplesPerSec);

                    //WaveOut Gerät ermitteln
                    int deviceId = WinSound.GetWaveOutDeviceIdByName(WaveOutDeviceName);
                    //WaveIn Gerät öffnen
                    Win32.MMRESULT hr = Win32.waveOutOpen(ref hWaveOut, deviceId, ref waveFormatEx, delegateWaveOutProc, 0, (int)Win32.WaveProcFlags.CALLBACK_FUNCTION);

                    //Wenn nicht erfolgreich
                    if (hr != Win32.MMRESULT.MMSYSERR_NOERROR)
                    {
                        IsWaveOutOpened = false;
                        return false;
                    }

                    //Handle sperren
                    GCHandle.Alloc(hWaveOut, GCHandleType.Pinned);
                }
            }

            IsWaveOutOpened = true;
            return true;
        }
        /// <summary>
        ///Open
        /// </summary>
        /// <param name="waveInDeviceName"></param>
        /// <param name="waveOutDeviceName"></param>
        /// <param name="samplesPerSecond"></param>
        /// <param name="bitsPerSample"></param>
        /// <param name="channels"></param>
        /// <returns></returns>
        public bool Open(string waveOutDeviceName, int samplesPerSecond, int bitsPerSample, int channels, int bufferCount)
        {
            try
            {
                lock (Locker)
                {
                    //Wenn nicht schon geöffnet
                    if (Opened == false)
                    {

                        //Daten übernehmen
                        WaveOutDeviceName = waveOutDeviceName;
                        SamplesPerSecond = samplesPerSecond;
                        BitsPerSample = bitsPerSample;
                        Channels = channels;
                        BufferCount = Math.Max(bufferCount, 1);

                        //Wenn WaveOut geöffnet werden konnte
                        if (OpenWaveOut())
                        {
                            //Wenn alle Buffer erzeugt werden konnten
                            if (CreateWaveOutHeaders())
                            {
                                //Thread starten
                                StartThreadPlayWaveOut();
                                IsClosed = false;
                                return true;
                            }
                        }
                    }

                    //Schon geöffnet
                    return false;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(String.Format("Start | {0}", ex.Message));
                return false;
            }
        }
        /// <summary>
        /// PlayData
        /// </summary>
        /// <param name="datas"></param>
        /// <param name="isBlocking"></param>
        /// <returns></returns>
        public bool PlayData(Byte[] datas, bool isBlocking)
        {
            try
            {
                if (Opened)
                {
                    int index = GetNextFreeWaveOutHeaderIndex();
                    if (index != -1)
                    {
                        //Werte übernehmen
                        this.IsBlocking = isBlocking;

                        //Wenn unterschiedliche Datengrösse
                        if (WaveOutHeaders[index]->dwBufferLength != datas.Length)
                        {
                            //Datenspeicher neu anlegen
                            Marshal.FreeHGlobal(WaveOutHeaders[index]->lpData);
                            WaveOutHeaders[index]->lpData = Marshal.AllocHGlobal(datas.Length);
                            WaveOutHeaders[index]->dwBufferLength = (uint)datas.Length;
                        }

                        //Daten kopieren
                        WaveOutHeaders[index]->dwBufferLength = (uint)datas.Length;
                        WaveOutHeaders[index]->dwUser = (IntPtr)index;
                        Marshal.Copy(datas, 0, WaveOutHeaders[index]->lpData, datas.Length);

                        //Abspielen
                        this.IsStarted = true;
                        Win32.MMRESULT hr = Win32.waveOutWrite(hWaveOut, WaveOutHeaders[index], sizeof(Win32.WAVEHDR));
                        if (hr == Win32.MMRESULT.MMSYSERR_NOERROR)
                        {
                            //Wenn blockierend
                            if (isBlocking)
                            {
                                AutoResetEventDataPlayed.WaitOne();
                                AutoResetEventDataPlayed.Set();
                            }
                            return true;
                        }
                        else
                        {
                            //Fehler beim Abspielen
                            AutoResetEventDataPlayed.Set();
                            return false;
                        }
                    }
                    else
                    {
                        //Kein freier Ausgabebuffer vorhanden
                        System.Diagnostics.Debug.WriteLine(String.Format("No free WaveOut Buffer found | {0}", DateTime.Now.ToLongTimeString()));
                        return false;
                    }
                }
                else
                {
                    //Nicht geöffnet
                    return false;
                }

            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(String.Format("PlayData | {0}", ex.Message));
                return false;
            }
        }
        /// <summary>
        /// PlayFile (Wave Files)
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public bool PlayFile(string fileName, string waveOutDeviceName)
        {
            lock (Locker)
            {
                try
                {
                    //WaveFile auslesen
                    WaveFileHeader header = WaveFile.Read(fileName);

                    //Wenn Daten vorhanden
                    if (header.Payload.Length > 0)
                    {
                        //Wenn geöffnet
                        if (Open(waveOutDeviceName, (int)header.SamplesPerSecond, (int)header.BitsPerSample, (int)header.Channels, 8))
                        {
                            int index = GetNextFreeWaveOutHeaderIndex();
                            if (index != -1)
                            {
                                //Bytes Teilweise in Ausgabebuffer abspielen
                                this.IsStarted = true;
                                return PlayBytes(header.Payload);
                            }
                            else
                            {
                                //Kein freier Ausgabebuffer vorhanden
                                AutoResetEventDataPlayed.Set();
                                return false;
                            }
                        }
                        else
                        {
                            //Nicht geöffnet
                            AutoResetEventDataPlayed.Set();
                            return false;
                        }
                    }
                    else
                    {
                        //Fehlerhafte Datei
                        AutoResetEventDataPlayed.Set();
                        return false;
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(String.Format("PlayFile | {0}", ex.Message));
                    AutoResetEventDataPlayed.Set();
                    return false;
                }
            }
        }
        /// <summary>
        /// Close
        /// </summary>
        /// <returns></returns>
        public bool Close()
        {
            try
            {
                lock (Locker)
                {
                    //Wenn geöffnet
                    if (Opened)
                    {
                        //Als manuel beendet setzen
                        IsClosed = true;

                        //Warten bis alle Daten fertig abgespielt
                        int count = 0;
                        while (Win32.waveOutReset(hWaveOut) != Win32.MMRESULT.MMSYSERR_NOERROR && count <= 100)
                        {
                            System.Threading.Thread.Sleep(50);
                            count++;
                        }

                        //Header und Daten freigeben
                        FreeWaveOutHeaders();

                        //Warten bis alle Daten fertig abgespielt
                        count = 0;
                        while (Win32.waveOutClose(hWaveOut) != Win32.MMRESULT.MMSYSERR_NOERROR && count <= 100)
                        {
                            System.Threading.Thread.Sleep(50);
                            count++;
                        }

                        //Variablen setzen
                        IsWaveOutOpened = false;
                        AutoResetEventDataPlayed.Set();
                        return true;
                    }
                    return false;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(String.Format("Close | {0}", ex.Message));
                return false;
            }
        }
        /// <summary>
        /// StartPause
        /// </summary>
        /// <returns></returns>
        public bool StartPause()
        {
            try
            {
                lock (Locker)
                {
                    //Wenn geöffnet
                    if (Opened)
                    {
                        //Wenn nicht schon pausiert
                        if (IsPaused == false)
                        {
                            //Pausieren
                            Win32.MMRESULT hr = Win32.waveOutPause(hWaveOut);
                            if (hr == Win32.MMRESULT.MMSYSERR_NOERROR)
                            {
                                //Speichern
                                IsPaused = true;
                                AutoResetEventDataPlayed.Set();
                                return true;
                            }
                        }
                    }
                    return false;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(String.Format("StartPause | {0}", ex.Message));
                return false;
            }
        }
        /// <summary>
        /// EndPause
        /// </summary>
        /// <returns></returns>
        public bool EndPause()
        {
            try
            {
                lock (Locker)
                {
                    //Wenn geöffnet
                    if (Opened)
                    {
                        //Wenn pausiert
                        if (IsPaused)
                        {
                            //Pausieren
                            Win32.MMRESULT hr = Win32.waveOutRestart(hWaveOut);
                            if (hr == Win32.MMRESULT.MMSYSERR_NOERROR)
                            {
                                //Speichern
                                IsPaused = false;
                                AutoResetEventDataPlayed.Set();
                                return true;
                            }
                        }
                    }
                    return false;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(String.Format("EndPause | {0}", ex.Message));
                return false;
            }
        }
        /// <summary>
        /// GetNextFreeWaveOutHeaderIndex
        /// </summary>
        /// <returns></returns>
        private int GetNextFreeWaveOutHeaderIndex()
        {
            for (int i = 0; i < WaveOutHeaders.Length; i++)
            {
                if (IsHeaderPrepared(*WaveOutHeaders[i]) && !IsHeaderInqueue(*WaveOutHeaders[i]))
                {
                    return i;
                }
            }
            return -1;
        }
        /// <summary>
        /// IsHeaderPrepared
        /// </summary>
        /// <param name="flags"></param>
        /// <returns></returns>
        private bool IsHeaderPrepared(Win32.WAVEHDR header)
        {
            return (header.dwFlags & Win32.WaveHdrFlags.WHDR_PREPARED) > 0;
        }
        /// <summary>
        /// IsHeaderInqueue
        /// </summary>
        /// <param name="flags"></param>
        /// <returns></returns>
        private bool IsHeaderInqueue(Win32.WAVEHDR header)
        {
            return (header.dwFlags & Win32.WaveHdrFlags.WHDR_INQUEUE) > 0;
        }
        /// <summary>
        /// waveOutProc
        /// </summary>
        /// <param name="hWaveOut"></param>
        /// <param name="msg"></param>
        /// <param name="dwInstance"></param>
        /// <param name="wParam"></param>
        /// <param name="lParam"></param>
        private void waveOutProc(IntPtr hWaveOut, Win32.WOM_Messages msg, IntPtr dwInstance, Win32.WAVEHDR* pWaveHeader, IntPtr lParam)
        {
            try
            {
                switch (msg)
                {
                    //Open
                    case Win32.WOM_Messages.OPEN:
                        break;

                    //Done
                    case Win32.WOM_Messages.DONE:
                        //Vermerken das Daten ankommen
                        IsStarted = true;
                        AutoResetEventDataPlayed.Set();
                        break;

                    //Close
                    case Win32.WOM_Messages.CLOSE:
                        IsStarted = false;
                        IsWaveOutOpened = false;
                        IsPaused = false;
                        IsClosed = true;
                        AutoResetEventDataPlayed.Set();
                        this.hWaveOut = IntPtr.Zero;
                        break;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(String.Format("Player.cs | waveOutProc() | {0}", ex.Message));
                AutoResetEventDataPlayed.Set();
            }
        }
        /// <summary>
        /// OnThreadRecording
        /// </summary>
        private void OnThreadPlayWaveOut()
        {
            while (Opened && !IsClosed)
            {
                //Warten bis Aufnahme beendet
                AutoResetEventDataPlayed.WaitOne();

                lock (Locker)
                {
                    if (Opened && !IsClosed)
                    {
                        //Variablen setzen
                        IsThreadPlayWaveOutRunning = true;

                        //Wenn keine Daten mehr abgespielt werden
                        if (!Playing)
                        {
                            //Wenn Daten abgespielt wurden
                            if (IsStarted)
                            {
                                IsStarted = false;
                                //Ereignis absenden
                                if (PlayerStopped != null)
                                {
                                    try
                                    {
                                        PlayerStopped();
                                    }
                                    catch (Exception ex)
                                    {
                                        System.Diagnostics.Debug.WriteLine(String.Format("Player Stopped | {0}", ex.Message));
                                    }
                                    finally
                                    {
                                        AutoResetEventDataPlayed.Set();
                                    }
                                }
                            }
                        }
                    }
                }

                //Wenn blockierend
                if (IsBlocking)
                {
                    AutoResetEventDataPlayed.Set();
                }
            }

            lock (Locker)
            {
                //Variablen setzen
                IsThreadPlayWaveOutRunning = false;
            }

            //Ereignis aussenden
            if (PlayerClosed != null)
            {
                try
                {
                    PlayerClosed();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(String.Format("Player Closed | {0}", ex.Message));
                }
            }
        }
    }
}
