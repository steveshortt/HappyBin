using System;
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
		public Version CurrentVer
		{
			get
			{
				if( string.IsNullOrWhiteSpace( this.CurrentVersion ) )
				{
					return new Version( 0, 0, 0, 0 );
				}
				else
				{
					return new Version( this.CurrentVersion );
				}
			}
		}

		[XmlElement()]
		public string LastMandatoryVersion { get; set; }
		[XmlIgnore()]
		public Version LastMandatoryVer
		{
			get
			{
				if( string.IsNullOrWhiteSpace( this.LastMandatoryVersion ) )
				{
					return new Version( 0, 0, 0, 0 );
				}
				else
				{
					return new Version( this.LastMandatoryVersion );
				}
			}
		}

		[XmlElement()]
		public bool IsMandatory { get; set; }
		[XmlElement()]
		public string PatchUri { get; set; }
		[XmlElement()]
		public long PatchSizeBytes { get; set; }


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