using System;
using System.Collections;
using System.Management.Automation;
using Id.PowershellExtensions.DatabaseMigrations;
using Migrator.Compile;
using System.Reflection;

namespace Id.PowershellExtensions
{
    [Cmdlet(VerbsCommon.New, "DatabaseWindowsLogin", SupportsShouldProcess = true)]
    public class NewDatabaseWindowsLogin : PSCmdlet
    {
        [Parameter(Mandatory = true, Position = 1, ValueFromPipelineByPropertyName = true)]
        public string UserName { get; set; }

        [Parameter(Mandatory = false, Position = 2, ValueFromPipelineByPropertyName = true)]
        public string Directory { get; set; }

        [Parameter(Mandatory = false, Position = 3, ValueFromPipelineByPropertyName = true)]
        public string MigrationsAssemblyPath { get; set; }

        [Parameter(Mandatory = false, Position = 4, ValueFromPipelineByPropertyName = true)]
        public string Language { get; set; }

        [Parameter(Mandatory = false, Position = 5, ValueFromPipelineByPropertyName = true)]
        public string Provider { get; set; }

        [Parameter(Mandatory = false, Position = 6, ValueFromPipelineByPropertyName = true)]
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
                var newWindowsLogin = new NewWindowsLogin(new TaskLogger(this), UserName);

                if (!string.IsNullOrEmpty(this.Directory))
                {
                    ScriptEngine engine = new ScriptEngine(this.Language, null);
                    newWindowsLogin.Execute(engine.Compile(this.Directory));
                }
                
                if (MigrationsAssemblyPath != null)
                {
                    Assembly asm = Assembly.LoadFrom(this.MigrationsAssemblyPath);
                    newWindowsLogin.Execute(asm);

                }
                else if (Settings != null)
                {
                    newWindowsLogin.Execute(Settings.ToDictionary());
                }
            }
            catch (Exception e)
            {
                ThrowTerminatingError(
                    new ErrorRecord(
                        e,
                        "New-DatabaseWindowsLogin",
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
