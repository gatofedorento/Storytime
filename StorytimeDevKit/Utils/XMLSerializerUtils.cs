﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.IO;
using System.Xml;

namespace StoryTimeDevKit.Utils
{
    public static class XMLSerializerUtils
    {
        public static void SerializeToXML<TData>(TData data, string path) where TData : class
        {
            XmlSerializer serializer = new XmlSerializer(typeof(TData));
            TextWriter textWriter = new StreamWriter(path);
            serializer.Serialize(textWriter, data);
            textWriter.Close();
        }

        public static TData DeserializeFromXML<TData>(string filePath) where TData : class
        {
            XmlSerializer deserializer = new XmlSerializer(typeof(TData));
            StreamReader stream = new StreamReader(filePath);
            
            using(XmlReader xml = XmlReader.Create(stream))
            {
                if (!deserializer.CanDeserialize(xml))
                {
                    return null;
                }
            }

            //after being used by the XmlReader the stream has to be reset
            stream.DiscardBufferedData();
            stream.BaseStream.Seek(0, SeekOrigin.Begin);
            stream.BaseStream.Position = 0;

            TData data = deserializer.Deserialize(stream) as TData;
            stream.Close();
            return data;
        }
    }
}
