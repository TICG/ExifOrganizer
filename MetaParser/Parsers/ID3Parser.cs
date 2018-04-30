﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExifOrganizer.Meta.Parsers
{
    internal class ID3Parser : Parser
    {
        private static readonly Encoding iso_8859_1 = Encoding.GetEncoding("iso-8859-1");

        private enum ID3Tag
        {
            Version,
            Title,
            Artist,
            Album,
            Year,
            Comment,
            Genre,
            Track
        }

        private enum ID3v1Genre : byte
        {
            // Standard
            Blues = 0,

            ClassicRock = 1,
            Country = 2,
            Dance = 3,
            Disco = 4,
            Funk = 5,
            Grunge = 6,
            HipHop = 7,
            Jazz = 8,
            Metal = 9,
            NewAge = 10,
            Oldies = 11,
            Other = 12,
            Pop = 13,
            RnB = 14,
            Rap = 15,
            Reggae = 16,
            Rock = 17,
            Techno = 18,
            Industrial = 19,
            Alternative = 20,
            Ska = 21,
            DeathMetal = 22,
            Pranks = 23,
            Soundtrack = 24,
            EuroTechno = 25,
            Ambient = 26,
            TripHop = 27,
            Vocal = 28,
            JazzFunk = 29,
            Fusion = 30,
            Trance = 31,
            Classical = 32,
            Instrumental = 33,
            Acid = 34,
            House = 35,
            Game = 36,
            SoundClip = 37,
            Gospel = 38,
            Noise = 39,
            AlternativeRock = 40,
            Bass = 41,
            Soul = 42,
            Punk = 43,
            Space = 44,
            Meditative = 45,
            InstrumentalPop = 46,
            InstrumentalRock = 47,
            Ethnic = 48,
            Gothic = 49,
            Darkwave = 50,
            TechnoIndustrial = 51,
            Electronic = 52,
            PopFolk = 53,
            Eurodance = 54,
            Dream = 55,
            SouthernRock = 56,
            Comedy = 57,
            Cult = 58,
            Gangsta = 59,
            Top40 = 60,
            ChristianRap = 61,
            PopFunk = 62,
            Jungle = 63,
            NativeAmerican = 64,
            Cabaret = 65,
            NewWave = 66,
            Psychedelic = 67,
            Rave = 68,
            Showtunes = 69,
            Trailer = 70,
            LoFi = 71,
            Tribal = 72,
            AcidPunk = 73,
            AcidJazz = 74,
            Polka = 75,
            Retro = 76,
            Musical = 77,
            RocknRoll = 78,
            HardRock = 79,

            // Winamp extended
            Folk = 80,

            FolkRock = 81,
            NationalFolk = 82,
            Swing = 83,
            FastFusion = 84,
            Bebob = 85,
            Latin = 86,
            Revival = 87,
            Celtic = 88,
            BlueGrass = 89,
            Avantgarde = 90,
            GothicRock = 91,
            ProgressiveRock = 92,
            PsychedelicRock = 93,
            SymphonicRock = 94,
            SlowRock = 95,
            BigBand = 96,
            Chorus = 97,
            EasyListening = 98,
            Acoustic = 99,
            Humour = 100,
            Speech = 101,
            Chanson = 102,
            Opera = 103,
            ChamberMusic = 104,
            Sonata = 105,
            Symphony = 106,
            BootyBass = 107,
            Primus = 108,
            PornGroove = 109,
            Satire = 110,
            SlowJam = 111,
            Club = 112,
            Tango = 113,
            Samba = 114,
            Folklore = 115,
            Ballad = 116,
            PowerBallad = 117,
            RhythmicSoul = 118,
            Freestyle = 119,
            Duet = 120,
            PunkRock = 121,
            DrumSolo = 122,
            Acapella = 123,
            EuroHouse = 124,
            DanceHall = 125,
            Goa = 126,
            DrumnBass = 127,
            ClubHouse = 128,
            HardcoreTechno = 129,
            Terror = 130,
            Indie = 131,
            BritPop = 132,
            Negerpunk = 133,
            PolskPunk = 134,
            Beat = 135,
            ChristianGangstaRap = 136,
            HeavyMetal = 137,
            BlackMetal = 138,
            Crossover = 139,
            ContemporaryChristian = 140,
            ChristianRock = 141,

            // Winamp 1.91
            Merengue = 142,

            Salsa = 143,
            ThrashMetal = 144,
            Anime = 145,
            JPop = 146,
            SynthPop = 147,

            // TODO: Winamp 5.6
            Abstract = 148,

            ArtRock = 149,
            Baroque = 150,
            Bhangra = 151,
            BigBeat = 152,
            Breakbeat = 153,
            Chillout = 154,
            Downtempo = 155,
            Dub = 156,
            EBM = 157,
            Eclectic = 158,
            Electro = 159,
            Electroclash = 160,
            Emo = 161,
            Experimental = 162,
            Garage = 163,
            Global = 164,
            IDM = 165,
            Illbient = 166,
            IndustroGoth = 167,
            JamBand = 168,
            Krautrock = 169,
            Leftfield = 170,
            Lounge = 171,
            MathRock = 172,
            NewRomantic = 173,
            NuBreakz = 174,
            PostPunk = 175,
            PostRock = 176,
            Psytrance = 177,
            Shoegaze = 178,
            SpaceRock = 179,
            TropRock = 180,
            WorldMusic = 181,
            Neoclassical = 182,
            Audiobook = 183,
            AudioTheatre = 184,
            NeueDeutscheWelle = 185,
            Podcast = 186,
            IndieRock = 187,
            GFunk = 188,
            Dubstep = 189,
            GarageRock = 190,
            Psybient = 191,

            // Other
            Undefined = 255
        }

        public static MetaData Parse(string filename)
        {
            Task<MetaData> task = ParseAsync(filename);
            task.ConfigureAwait(false); // Prevent deadlock of caller
            return task.Result;
        }

        public static Task<MetaData> ParseAsync(string filename)
        {
            return Task.Run(() => ParseThread(filename));
        }

        private static MetaData ParseThread(string filename)
        {
            if (!File.Exists(filename))
                throw new MetaParseException("File not found: {0}", filename);

            Dictionary<ID3Tag, object> id3 = ParseID3(filename);

            MetaData meta = new MetaData();
            meta.Type = MetaType.Music;
            meta.Path = filename;
            meta.Data = new Dictionary<MetaKey, object>();
            meta.Data[MetaKey.FileName] = Path.GetFileName(filename);
            meta.Data[MetaKey.OriginalName] = meta.Data[MetaKey.FileName];
            meta.Data[MetaKey.Size] = GetFileSize(filename);
            meta.Data[MetaKey.DateCreated] = File.GetCreationTime(filename);
            meta.Data[MetaKey.DateModified] = File.GetLastWriteTime(filename);
            meta.Data[MetaKey.Timestamp] = meta.Data[MetaKey.DateModified];

            if (id3.ContainsKey(ID3Tag.Version))
                meta.Data[MetaKey.MetaType] = id3[ID3Tag.Version];
            if (id3.ContainsKey(ID3Tag.Title))
                meta.Data[MetaKey.Title] = id3[ID3Tag.Title];
            if (id3.ContainsKey(ID3Tag.Artist))
                meta.Data[MetaKey.Artist] = id3[ID3Tag.Artist];
            if (id3.ContainsKey(ID3Tag.Album))
                meta.Data[MetaKey.Album] = id3[ID3Tag.Album];
            if (id3.ContainsKey(ID3Tag.Year))
                meta.Data[MetaKey.Year] = id3[ID3Tag.Year];
            if (id3.ContainsKey(ID3Tag.Comment))
                meta.Data[MetaKey.Comment] = id3[ID3Tag.Comment];
            if (id3.ContainsKey(ID3Tag.Genre) && ((ID3v1Genre)id3[ID3Tag.Genre]) != ID3v1Genre.Undefined)
                meta.Data[MetaKey.Genre] = id3[ID3Tag.Genre].ToString(); // TODO: add custom ToString() method
            if (id3.ContainsKey(ID3Tag.Track))
                meta.Data[MetaKey.Track] = id3[ID3Tag.Track];

            return meta;
        }

        private static Dictionary<ID3Tag, object> ParseID3(string filename)
        {
            byte[] data = File.ReadAllBytes(filename);
            Dictionary<ID3Tag, object> id3v1 = ParseID3v1(data);
            Dictionary<ID3Tag, object> id3v2 = ParseID3v2(data);

            // Merge ID3v1 into ID3v2
            foreach (var tag in id3v1)
            {
                Trace.WriteLine($"[{nameof(ID3Parser)}] * {tag.Key} = \"{tag.Value}\"");
                if (id3v2.ContainsKey(tag.Key))
                {
                    Trace.WriteLine($"[{nameof(ID3Parser)}] key found in both ID3v1 and ID3v2: {tag.Key}");
                    continue;
                }

                id3v2[tag.Key] = tag.Value;
            }

            return id3v2;
        }

        private static Dictionary<ID3Tag, object> ParseID3v1(byte[] data)
        {
            Dictionary<ID3Tag, object> tags = new Dictionary<ID3Tag, object>();
            if (data.Length < 128)
                return tags;

            byte[] tail = data.Skip(data.Length - 128).ToArray();
            if (tail[0] != 'T' || tail[1] != 'A' || tail[2] != 'G')
                return tags;

            tags[ID3Tag.Version] = "ID3v1";
            tags[ID3Tag.Title] = iso_8859_1.GetString(tail.Skip(3).Take(30).TakeWhile(c => c != '\0').ToArray());
            tags[ID3Tag.Artist] = iso_8859_1.GetString(tail.Skip(33).Take(30).TakeWhile(c => c != '\0').ToArray());
            tags[ID3Tag.Album] = iso_8859_1.GetString(tail.Skip(63).Take(30).TakeWhile(c => c != '\0').ToArray());
            tags[ID3Tag.Year] = iso_8859_1.GetString(tail.Skip(93).Take(4).TakeWhile(c => c != '\0').ToArray());
            IEnumerable<byte> comment = tail.Skip(97).Take(30);
            tags[ID3Tag.Comment] = iso_8859_1.GetString(comment.TakeWhile(c => c != '\0').ToArray());
            if (comment.Last() != '\0' && comment.Skip(28).First() == '\0')
            {
                tags[ID3Tag.Version] = "ID3v1.1";
                tags[ID3Tag.Track] = (int)comment.Last();
            }
            tags[ID3Tag.Genre] = (ID3v1Genre)tail.Skip(3 + 30 + 30 + 30 + 4 + 30).First();
            return tags;
        }

        private static Dictionary<ID3Tag, object> ParseID3v2(byte[] data)
        {
            Dictionary<ID3Tag, object> tags = new Dictionary<ID3Tag, object>();

            //tags[ID3Tag.Version] = "ID3v2";
            //tags[ID3Tag.Title] = "TODO-title";
            //tags[ID3Tag.Artist] = "TODO-artist";
            //tags[ID3Tag.Album] = "TODO-album";
            //tags[ID3Tag.Year] = "TODO-year";
            //tags[ID3Tag.Comment] = "TODO-comment";
            //tags[ID3Tag.Track] = -1; // TODO
            //tags[ID3Tag.Genre] = ID3v1Genre.Undefined;

            return tags;
        }
    }
}