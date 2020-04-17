using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.IO.Packaging;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Xml.Linq;

namespace IOStreams
{

    public static class TestTasks
    {
        /// <summary>
        /// Parses Resourses\Planets.xlsx file and returns the planet data: 
        ///   Jupiter     69911.00
        ///   Saturn      58232.00
        ///   Uranus      25362.00
        ///    ...
        /// See Resourses\Planets.xlsx for details
        /// </summary>
        /// <param name="xlsxFileName">source file name</param>
        /// <returns>sequence of PlanetInfo</returns>
        public static IEnumerable<PlanetInfo> ReadPlanetInfoFromXlsx(string xlsxFileName)
        {
            // TODO : Implement ReadPlanetInfoFromXlsx method using System.IO.Packaging + Linq-2-Xml

            // HINT : Please be as simple & clear as possible.
            //        No complex and common use cases, just this specified file.
            //        Required data are stored in Planets.xlsx archive in 2 files:
            //         /xl/sharedStrings.xml      - dictionary of all string values
            //         /xl/worksheets/sheet1.xml  - main worksheet
            List<PlanetInfo> result = new List<PlanetInfo>();
            using (FileStream fileStream = new FileStream(xlsxFileName, FileMode.Open))
            using (Package package = Package.Open(fileStream))
            {
                PackagePart sharedStrings = package.GetPart(new Uri("/xl/sharedStrings.xml", UriKind.Relative));
                PackagePart sheet1 = package.GetPart(new Uri("/xl/worksheets/sheet1.xml", UriKind.Relative));
                int sharedStringLenght = (int)sharedStrings.GetStream().Length;
                int sheetLenght = (int)sheet1.GetStream().Length;
                byte[] sheetBuffer = new byte[sheetLenght];
                byte[] sharedStringBuffer = new byte[sharedStringLenght];
                sharedStrings.GetStream().Read(sharedStringBuffer,0,sharedStringBuffer.Length);
                sheet1.GetStream().Read(sheetBuffer,0,sheetBuffer.Length);
                string sheetStr = Encoding.UTF8.GetString(sheetBuffer);
                string sharedStringStr = Encoding.UTF8.GetString(sharedStringBuffer);
                XDocument sharedStringDoc = XDocument.Parse(sharedStringStr);
                XDocument sheetStringDoc = XDocument.Parse(sheetStr);
                XNamespace xmlns = sharedStringDoc.Root.FirstAttribute.Value;
                List<String> sharedTable = sharedStringDoc.Root.Elements(xmlns+"si").Elements(xmlns+"t").Select(a => a.Value).ToList();
                foreach (XElement row in sheetStringDoc.Root.Element(xmlns+"sheetData").Elements(xmlns+"row").Skip(1))
                { 
                    List<XElement> nodes = row.Elements().ToList();
                    if (nodes.Count < 2)
                        continue;
                    int rowId = Int32.Parse(nodes[0].Element(xmlns+"v").Value);
                    string radiusStr = nodes[1].Element(xmlns + "v").Value;
                    float radius;
                    if (!float.TryParse(radiusStr, out radius))
                        radius = float.Parse(radiusStr.Replace('.', ','));
                    PlanetInfo planet = new PlanetInfo();
                    planet.MeanRadius = Math.Round(radius,1);
                    planet.Name = sharedTable[rowId];
                    result.Add(planet);
                }
            }
            return result;

        }


        /// <summary>
        /// Calculates hash of stream using specifued algorithm
        /// </summary>
        /// <param name="stream">source stream</param>
        /// <param name="hashAlgorithmName">hash algorithm ("MD5","SHA1","SHA256" and other supported by .NET)</param>
        /// <returns></returns>
        public static string CalculateHash(this Stream stream, string hashAlgorithmName)
        {
            using (HashAlgorithm algoritm = HashAlgorithm.Create(hashAlgorithmName))
            {
                if (algoritm == null)
                    throw new ArgumentException();
                algoritm.ComputeHash(stream);
                string hash = "";
                foreach (var item in algoritm.Hash)
                    hash += item.ToString("X2");
                return hash;
            }
        }


        /// <summary>
        /// Returns decompressed strem from file. 
        /// </summary>
        /// <param name="fileName">source file</param>
        /// <param name="method">method used for compression (none, deflate, gzip)</param>
        /// <returns>output stream</returns>
        public static Stream DecompressStream(string fileName, DecompressionMethods method)
        {
            FileStream file = new FileStream(fileName, FileMode.Open);
            MemoryStream endStream = new MemoryStream((int)file.Length);
            switch (method)
            {
                case DecompressionMethods.GZip:
                    GZipStream gZipStream = new GZipStream(file, CompressionMode.Decompress);
                    gZipStream.CopyTo(endStream);
                    gZipStream.Close();
                    break;
                case DecompressionMethods.Deflate:
                    DeflateStream deflateStream = new DeflateStream(file, CompressionMode.Decompress);
                    deflateStream.CopyTo(endStream);
                    deflateStream.Close();
                    break;
                default:
                    return file;
            }
            file.Close();
            endStream.Position = 0;
            return endStream;

        }


        /// <summary>
        /// Reads file content econded with non Unicode encoding
        /// </summary>
        /// <param name="fileName">source file name</param>
        /// <param name="encoding">encoding name</param>
        /// <returns>Unicoded file content</returns>
        public static string ReadEncodedText(string fileName, string encoding)
        {
            string result = "";
            Encoding encoder = Encoding.GetEncoding(encoding);
            using (FileStream fileStream = new FileStream(fileName, FileMode.Open))
            {
                byte[] buffer = new byte[fileStream.Length];
                fileStream.Read(buffer, 0, buffer.Length);
                result = encoder.GetString(buffer);
            }
            return result;
        }
    }


    public class PlanetInfo : IEquatable<PlanetInfo>
    {
        public string Name { get; set; }
        public double MeanRadius { get; set; }

        public override string ToString()
        {
            return string.Format("{0} {1}", Name, MeanRadius);
        }

        public bool Equals(PlanetInfo other)
        {
            return Name.Equals(other.Name)
                && MeanRadius.Equals(other.MeanRadius);
        }
    }



}
