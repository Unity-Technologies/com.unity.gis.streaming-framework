
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Unity.Mathematics;
using UnityEngine.Assertions;

namespace Unity.Geospatial.Streaming.UniversalDecoder
{
    /// <summary>
    /// The file type is used to figure out the content type of a file based on its path extension
    /// or magic number.
    /// </summary>
    /// <remarks>This does not allow to figure out the <see cref="INodeContentLoader"/> to use since multiple
    /// <see cref="INodeContentLoader"/> uses files of the same type, for example, json.</remarks>
    public sealed class FileType :
        IEquatable<FileType>
    {
        /// <summary>
        /// Registered <see cref="FileType"/> sorted by file extension.
        /// </summary>
        /// <remarks>Two <see cref="FileType"/> cannot be registered with the same extension.</remarks>
        private static readonly IDictionary<string, FileType> k_ByExtension = new Dictionary<string, FileType>();

        /// <summary>
        /// Registered <see cref="FileType"/> sorted by magic numbers.
        /// </summary>
        /// <remarks>Two <see cref="FileType"/> cannot be registered with the same magic number.</remarks>
        private static readonly IDictionary<int, FileType> k_ByMagicNumber = new Dictionary<int, FileType>();

        /// <summary>
        /// Length of the longest registered magic number part of <see cref="FileType.MagicNumbers"/>.
        /// </summary>
        private static int s_MaxMagicNumber;

        /// <summary>
        /// Length of the shortest registered magic number part of <see cref="FileType.MagicNumbers"/>.
        /// </summary>
        private static int s_MinMagicNumber = int.MaxValue;

        /// <summary>
        /// Number used to autoincrement the Id on each new instance.
        /// </summary>
        private static int s_LastId = -1;

        /// <summary>
        /// Unique identifier.
        /// </summary>
        private readonly int m_Id;

        /// <summary>
        /// OGC 3d Tiles Batch 3D Model format.
        /// </summary>
        /// <see href="https://github.com/CesiumGS/3d-tiles/blob/main/specification/TileFormats/Batched3DModel">Batched3DModel</see>
        public static readonly FileType Ogc3dTilesB3dm = CreateAndRegister(".b3dm", 0x62, 0x33, 0x64, 0x6D);

        /// <summary>
        /// OGC 3d Tiles Composite format.
        /// </summary>
        /// <see href="https://github.com/CesiumGS/3d-tiles/tree/main/specification/TileFormats/Composite">Composite</see>
        public static readonly FileType Ogc3dTilesCmpt = CreateAndRegister(".cmpt", 0x63, 0x6D, 0x70, 0x74);

        /// <summary>
        /// Binary GL Transmission Format.
        /// </summary>
        /// <see href="https://github.com/KhronosGroup/glTF">glTF</see>
        public static readonly FileType Glb = CreateAndRegister("glb", new string[] { ".glb" }, 0x67, 0x6C, 0x54, 0x46);

        /// <summary>
        /// GL Transmission Format.
        /// </summary>
        /// <see href="https://github.com/KhronosGroup/glTF">glTF</see>
        public static readonly FileType Gltf = CreateAndRegister("gltf", new string[] { ".gltf", ".gltf1", ".gltf2" });

        /// <summary>
        /// OGC 3d Tiles Instanced 3D Model format.
        /// </summary>
        /// <see href="https://github.com/CesiumGS/3d-tiles/tree/main/specification/TileFormats/Instanced3DModel">Instanced3DModel</see>
        public static readonly FileType Ogc3dTilesI3dm = CreateAndRegister(".i3dm", 0x69, 0x33, 0x64, 0x6D);

        /// <summary>
        /// JPEG IMAGE Format.
        /// </summary>
        public static readonly FileType Jpeg = CreateAndRegister(
            "jpeg",
            new string[] { ".jpg", ".jpe", ".jpeg", ".jif" },
            new byte[][] {
                new byte[] { 0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x10, 0x4A, 0x46, 0x49, 0x46 },
                new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 },
                new byte[] { 0xFF, 0xD8, 0xFF, 0xE1 },
                new byte[] { 0xFF, 0xD8, 0xFF, 0xE2 },
                new byte[] { 0xFF, 0xD8, 0xFF, 0xE3 },
                new byte[] { 0xFF, 0xD8, 0xFF, 0xE8 }
            });

