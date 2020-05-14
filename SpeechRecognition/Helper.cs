using System;
using System.Diagnostics;
using System.IO;
using Microsoft.CognitiveServices.Speech.Audio;

namespace SpeechRecognition
{
    /// <summary>
    /// The classes in this file are originally from https://github.com/Azure-Samples/cognitive-services-speech-sdk
    /// </summary>
    public class SpeechUtils
    {
        public static AudioConfig OpenWavFile(string filename)
        {
            var reader = new BinaryReader(File.OpenRead(filename));
            return OpenWavFile(reader);
        }

        public static AudioConfig OpenWavFile(BinaryReader reader)
        {
            var format = ReadWaveHeader(reader);
            return AudioConfig.FromStreamInput(new BinaryAudioStreamReader(reader), format);
        }

        public static BinaryAudioStreamReader CreateWavReader(string filename)
        {
            var reader = new BinaryReader(File.OpenRead(filename));
            // read the wave header so that it won't get into the in the following readings
            var format = ReadWaveHeader(reader);
            return new BinaryAudioStreamReader(reader);
        }

        public static AudioStreamFormat ReadWaveHeader(BinaryReader reader)
        {
            // Tag "RIFF"
            var data = new char[4];
            reader.Read(data, 0, 4);
            Trace.Assert(data[0] == 'R' && data[1] == 'I' && data[2] == 'F' && data[3] == 'F', "Wrong wav header");

            // Chunk size
            long fileSize = reader.ReadInt32();

            // Subchunk, Wave Header
            // Subchunk, Format
            // Tag: "WAVE"
            reader.Read(data, 0, 4);
            Trace.Assert(data[0] == 'W' && data[1] == 'A' && data[2] == 'V' && data[3] == 'E',
                "Wrong wav tag in wav header");

            // Tag: "fmt"
            reader.Read(data, 0, 4);
            Trace.Assert(data[0] == 'f' && data[1] == 'm' && data[2] == 't' && data[3] == ' ',
                "Wrong format tag in wav header");

            // chunk format size
            var formatSize = reader.ReadInt32();
            var formatTag = reader.ReadUInt16();
            var channels = reader.ReadUInt16();
            var samplesPerSecond = reader.ReadUInt32();
            var avgBytesPerSec = reader.ReadUInt32();
            var blockAlign = reader.ReadUInt16();
            var bitsPerSample = reader.ReadUInt16();

            // Until now we have read 16 bytes in format, the rest is cbSize and is ignored for now.
            if (formatSize > 16)
                reader.ReadBytes(formatSize - 16);

            // Second Chunk, data
            // tag: data.
            reader.Read(data, 0, 4);
            Trace.Assert(data[0] == 'd' && data[1] == 'a' && data[2] == 't' && data[3] == 'a', "Wrong data tag in wav");
            // data chunk size
            var dataSize = reader.ReadInt32();

            // now, we have the format in the format parameter and the
            // reader set to the start of the body, i.e., the raw sample data
            return AudioStreamFormat.GetWaveFormatPCM(samplesPerSecond, (byte) bitsPerSample, (byte) channels);
        }
    }

    /// <summary>
    ///     Adapter class to the native stream api.
    /// </summary>
    public sealed class BinaryAudioStreamReader : PullAudioInputStreamCallback
    {
        private readonly BinaryReader _reader;


        private bool _disposed;

        /// <summary>
        ///     Creates and initializes an instance of BinaryAudioStreamReader.
        /// </summary>
        /// <param name="reader">
        ///     The underlying stream to read the audio data from. Note: The stream contains the bare sample data,
        ///     not the container (like wave header data, etc).
        /// </param>
        public BinaryAudioStreamReader(BinaryReader reader)
        {
            _reader = reader;
        }

        /// <summary>
        ///     Creates and initializes an instance of BinaryAudioStreamReader.
        /// </summary>
        /// <param name="stream">
        ///     The underlying stream to read the audio data from. Note: The stream contains the bare sample data,
        ///     not the container (like wave header data, etc).
        /// </param>
        public BinaryAudioStreamReader(Stream stream)
            : this(new BinaryReader(stream))
        {
        }

        /// <summary>
        ///     Reads binary data from the stream.
        /// </summary>
        /// <param name="dataBuffer">The buffer to fill</param>
        /// <param name="size">The size of data in the buffer.</param>
        /// <returns>
        ///     The number of bytes filled, or 0 in case the stream hits its end and there is no more data available.
        ///     If there is no data immediate available, Read() blocks until the next data becomes available.
        /// </returns>
        public override int Read(byte[] dataBuffer, uint size)
        {
            return _reader.Read(dataBuffer, 0, (int) size);
        }

        /// <summary>
        ///     This method performs cleanup of resources.
        ///     The Boolean parameter <paramref name="disposing" /> indicates whether the method is called from
        ///     <see cref="IDisposable.Dispose" /> (if <paramref name="disposing" /> is true) or from the finalizer (if
        ///     <paramref name="disposing" /> is false).
        ///     Derived classes should override this method to dispose resource if needed.
        /// </summary>
        /// <param name="disposing">Flag to request disposal.</param>
        protected override void Dispose(bool disposing)
        {
            if (_disposed) return;

            if (disposing) _reader.Dispose();

            _disposed = true;
            base.Dispose(disposing);
        }
    }

    /// <summary>
    ///     Implements a custom class for PushAudioOutputStreamCallback.
    ///     This is to receive the audio data when the synthesizer has produced audio data.
    /// </summary>
    public sealed class PushAudioOutputStreamSampleCallback : PushAudioOutputStreamCallback
    {
        private byte[] _audioData;

        /// <summary>
        ///     Constructor
        /// </summary>
        public PushAudioOutputStreamSampleCallback()
        {
            _audioData = new byte[0];
        }

        /// <summary>
        ///     A callback which is invoked when the synthesizer has a output audio chunk to write out
        /// </summary>
        /// <param name="dataBuffer">The output audio chunk sent by synthesizer</param>
        /// <returns>Tell synthesizer how many bytes are received</returns>
        public override uint Write(byte[] dataBuffer)
        {
            var oldSize = _audioData.Length;
            Array.Resize(ref _audioData, oldSize + dataBuffer.Length);
            for (var i = 0; i < dataBuffer.Length; ++i) _audioData[oldSize + i] = dataBuffer[i];

            Console.WriteLine($"{dataBuffer.Length} bytes received.");

            return (uint) dataBuffer.Length;
        }

        /// <summary>
        ///     A callback which is invoked when the synthesizer is about to close the stream
        /// </summary>
        public override void Close()
        {
            Console.WriteLine("Push audio output stream closed.");
        }

        /// <summary>
        ///     Get the received audio data
        /// </summary>
        /// <returns>The received audio data in byte array</returns>
        public byte[] GetAudioData()
        {
            return _audioData;
        }
    }
}