using System.Linq;
using System.IO;
using System.Runtime.Serialization;

namespace DashboardApi.Utility
{
    public class CompressionHelper<T>
    {
        public byte[] GetBytes(object data)
        {
            DataContractSerializer serializer = new DataContractSerializer(typeof(T));

            byte[] byteArr;
            using (var ms = new System.IO.MemoryStream())
            {
                serializer.WriteObject(ms, data);
                byteArr = ms.ToArray();
            }
            return byteArr;
        }

        //Json Start
        public byte[] GetBytesForJson(object data)
        {
            DataContractSerializer serializer = new DataContractSerializer(typeof(T));

            //byte[] byteArr;
            var str = Newtonsoft.Json.JsonConvert.SerializeObject(data);
            return System.Text.Encoding.ASCII.GetBytes(str);
            //using (var ms = new System.IO.MemoryStream())
            //{
            //    serializer.WriteObject(ms, data);
            //    byteArr = ms.ToArray();
            //}
            //return byteArr;
        }

        public void CompressAndSaveJsonLZ4(T obj, string fileName)
        {
            using (FileStream fs = new FileStream(fileName, FileMode.OpenOrCreate))
            {
                using (MemoryStream objectSerialization = new MemoryStream())
                {
                    var byt = GetBytesForJson(obj);

                    using (BinaryWriter binaryWriter = new BinaryWriter(fs))
                    {
                        binaryWriter.Write(byt.Length);    //write the length first

                        using (var stream = new LZ4.LZ4Stream(fs, LZ4.LZ4StreamMode.Compress, true, byt.Count()))
                        {
                            stream.Write(byt, 0, byt.Length);
                        }
                    }
                }
            }

        }

        public T ReadCompressJsonLZ4(string file)
        {

            int sz = 0;
            using (FileStream fs = new FileStream(file, FileMode.Open))
            {
                using (BinaryReader binaryReader = new BinaryReader(fs))
                {
                    int length = binaryReader.ReadInt32();    //read the length first
                    //if (length < 0) { length = 0; }
                    byte[] bytesUncompressed = new byte[length]; //you can convert this back to the object using a MemoryStream ;)

                    using (var stream = new LZ4.LZ4Stream(fs, LZ4.LZ4StreamMode.Decompress, true, length))
                    {
                        try
                        {
                            while ((sz = stream.Read(bytesUncompressed, 0, bytesUncompressed.Length)) > 0)
                            {
                                // ...
                            }
                        }
                        catch (System.IO.EndOfStreamException ex) { }
                        catch (System.NotSupportedException ex) { }

                        var str =System.Text.Encoding.UTF8.GetString(bytesUncompressed, 0, bytesUncompressed.Length);
                        return (T)Newtonsoft.Json.JsonConvert.DeserializeObject(str);

                        //var serializer = new DataContractSerializer(typeof(T));
                        //using (var ms = new MemoryStream(bytesUncompressed))
                        //{
                        //    //if (length == 0) { return default(T); }
                        //    return (T)serializer.ReadObject(ms);
                        //}
                    }

                }
            }
        }
        //Json End

        public void CompressAndSaveLZ4(T obj, string fileName)
        {
            using (FileStream fs = new FileStream(fileName, FileMode.OpenOrCreate))
            {
                using (MemoryStream objectSerialization = new MemoryStream())
                {
                    var byt = GetBytes(obj);

                    using (BinaryWriter binaryWriter = new BinaryWriter(fs))
                    {
                        binaryWriter.Write(byt.Length);    //write the length first

                        using (var stream = new LZ4.LZ4Stream(fs, LZ4.LZ4StreamMode.Compress, true, byt.Count()))
                        {
                            stream.Write(byt, 0, byt.Length);
                        }
                    }
                }
            }

        }

        public T ReadCompressSharpLZ4(string file)
        {

            int sz = 0;
            using (FileStream fs = new FileStream(file, FileMode.Open))
            {
                using (BinaryReader binaryReader = new BinaryReader(fs))
                {
                    int length = binaryReader.ReadInt32();    //read the length first
                    //if (length < 0) { length = 0; }
                    byte[] bytesUncompressed = new byte[length]; //you can convert this back to the object using a MemoryStream ;)

                    using (var stream = new LZ4.LZ4Stream(fs, LZ4.LZ4StreamMode.Decompress, true, length))
                    {
                        try
                        {
                            while ((sz = stream.Read(bytesUncompressed, 0, bytesUncompressed.Length)) > 0)
                            {
                                // ...
                            }
                        }
                        catch (System.IO.EndOfStreamException ex) { }
                        catch (System.NotSupportedException ex) { }

                        var serializer = new DataContractSerializer(typeof(T));
                        using (var ms = new MemoryStream(bytesUncompressed))
                        {
                            //if (length == 0) { return default(T); }
                            return (T)serializer.ReadObject(ms);
                        }
                    }

                }
            }
        }

    }
}