        /// <summary>
        /// JavaScript Object Notation format.
        /// </summary>
        /// <see href="https://www.json.org/json-en.html">json</see>
        public static readonly FileType Json = CreateAndRegister(".json");

        /// <summary>
        /// OGC 3d Tiles Point Cloud format.
        /// </summary>
        /// <see href="https://github.com/CesiumGS/3d-tiles/tree/main/specification/TileFormats/PointCloud">PointCloud</see>
        public static readonly FileType Ogc3dTilesPnts = CreateAndRegister(".pnts", 0x70, 0x6e, 0x74, 0x73);

        /// <summary>
        /// TMS (Terrain Management System) terrain format.
        /// </summary>
        public static readonly FileType TmsTerrain = CreateAndRegister(".utr", new char[] { 'u', 't', 'r', 'm' });

        /// <summary>
        /// Default constructor.
        /// This new file type will be registered after creation.
        /// </summary>
        /// <param name="name">Name used when converting to string.</param>
        /// <param name="extensions">List of file path extensions associated with the file format.</param>
        /// <param name="magicNumbers">List of <see href="https://en.wikipedia.org/wiki/Magic_number_(programming)#In_files">magic numbers</see> associated with the file format.</param>
        public FileType(string name, IEnumerable<string> extensions, IEnumerable<byte[]> magicNumbers = null)
        {
            string Selector(string ext) => $"{(ext.StartsWith(".") ? "" : ".")}{ext.ToLower()}";

            Extensions = extensions is null
                ? Array.Empty<string>()
                : extensions
                    .Where(each => !string.IsNullOrEmpty(each))
                    .Select(Selector)
                    .ToArray();

            MagicNumbers = magicNumbers is null
                ? Array.Empty<byte[]>()
                : magicNumbers
                    .Where(each => each != null && each.Length > 0)
                    .ToArray();

            Name = name;

            m_Id = GetNextId();
        }

        /// <summary>
        /// Get unique id and reserve it by incrementing the next available id.
        /// </summary>
        /// <returns>A unique id ready to be associated with.</returns>
        private static int GetNextId()
        {
            return ++s_LastId;
        }

        /// <summary>
        /// Create a new <see cref="FileType"/> instance and <see cref="Register"/> it.
        /// </summary>
        /// <param name="name">Name used when converting to string.</param>
        /// <param name="extensions">List of file path extensions associated with the file format.</param>
        /// <param name="magicNumbers">List of <see href="https://en.wikipedia.org/wiki/Magic_number_(programming)#In_files">magic numbers</see> associated with the file format.</param>
        /// <returns>The newly created <see cref="FileType"/> instance.</returns>
        public static FileType CreateAndRegister(string name, IEnumerable<string> extensions, IEnumerable<byte[]> magicNumbers = null)
        {
            FileType result = new FileType(name, extensions, magicNumbers);
            result.Register();
            return result;
        }

        /// <summary>
        /// Create a new <see cref="FileType"/> instance and <see cref="Register"/> it.
        /// </summary>
        /// <param name="name">Name used when converting to string.</param>
        /// <param name="extensions">List of file path extensions associated with the file format.</param>
        /// <param name="magicNumbers">List of <see href="https://en.wikipedia.org/wiki/Magic_number_(programming)#In_files">magic numbers</see>
        /// as a char array when the header can be considered as UTF8 encoded.</param>
        /// <returns>The newly created <see cref="FileType"/> instance.</returns>
        public static FileType CreateAndRegister(string name, IEnumerable<string> extensions, IEnumerable<char[]> magicNumbers)
        {
            return CreateAndRegister(
                name,
                extensions,
                magicNumbers
                    .Select(each => each
                        .Select(c => (byte)c)
                        .ToArray())
                    .ToArray());
        }

