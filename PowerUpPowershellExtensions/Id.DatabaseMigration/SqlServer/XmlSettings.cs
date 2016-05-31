using System;
using System.IO;
using System.Reflection;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;

namespace Id.DatabaseMigration.SqlServer
{
    public class XmlSettings : SqlSettings, ISqlServerSettings
    {
        protected readonly Assembly assemblyOverride;

        protected string ConfigurationDirectory
        {
            get
            {
                Assembly assembly = this.GetType().Assembly;
                if (this.assemblyOverride != null)
                    assembly = this.assemblyOverride;
                return Path.GetDirectoryName(Uri.UnescapeDataString(new UriBuilder(assembly.CodeBase).Path));
            }
        }


        protected virtual string Location
        {
            get
            {
                return string.Format("{0}\\migrations.config", (object)this.ConfigurationDirectory);
            }
        }

        protected virtual string SchemaLocation
        {
            get
            {
                return string.Format("{0}\\migrations.xsd", (object)this.ConfigurationDirectory);
            }
        }

        public XmlSettings()
        {
            this.ParseSettingsXml();
        }

        public XmlSettings(Assembly migrationAssembly)
        {
            this.assemblyOverride = migrationAssembly;
            this.ParseSettingsXml();
        }

        protected virtual void ParseSettingsXml()
        {
            XDocument source = XDocument.Load(this.Location);
            if (source == null)
                throw new FileNotFoundException("No config file found");
            XmlSchemaSet schemas = new XmlSchemaSet();
            using (StreamReader streamReader = new StreamReader(this.SchemaLocation))
                schemas.Add("", XmlReader.Create((TextReader)streamReader));
            System.Xml.Schema.Extensions.Validate(source, schemas, (ValidationEventHandler)((sender, e) =>
            {
                throw new XmlSchemaValidationException(e.Message);
            }));
            XElement root = source.Root;
            this.Server = root.Element((XName)"server").Value;
            this.DatabaseName = root.Element((XName)"database").Value;
            this.DefaultUserName = root.Element((XName)"defaultuser").Value;
            this.DefaultUserPassword = root.Element((XName)"defaultuserpassword").Value;
            this.MigrationUserName = root.Element((XName)"migrationuser").Value;
            this.MigrationUserPassword = root.Element((XName)"migrationuserpassword").Value;
        }
    }
}
