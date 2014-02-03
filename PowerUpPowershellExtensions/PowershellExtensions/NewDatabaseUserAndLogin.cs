using System;
using System.Management.Automation;
using Id.PowershellExtensions.DatabaseMigrations;
using Migrator.Compile;
using System.Reflection;

namespace Id.PowershellExtensions
{
    [Cmdlet(VerbsCommon.New, "DatabaseUserAndLogin", SupportsShouldProcess = true)]
    public class NewDatabaseUserAndLogin : PSCmdlet
    {
        [Parameter(Mandatory = true, Position = 1, ValueFromPipelineByPropertyName = true)]
        public string UserName { get; set; }

        [Parameter(Mandatory = true, Position = 2, ValueFromPipelineByPropertyName = true)]
        public string Password { get; set; }

        [Parameter(Mandatory = false, Position = 3, ValueFromPipelineByPropertyName = true)]
        public string Directory { get; set; }

        [Parameter(Mandatory = false, Position = 4, ValueFromPipelineByPropertyName = true)]
        public string MigrationsAssemblyPath { get; set; }

        [Parameter(Mandatory = false, Position = 5, ValueFromPipelineByPropertyName = true)]
        public string Language { get; set; }

        [Parameter(Mandatory = false, Position = 6, ValueFromPipelineByPropertyName = true)]
        public string Provider { get; set; }

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
                var newUserAndLogin = new NewUserAndLogin(new TaskLogger(this), Provider, UserName, Password);

                if (!string.IsNullOrEmpty(this.Directory))
                {
                    ScriptEngine engine = new ScriptEngine(this.Language, null);
                    newUserAndLogin.Execute(engine.Compile(this.Directory));
                }
                if (null != this.MigrationsAssemblyPath)
                {
                    Assembly asm = Assembly.LoadFrom(this.MigrationsAssemblyPath);
                    newUserAndLogin.Execute(asm);
                }
            }
            catch (Exception e)
            {
                ThrowTerminatingError(
                    new ErrorRecord(
                        e,
                        "New-DatabaseUserAndLogin",
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