        /// <summary>
        /// Create a new <see cref="FileType"/> instance and <see cref="Register"/> it.
        /// </summary>
        /// <param name="name">Name used when converting to string.</param>
        /// <param name="extensions">List of file path extensions associated with the file format.</param>
        /// <param name="magicNumber">The <see href="https://en.wikipedia.org/wiki/Magic_number_(programming)#In_files">magic number</see> associated with the file format.</param>
        /// <returns>The newly created <see cref="FileType"/> instance.</returns>
        public static FileType CreateAndRegister(string name, IEnumerable<string> extensions, params byte[] magicNumber)
        {
            return CreateAndRegister(name, extensions, new byte[][] { magicNumber });
        }

        /// <summary>
        /// Create a new <see cref="FileType"/> instance and <see cref="Register"/> it.
        /// </summary>
        /// <param name="name">Name used when converting to string.</param>
        /// <param name="extensions">List of file path extensions associated with the file format.</param>
        /// <param name="magicNumbers">The <see href="https://en.wikipedia.org/wiki/Magic_number_(programming)#In_files">magic numbers</see>
        /// as a string when the header can be considered as UTF8 encoded.</param>
        /// <returns>The newly created <see cref="FileType"/> instance.</returns>
        public static FileType CreateAndRegister(string name, IEnumerable<string> extensions, IEnumerable<string> magicNumbers)
        {
            return CreateAndRegister(
                name,
                extensions,
                magicNumbers
                    .Select(each => Encoding.UTF8.GetBytes(each))
                    .ToArray());
        }

        /// <summary>
        /// Create a new <see cref="FileType"/> instance and <see cref="Register"/> it.
        /// </summary>
        /// <param name="name">Name used when converting to string.</param>
        /// <param name="extensions">List of file path extensions associated with the file format.</param>
        /// <param name="magicNumber">The <see href="https://en.wikipedia.org/wiki/Magic_number_(programming)#In_files">magic number</see>
        /// as a char array when the header can be considered as UTF8 encoded.</param>
        /// <returns>The newly created <see cref="FileType"/> instance.</returns>
        public static FileType CreateAndRegister(string name, IEnumerable<string> extensions, IEnumerable<char> magicNumber)
        {
            return CreateAndRegister(
                name,
                extensions,
                magicNumber
                    .Select(c => (byte)c)
                    .ToArray());
        }

        /// <summary>
        /// Create a new <see cref="FileType"/> instance and <see cref="Register"/> it.
        /// </summary>
        /// <param name="name">Name used when converting to string.</param>
        /// <param name="extensions">List of file path extensions associated with the file format.</param>
        /// <param name="magicNumber">The <see href="https://en.wikipedia.org/wiki/Magic_number_(programming)#In_files">magic number</see>
        /// as a string when the header can be considered as UTF8 encoded.</param>
        /// <returns>The newly created <see cref="FileType"/> instance.</returns>
        public static FileType CreateAndRegister(string name, IEnumerable<string> extensions, string magicNumber)
        {
            return CreateAndRegister(
                name,
                extensions,
                Encoding.UTF8.GetBytes(magicNumber));
        }

        /// <summary>
        /// Create a new <see cref="FileType"/> instance and <see cref="Register"/> it.
        /// </summary>
        /// <param name="name">Name used when converting to string.</param>
        /// <param name="extension">List of file path extensions associated with the file format.</param>
        /// <param name="magicNumber">The <see href="https://en.wikipedia.org/wiki/Magic_number_(programming)#In_files">magic number</see> associated with the file format.</param>
        /// <returns>The newly created <see cref="FileType"/> instance.</returns>
        public static FileType CreateAndRegister(string name, string extension, params byte[] magicNumber)
        {
            return CreateAndRegister(name, new string[] { extension }, new byte[][] { magicNumber });
        }

