using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

namespace Q42.Mvc.XsltViewEngine
{
  /// <summary>
  /// Static class used for serializing an object to or from xml.
  /// </summary>
  public class Serializer
  {
    /// <summary>
    /// private dictionary of specific serializers, used to speed up serialization of generic objects.
    /// </summary>
    private static Dictionary<Type, XmlSerializer> specificSerializers = new Dictionary<Type, XmlSerializer>();

    /// <summary>
    /// A dictionary of specific serializers per type. Serializers added here are used instead of the
    /// default serializer, when an object of the specified type is being serialized. Use this mechanism
    /// to add a serializer with a large array of pre-know types.
    /// </summary>
    public static Dictionary<Type, XmlSerializer> SpecificSerializers
    {
      get { return specificSerializers; }
    }

    /// <summary>
    /// Serializes an object to xml. Casing of objectnames and parameternames is left unchanged.
    /// </summary>
    /// <param name="obj">Object to seriaize.</param>
    /// <returns>Xml representation of the object.</returns>
    public static XmlNode ToXml(object obj)
    {
      Type type = obj.GetType();
      XmlSerializer serializer;
      if (specificSerializers.ContainsKey(type))
        serializer = specificSerializers[type];
      else
        serializer = new XmlSerializer(type);

      XmlDocument doc = new XmlDocument();
      using (XmlWriter writer = doc.CreateNavigator().AppendChild())
      {
        serializer.Serialize(writer, obj);
      }
      return doc;
    }

    /// <summary>
    /// Deserializes an object from xml back to an object.
    /// </summary>
    /// <typeparam name="T">Type of the object to return.</typeparam>
    /// <param name="xml">Xml source to deserialize.</param>
    /// <returns>An instance of type T.</returns>
    public static T ToObject<T>(XmlNode xml) where T : class
    {
      Type type = typeof(T);
      XmlSerializer serializer;
      if (specificSerializers.ContainsKey(type))
        serializer = specificSerializers[type];
      else
        serializer = new XmlSerializer(type);

      XmlNodeReader reader = new XmlNodeReader(xml);
      object obj = serializer.Deserialize(reader);
      return obj as T;
    }
  }
}