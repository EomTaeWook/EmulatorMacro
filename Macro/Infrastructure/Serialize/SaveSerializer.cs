﻿using Macro.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using Utils.Infrastructure;

namespace Macro.Infrastructure.Serialize
{
    public class ObjectSerializer
    {
        public static byte[] SerializeObject<T>(T model)
        {
            var properties = model.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance)
                                            .Where(r => r.CanRead && r.CanWrite).OrderBy(r => r.Name).ToList();

            using (var ms = new MemoryStream())
            {
                var bf = new BinaryFormatter();
                bf.Serialize(ms, "\uFF1C");
                foreach (var prop in properties)
                {
                    var val = prop.GetValue(model);
                    val = val ?? Activator.CreateInstance(prop.PropertyType);
                    bf.Serialize(ms, val);
                }
                bf.Serialize(ms, "\uFF1E");
                return ms.ToArray();
            }
        }
        public static List<T> DeserializeObject<T>(byte[] data)
        {
            var list = new List<T>();
            using (var ms = new MemoryStream(data))
            {
                var bf = new BinaryFormatter();
                var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                                            .Where(r => r.CanRead && r.CanWrite).OrderBy(r => r.Name).ToList();

                while (ms.Position < ms.Length)
                {
                    var startTag = bf.Deserialize(ms);
                    if (!startTag.Equals("\uFF1C"))
                    {
                        throw new FormatException(DocumentHelper.Get(Utils.Document.Message.FailedFileBroken));
                    }
                    var @object = (T)Activator.CreateInstance(typeof(T));
                    bool isComplete = true;
                    foreach (var prop in properties)
                    {
                        var val = bf.Deserialize(ms);
                        if (val.Equals("\uFF1E"))
                        {
                            isComplete = false;
                            break;
                        }
                        prop.SetValue(@object, val);
                    }
                    if(isComplete)
                    {
                        var endTag = bf.Deserialize(ms);
                        if(!endTag.Equals("\uFF1E"))
                            throw new FormatException(DocumentHelper.Get(Utils.Document.Message.FailedFileBroken));
                    }
                    list.Add(@object);
                }
            }
            return list;
        }
    }
}