        /// <summary>
        /// Create a new <see cref="FileType"/> instance and <see cref="Register"/> it.
        /// </summary>
        /// <param name="name">Name used when converting to string.</param>
        /// <param name="extension">List of file path extensions associated with the file format.</param>
        /// <param name="magicNumber">The <see href="https://en.wikipedia.org/wiki/Magic_number_(programming)#In_files">magic number</see>
        /// as a char array when the header can be considered as UTF8 encoded.</param>
        /// <returns>The newly created <see cref="FileType"/> instance.</returns>
        public static FileType CreateAndRegister(string name, string extension, IEnumerable<char> magicNumber)
        {
            return CreateAndRegister(
                name,
                extension,
                magicNumber
                    .Select(c => (byte)c)
                    .ToArray());
        }

        /// <summary>
        /// Create a new <see cref="FileType"/> instance and <see cref="Register"/> it.
        /// </summary>
        /// <param name="name">Name used when converting to string.</param>
        /// <param name="extension">List of file path extensions associated with the file format.</param>
        /// <param name="magicNumber">The <see href="https://en.wikipedia.org/wiki/Magic_number_(programming)#In_files">magic number</see>
        /// as a string when the header can be considered as UTF8 encoded.</param>
        /// <returns>The newly created <see cref="FileType"/> instance.</returns>
        public static FileType CreateAndRegister(string name, string extension, string magicNumber)
        {
            return CreateAndRegister(
                name,
                extension,
                Encoding.UTF8.GetBytes(magicNumber));
        }

        /// <summary>
        /// Create a new <see cref="FileType"/> instance and <see cref="Register"/> it.
        /// The name of the file type will be the same as the specified file extension.
        /// </summary>
        /// <param name="extension">List of file path extensions associated with the file format.</param>
        /// <param name="magicNumber">The <see href="https://en.wikipedia.org/wiki/Magic_number_(programming)#In_files">magic number</see> associated with the file format.</param>
        /// <returns>The newly created <see cref="FileType"/> instance.</returns>
        public static FileType CreateAndRegister(string extension, params byte[] magicNumber)
        {
            Assert.IsFalse(
                string.IsNullOrEmpty(extension),
                $"You need to give a name to the {nameof(FileType)} when no extension is given.");

            return CreateAndRegister(extension.TrimStart('.').ToLower(), new string[] { extension }, new byte[][] { magicNumber });
        }

        /// <summary>
        /// Create a new <see cref="FileType"/> instance and <see cref="Register"/> it.
        /// The name of the file type will be the same as the specified file extension.
        /// </summary>
        /// <param name="extension">List of file path extensions associated with the file format.</param>
        /// <param name="magicNumber">The <see href="https://en.wikipedia.org/wiki/Magic_number_(programming)#In_files">magic number</see>
        /// as a char array when the header can be considered as UTF8 encoded.</param>
        /// <returns>The newly created <see cref="FileType"/> instance.</returns>
        public static FileType CreateAndRegister(string extension, IEnumerable<char> magicNumber)
        {
            return CreateAndRegister(
                extension,
                magicNumber
                    .Select(c => (byte)c)
                    .ToArray());
        }

        /// <summary>
        /// Create a new <see cref="FileType"/> instance and <see cref="Register"/> it.
        /// The name of the file type will be the same as the specified file extension.
        /// </summary>
        /// <param name="extension">List of file path extensions associated with the file format.</param>
        /// <param name="magicNumber">The <see href="https://en.wikipedia.org/wiki/Magic_number_(programming)#In_files">magic number</see>
        /// as a string when the header can be considered as UTF8 encoded.</param>
        /// <returns>The newly created <see cref="FileType"/> instance.</returns>
        public static FileType CreateAndRegister(string extension, string magicNumber)
        {
            return CreateAndRegister(
                extension,
                Encoding.UTF8.GetBytes(magicNumber));
        }

        /// <summary>
        /// File extensions associated with this file type.
        /// </summary>
        /// <remarks>This can be an empty list.</remarks>
        public IReadOnlyList<string> Extensions { get; }

