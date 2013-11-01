﻿using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace HappyBin.AutoUpdater
{
	public class UpdateConfig
	{
		public UpdateConfig()
		{
		}

		[XmlElement()]
		public string CurrentVersion { get; set; }
		[XmlIgnore()]
		public Version CurrVer { get { return new Version( this.CurrentVersion ); } }
		[XmlElement()]
		public bool IsMandatory { get; set; }
		[XmlElement()]
		public string PatchUri { get; set; }


		public void Serialize(string filePath)
		{
			XmlSerializer s = new XmlSerializer( typeof( UpdateConfig ) );
			XmlTextWriter w = new XmlTextWriter( filePath, System.Text.Encoding.ASCII );
			w.Formatting = Formatting.Indented;
			s.Serialize( w, this );
			w.Close();
		}

		public static UpdateConfig Deserialize(TextReader reader)
		{
			XmlSerializer s = new XmlSerializer( typeof( UpdateConfig ) );
			return (UpdateConfig)s.Deserialize( reader );
		}
	}
}