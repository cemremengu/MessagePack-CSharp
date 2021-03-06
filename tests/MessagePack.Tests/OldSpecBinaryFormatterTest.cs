﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using MessagePack.Formatters;
using MessagePack.Resolvers;
using Xunit;

namespace MessagePack.Tests
{
    public class OldSpecBinaryFormatterTest
    {
        [Theory]
        [InlineData(10)] // fixstr
        [InlineData(1000)] // str 16
        [InlineData(100000)] // str 32
        public void Serialize(int arrayLength)
        {
            var sourceBytes = Enumerable.Range(0, arrayLength).Select(i => unchecked((byte)i)).ToArray(); // long byte array
            byte[] messagePackBytes = null;
            var length = OldSpecBinaryFormatter.Instance.Serialize(ref messagePackBytes, 0, sourceBytes, StandardResolver.Instance);
            Assert.NotEmpty(messagePackBytes);
            Assert.Equal(length, messagePackBytes.Length);

            var deserializedBytes = DeserializeByClassicMsgPack<byte[]>(messagePackBytes);
            Assert.Equal(sourceBytes, deserializedBytes);
        }

        [Fact]
        public void SerializeNil()
        {
            byte[] sourceBytes = null;
            byte[] messagePackBytes = null;
            var length = OldSpecBinaryFormatter.Instance.Serialize(ref messagePackBytes, 0, sourceBytes, StandardResolver.Instance);
            Assert.NotEmpty(messagePackBytes);
            Assert.Equal(length, messagePackBytes.Length);
            Assert.Equal(MessagePackCode.Nil, messagePackBytes[0]); 

            var deserializedBytes = DeserializeByClassicMsgPack<byte[]>(messagePackBytes);
            Assert.Null(deserializedBytes);
        }

        [Theory]
        [InlineData(10)] // fixstr
        [InlineData(1000)] // str 16
        [InlineData(100000)] // str 32
        public void Deserialize(int arrayLength)
        {
            var sourceBytes = Enumerable.Range(0, arrayLength).Select(i => unchecked((byte) i)).ToArray(); // long byte array
            var messagePackBytes = SerializeByClassicMsgPack(sourceBytes); 

            var deserializedBytes = OldSpecBinaryFormatter.Instance.Deserialize(messagePackBytes, 0, StandardResolver.Instance, out var readSize);
            Assert.NotNull(deserializedBytes);
            Assert.Equal(sourceBytes, deserializedBytes);
        }

        [Fact]
        public void DeserializeNil()
        {
            var messagePackBytes = new byte[]{ MessagePackCode.Nil }; 

            var deserializedObj = OldSpecBinaryFormatter.Instance.Deserialize(messagePackBytes, 0, StandardResolver.Instance, out var readSize);
            Assert.Null(deserializedObj);
        }

        private static byte[] SerializeByClassicMsgPack<T>(T obj)
        {
            var context = new MsgPack.Serialization.SerializationContext
            {
                SerializationMethod = MsgPack.Serialization.SerializationMethod.Array,
                CompatibilityOptions = { PackerCompatibilityOptions = MsgPack.PackerCompatibilityOptions.Classic }
            };

            var serializer = MsgPack.Serialization.MessagePackSerializer.Get<T>(context);
            using (var memory = new MemoryStream())
            {
                serializer.Pack(memory, obj);
                return memory.ToArray();
            }
        }

        private static T DeserializeByClassicMsgPack<T>(byte[] messagePackBytes)
        {
            var context = new MsgPack.Serialization.SerializationContext
            {
                SerializationMethod = MsgPack.Serialization.SerializationMethod.Array,
                CompatibilityOptions = { PackerCompatibilityOptions = MsgPack.PackerCompatibilityOptions.Classic }
            };

            var serializer = MsgPack.Serialization.MessagePackSerializer.Get<T>(context);
            using (var memory = new MemoryStream(messagePackBytes))
            {
                return serializer.Unpack(memory);
            }
        }
    }
}