        /// <summary>
        /// List of <see href="https://en.wikipedia.org/wiki/Magic_number_(programming)#In_files">magic numbers</see> associated with the file format.
        /// </summary>
        /// <remarks>This can be an empty list.</remarks>
        public IReadOnlyList<byte[]> MagicNumbers { get; }

        /// <summary>
        /// Name of this instance allowing to easily differentiate it from others without comparing all of its values.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Validate if both <see cref="FileType"/> are the same.
        /// </summary>
        /// <param name="item1">First <see cref="FileType"/> to evaluate its equality.</param>
        /// <param name="item2">Second <see cref="FileType"/> to evaluate its equality.</param>
        /// <returns>
        /// <see langword="true"/> if both types represent the same <see cref="FileType"/>;
        /// <see langword="false"/> otherwise.
        /// </returns>
        public static bool operator ==(FileType item1, FileType item2)
        {
            return item1 is null && item2 is null || !(item1 is null) && item1.Equals(item2);
        }

        /// <summary>
        /// Validate if both <see cref="FileType"/> aren't the same.
        /// </summary>
        /// <param name="item1">First <see cref="FileType"/> to evaluate its inequality.</param>
        /// <param name="item2">Second <see cref="FileType"/> to evaluate its inequality.</param>
        /// <returns>
        /// <see langword="true"/> if both types does not represent the same <see cref="FileType"/>;
        /// <see langword="false"/> otherwise.
        /// </returns>
        public static bool operator !=(FileType item1, FileType item2)
        {
            return !(item1 is null) && !item1.Equals(item2) || !(item2 is null) && !item2.Equals(item1);
        }

        /// <summary>
        /// See if this is equivalent to another object.
        /// </summary>
        /// <param name="obj">The other object</param>
        /// <returns>True if the other object is a <see cref="FileType"/> class and has the same id.</returns>
        public override bool Equals(object obj)
        {
            return obj is FileType other && Equals(other);
        }

        /// <summary>
        /// See if two <see cref="FileType"/> instances are equivalent.
        /// </summary>
        /// <param name="obj">The other <see cref="FileType"/>.</param>
        /// <returns>
        /// <see langword="true"/> if they both are the same;
        /// <see langword="false"/> otherwise.
        /// </returns>
        public bool Equals(FileType obj)
        {
            return ReferenceEquals(this, obj);
        }

        /// <summary>
        /// Obtain the hash of the FileType.
        /// </summary>
        /// <returns>The hash of the FileType.</returns>
        public override int GetHashCode()
        {
            return m_Id;
        }

        /// <summary>
        /// Get the <see cref="FileType"/> that match the given <paramref name="path"/> based on its file extension.
        /// </summary>
        /// <param name="path">File path to get the <see cref="FileType"/> from.</param>
        /// <returns>
        /// The <see cref="FileType"/> matching the given file path;
        /// <see langword="null"/> if no registered <see cref="FileType"/> matches the given <paramref name="path"/>.
        /// </returns>
        public static FileType GetByFileExtension(string path)
        {
            if (string.IsNullOrEmpty(path))
                return null;

            string extension = Path.GetExtension(path);

            if (string.IsNullOrEmpty(extension))
                return null;

            return k_ByExtension.TryGetValue(extension.ToLower(), out FileType result)
                ? result
                : null;
        }

        /// <summary>
        /// Get the <see cref="FileType"/> that match the given <paramref name="stream"/> header.
        /// </summary>
        /// <param name="stream">Data stream to get the <see cref="FileType"/> from.</param>
        /// <returns>
        /// The <see cref="FileType"/> matching the given <paramref name="stream"/>;
        /// <see langword="null"/> if no registered <see cref="FileType"/> matches the given <paramref name="stream"/>.
        /// </returns>
        public static FileType GetByMagicNumber(Stream stream)
        {
            if (stream is null)
                return null;

            stream.Position = 0;
            byte[] header = new byte[s_MaxMagicNumber];
            int max = math.min(s_MaxMagicNumber, (int)stream.Length);
            stream.Read(header, 0, max);

            int keyMax = 0;
            int[] keys = new int[max];

            for (int i = 0; i < max; i++)
            {
                keyMax = (keyMax * 31) ^ header[i];
                keys[i] = keyMax;
            }

            for (int i = max - 1; i >= s_MinMagicNumber - 1; i--)
            {
                if (k_ByMagicNumber.TryGetValue(keys[i], out FileType result))
                    return result;
            }

            return null;
        }

