﻿using iDecryptIt.IO.Helpers;
using System.IO;

namespace iDecryptIt.IO.DmgTypes;

internal record UdifResourceFile(
    uint Version,
    uint HeaderSize,
    uint Flags,

    //
    ulong RunningDataForkOffset,
    ulong DataForkOffset,
    ulong DataForkLength,
    ulong ResourceForkOffset,
    ulong ResourceForkLength,

    //
    uint SegmentNumber,
    uint SegmentCount,
    UdifID SegmentID,

    //
    UdifChecksum DataForkChecksum,

    //
    ulong XmlOffset,
    ulong XmlLength,

    //
    // byte[] Reserved,
    //
    UdifChecksum MasterChecksum,

    //
    uint ImageVariant,
    ulong SectorCount

    //
    // uint[] Reserved2,
)
{
    private const string MAGIC = "koly";

    public static UdifResourceFile Read(BigEndianBinaryReader reader)
    {
        // TODO: is this big endian?
        if (reader.ReadAsciiChars(4) is not MAGIC)
            throw new InvalidDataException("Stream does not contain a UDIF resource file at the expected location.");
        uint version = reader.ReadUInt32();
        uint headerSize = reader.ReadUInt32();
        uint flags = reader.ReadUInt32();

        ulong runningDataFork = reader.ReadUInt64();
        ulong dataForkOffset = reader.ReadUInt64();
        ulong dataForkLength = reader.ReadUInt64();
        ulong resourceForkOffset = reader.ReadUInt64();
        ulong resourceForkLength = reader.ReadUInt64();

        uint segmentNumber = reader.ReadUInt32();
        uint segmentCount = reader.ReadUInt32();
        UdifID segmentID = UdifID.Read(reader);

        UdifChecksum dataForkChecksum = UdifChecksum.Read(reader);

        ulong xmlOffset = reader.ReadUInt64();
        ulong xmlLength = reader.ReadUInt64();

        reader.Skip(0x78);

        UdifChecksum masterChecksum = UdifChecksum.Read(reader);

        uint imageVariant = reader.ReadUInt32();
        ulong sectorCount = reader.ReadUInt64();

        reader.Skip(12);

        return new(
            version, headerSize, flags,
            runningDataFork, dataForkOffset, dataForkLength, resourceForkOffset, resourceForkLength,
            segmentNumber, segmentCount, segmentID,
            dataForkChecksum,
            xmlOffset, xmlLength,
            masterChecksum,
            imageVariant, sectorCount);
    }
}
