using NAudio.MediaFoundation;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;

namespace RemoteDesktop.Server.XamaOK
{

    //public class StreamWrapper : IStream
    //{
    //  public StreamWrapper(Stream stream)
    //  {
    //    if (stream == null)
    //      throw new ArgumentNullException("stream", "Can't wrap null stream.");
    //    this.stream = stream;
    //  }

    //  private Stream stream;

    //  public void Read(byte[] pv, int cb, System.IntPtr pcbRead)
    //  {
    //    Marshal.WriteInt32(pcbRead, (Int32)stream.Read(pv, 0, cb));
    //  }

    //  public void Seek(long dlibMove, int dwOrigin, System.IntPtr plibNewPosition)
    //  {
    //    Marshal.WriteInt32(plibNewPosition, (int) stream.Seek(dlibMove, (SeekOrigin)dwOrigin));
    //  }

    //    public void Write(byte[] pv, int cb, IntPtr pcbWritten)
    //    {
    //    }

    //    public void SetSize(long libNewSize)
    //    {
    //    }

    //    public void CopyTo(IStream pstm, long cb, IntPtr pcbRead, IntPtr pcbWritten)
    //    {
    //    }

    //    public void Commit(int grfCommitFlags)
    //    {
    //    }

    //    public void Revert()
    //    {
    //    }

    //    public void LockRegion(long libOffset, long cb, int dwLockType)
    //    {
    //    }

    //    public void UnlockRegion(long libOffset, long cb, int dwLockType)
    //    {
    //    }

    //    public void Stat(out System.Runtime.InteropServices.ComTypes.STATSTG pstatstg, int grfStatFlag)
    //    {
    //        System.Runtime.InteropServices.ComTypes.STATSTG dummy;
    //        pstatstg = dummy;

    //    }

    //    public void Clone(out IStream ppstm)
    //    {
    //    }
    //}


    class SoundEncodeUtil
    {

        public static int[] GetEncodeBitrates(Guid audioSubtype, int sampleRate, int channels)
        {
            var bitRates = new HashSet<int>();
            IMFCollection availableTypes;
            MediaFoundationInterop.MFTranscodeGetAudioOutputAvailableTypes(
                audioSubtype, _MFT_ENUM_FLAG.MFT_ENUM_FLAG_ALL, null, out availableTypes);
            int count;
            availableTypes.GetElementCount(out count);
            for (int n = 0; n < count; n++)
            {
                object mediaTypeObject;
                availableTypes.GetElement(n, out mediaTypeObject);
                var mediaType = (IMFMediaType)mediaTypeObject;

                // filter out types that are for the wrong sample rate and channels
                int samplesPerSecond;
                mediaType.GetUINT32(MediaFoundationAttributes.MF_MT_AUDIO_SAMPLES_PER_SECOND, out samplesPerSecond);
                if (sampleRate != samplesPerSecond)
                    continue;
                int channelCount;
                mediaType.GetUINT32(MediaFoundationAttributes.MF_MT_AUDIO_NUM_CHANNELS, out channelCount);
                if (channels != channelCount)
                    continue;

                int bytesPerSecond;
                mediaType.GetUINT32(MediaFoundationAttributes.MF_MT_AUDIO_AVG_BYTES_PER_SECOND, out bytesPerSecond);
                bitRates.Add(bytesPerSecond * 8);
                Marshal.ReleaseComObject(mediaType);
            }
            Marshal.ReleaseComObject(availableTypes);
            return bitRates.ToArray();
        }

        public static long BytesToNsPosition(int bytes, WaveFormat waveFormat)
        {
            long nsPosition = (10000000L * bytes) / waveFormat.AverageBytesPerSecond;
            return nsPosition;
        }


        public static long ConvertOneBuffer(IMFSinkWriter writer, int streamIndex, IWaveProvider inputProvider, long position, byte[] managedBuffer)
        {
            long durationConverted = 0;
            int maxLength;
            IMFMediaBuffer buffer = MediaFoundationApi.CreateMemoryBuffer(managedBuffer.Length);
            buffer.GetMaxLength(out maxLength);

            IMFSample sample = MediaFoundationApi.CreateSample();
            sample.AddBuffer(buffer);

            IntPtr ptr;
            int currentLength;
            buffer.Lock(out ptr, out maxLength, out currentLength);
            int read = inputProvider.Read(managedBuffer, 0, maxLength);
            if (read > 0)
            {
                durationConverted = BytesToNsPosition(read, inputProvider.WaveFormat);
                Marshal.Copy(managedBuffer, 0, ptr, read);
                buffer.SetCurrentLength(read);
                buffer.Unlock();
                sample.SetSampleTime(position);
                sample.SetSampleDuration(durationConverted);
                writer.WriteSample(streamIndex, sample);
                //writer.Flush(streamIndex);
            }
            else
            {
                buffer.Unlock();
            }

            Marshal.ReleaseComObject(sample);
            Marshal.ReleaseComObject(buffer);
            return durationConverted;
        }

        public static byte[] encodePCMtoMP3(IWaveProvider waveIn)
        {
            //            var ret = SoundEncodeUtil.GetEncodeBitrates(AudioSubtypes.MFAudioFormat_AAC, 8 * 1000, 1);
            //            var ret = SoundEncodeUtil.GetEncodeBitrates(AudioSubtypes.MFAudioFormat_MP3, 8 * 1000, 1);
            //MediaFoundationEncoder.EncodeToAac()
            //IMFSinkWriter writer;
            //IMFByteStream bs = MediaFoundationApi.CreateByteStream(new MemoryStream(new byte[1024]));
            //IMFByteStream bs = MediaFoundationApi.CreateByteStream(new StreamWrapper(new MemoryStream(new byte[1024])));


            //if (!System.IO.File.Exists(".\\mmap.dat"))
            //{
            //    FileStream fs = new FileStream(".\\mmap.dat", FileMode.Open);
            //    byte[] empty_buf = new byte[4 * 1024];
            //    Array.Clear(empty_buf, 0, empty_buf.Length);
            //    fs.Write(empty_buf, 0, empty_buf.Length);
            //}

            //  MemoryMappedFile mmf =
            //    MemoryMappedFile.CreateFromFile(".\\mmap.dat");
            //  MemoryMappedViewAccessor accessor =
            //    mmf.CreateViewAccessor();

            //var mediaType = MediaFoundationEncoder.SelectMediaType(
            //    AudioSubtypes.MFAudioFormat_MP3,
            //    new WaveFormat(8000, 1),
            //    0
            //);
            //MediaFoundationEncoder encoder = new MediaFoundationEncoder(mediaType);

            MediaFoundationEncoder.EncodeToMp3(waveIn, "F:\\work\\tmp\\tmp.mp3");
            Mp3FileReader reader = new Mp3FileReader("F:\\work\\tmp\\tmp.mp3");
            MemoryStream ms = new MemoryStream();
            Mp3Frame ret;
            while((ret = reader.ReadNextFrame()) != null)
            {
                ms.Write(ret.RawData, 0, ret.FrameLength);
            }
            reader.Close();
            reader.Dispose();

            return ms.ToArray();
            //MediaFoundationInterop.MFCreateSinkWriterFromURL(null, bs, null, out writer);
        }
    }
}
