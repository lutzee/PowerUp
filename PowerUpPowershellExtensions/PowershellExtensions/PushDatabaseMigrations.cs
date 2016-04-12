using System;
using System.Collections;
using System.Management.Automation;
using Id.DatabaseMigration;
using Id.PowershellExtensions.DatabaseMigrations;
using Migrator.Compile;
using System.Reflection;

namespace Id.PowershellExtensions
{
    [Cmdlet(VerbsCommon.Push, "DatabaseMigrations", SupportsShouldProcess = true)]
    public class PushDatabaseMigrations : PSCmdlet
    {
        [Parameter(Mandatory = false, Position = 1, ValueFromPipelineByPropertyName = true)]
        public string Directory { get; set; }

        [Parameter(Mandatory = false, Position = 2, ValueFromPipelineByPropertyName = true)]
        public bool DryRun { get; set; }

        [Parameter(Mandatory = false, Position = 3, ValueFromPipelineByPropertyName = true)]
        public string Language { get; set; }

        [Parameter(Mandatory = false, Position = 4, ValueFromPipelineByPropertyName = true)]
        public string MigrationsAssemblyPath { get; set; }

        [Parameter(Mandatory = false, Position = 5, ValueFromPipelineByPropertyName = true)]
        public string Provider { get; set; }

        [Parameter(Mandatory = false, Position = 6, ValueFromPipelineByPropertyName = true)]
        public long VersionTo { get; set; }

        [Parameter(Mandatory = false, Position = 7, ValueFromPipelineByPropertyName = true)]
        public bool Trace { get; set; }

        [Parameter(Mandatory = false, Position = 8, ValueFromPipelineByPropertyName = true)]
        public Hashtable Settings { get; set; }

        protected override void BeginProcessing()
        {
        }

        protected override void ProcessRecord()
        {
            if (string.IsNullOrEmpty(Language))
                Language = "CSharp";

            if (string.IsNullOrEmpty(Provider))
                Provider = "SqlServer";

            try
            {
                var db = new DatabaseMigrator(new TaskLogger(this), DryRun, Provider, VersionTo, Trace);

                if (!string.IsNullOrEmpty(this.Directory))
                {
                    var engine = new ScriptEngine(this.Language, null);
                    db.Execute(engine.Compile(this.Directory));
                }

                if (this.MigrationsAssemblyPath != null)
                {
                    Assembly asm = Assembly.LoadFrom(this.MigrationsAssemblyPath);
                    if (Settings == null)
                    {
                        db.Execute(asm);
                    }
                    else
                    {
                        db.Execute(Settings.ToDictionary(), asm);
                    }
                }
            }
            catch (Exception e)
            {
                ThrowTerminatingError(
                    new ErrorRecord(
                        e,
                        "Push-DatabaseMigrations",
                        ErrorCategory.NotSpecified,
                        this
                        )
                    );
            }
        }

        protected override void EndProcessing()
        {
        }
    }
}
