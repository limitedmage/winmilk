// ----------------------------------------------------------------------------------
// Microsoft Developer & Platform Evangelism
// 
// Copyright (c) Microsoft Corporation. All rights reserved.
// 
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND, 
// EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED WARRANTIES 
// OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE.
// ----------------------------------------------------------------------------------
// The example companies, organizations, products, domain names,
// e-mail addresses, logos, people, places, and events depicted
// herein are fictitious.  No association with any real company,
// organization, product, domain name, email address, logo, person,
// places, or events is intended or should be inferred.
// ----------------------------------------------------------------------------------

using System.IO;
using System.IO.IsolatedStorage;
using System.Runtime.Serialization.Json;
using System.Text;

namespace WinMilk.Helper
{
	public static class IsolatedStorageHelper
	{
		public static T GetObject<T>(string key)
		{
			if (IsolatedStorageSettings.ApplicationSettings.Contains(key)) 
			{
				string serializedObject = IsolatedStorageSettings.ApplicationSettings[key].ToString();
				return Deserialize<T>(serializedObject);
			}
			return default(T);
		}

		public static void SaveObject<T>(string key, T objectToSave)
		{
			string serializedObject = Serialize(objectToSave);
			IsolatedStorageSettings.ApplicationSettings[key] = serializedObject;
		}

		public static void DeleteObject(string key)
		{
			IsolatedStorageSettings.ApplicationSettings.Remove(key);
		}

		private static string Serialize(object objectToSerialize)
		{
			using (MemoryStream ms = new MemoryStream())
			{
				DataContractJsonSerializer serializer = new DataContractJsonSerializer(objectToSerialize.GetType());
				serializer.WriteObject(ms, objectToSerialize);
				ms.Position = 0;

				using (StreamReader reader = new StreamReader(ms))
				{
					return reader.ReadToEnd();
				}
			}
		}

		private static T Deserialize<T>(string jsonString)
		{
			using (MemoryStream ms = new MemoryStream(Encoding.Unicode.GetBytes(jsonString)))
			{
				DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(T));
				return (T)serializer.ReadObject(ms);
			}
		}
	}
}