        /// <summary>
        /// Get the <see cref="FileType"/> that match the given <paramref name="data"/> header.
        /// </summary>
        /// <param name="data">Data to get the <see cref="FileType"/> from.</param>
        /// <returns>
        /// The <see cref="FileType"/> matching the given <paramref name="data"/>;
        /// <see langword="null"/> if no registered <see cref="FileType"/> matches the given <paramref name="data"/>.
        /// </returns>
        public static FileType GetByMagicNumber(byte[] data)
        {
            if (data is null)
                return null;

            byte[] header = new byte[s_MaxMagicNumber];
            int max = math.min(s_MaxMagicNumber, data.Length);
            Array.Copy(data, header, max);

            int keyMax = 0;
            int[] keys = new int[max];

            for (int i = 0; i < max; i++)
            {
                keyMax = (keyMax * 31) ^ header[i];
                keys[i] = keyMax;
            }

            for (int i = max - 1; i >= s_MinMagicNumber - 1; i--)
            {
                if (k_ByMagicNumber.TryGetValue(keys[i], out FileType result))
                    return result;
            }

            return null;
        }

        /// <summary>
        /// Get the hash code of a list of bytes allowing to search dictionary by key instead of comparaison.
        /// </summary>
        /// <param name="array">Byte list to get its hash for.</param>
        /// <returns>Get the hash code result.</returns>
        private static int GetMagicHashCode(IReadOnlyList<byte> array)
        {
            unchecked
            {
                int result = 0;

                for (int i = 0; i < array.Count; i++)
                    result = (result * 31) ^ array[i];

                return result;
            }
        }

        /// <summary>
        /// Register this <see cref="FileType"/> instance allowing to execute <see cref="GetByFileExtension"/> and <see cref="GetByMagicNumber(byte[])"/>.
        /// </summary>
        public void Register()
        {
            Register(this);
        }

        /// <summary>
        /// Register a given <see cref="FileType"/> instance allowing to execute <see cref="GetByFileExtension"/> and <see cref="GetByMagicNumber(byte[])"/>.
        /// </summary>
        private static void Register(FileType fileType)
        {
            foreach (string extension in fileType.Extensions)
            {
                Assert.IsFalse(k_ByExtension.ContainsKey(extension), $"Two {nameof(FileType)}s has the same extension.");

                k_ByExtension[extension] = fileType;
            }

            foreach (byte[] magic in fileType.MagicNumbers)
            {
                int key = GetMagicHashCode(magic);

                Assert.IsFalse(k_ByMagicNumber.ContainsKey(key), $"Two {nameof(FileType)}s has the same magic key.");

                s_MaxMagicNumber = math.max(s_MaxMagicNumber, magic.Length);
                s_MinMagicNumber = math.min(s_MinMagicNumber, magic.Length);

                k_ByMagicNumber[key] = fileType;
            }
        }

        /// <summary>
        /// Stop searching for this <see cref="FileType"/> instance when executing <see cref="GetByFileExtension"/> or <see cref="GetByMagicNumber(byte[])"/>.
        /// </summary>
        public void Unregister()
        {
            foreach (string extension in Extensions)
                k_ByExtension.Remove(new KeyValuePair<string, FileType>(extension, this));

            foreach (byte[] magic in MagicNumbers)
            {
                int key = GetMagicHashCode(magic);
                k_ByMagicNumber.Remove(new KeyValuePair<int, FileType>(key, this));
            }
        }

        /// <summary>
        /// Get the name of this instance.
        /// </summary>
        /// <returns>The class type followed by the name of this instance.</returns>
        public override string ToString()
        {
            return $"{GetType().Name} {Name}";
        }
    }
}
