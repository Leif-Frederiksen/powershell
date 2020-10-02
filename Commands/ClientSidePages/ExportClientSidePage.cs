﻿using PnP.Framework.Provisioning.Connectors;
using PnP.Framework.Provisioning.Model;
using PnP.Framework.Provisioning.Model.Configuration;
using PnP.Framework.Provisioning.ObjectHandlers;

using PnP.PowerShell.Commands.Base.PipeBinds;
using PnP.PowerShell.Commands.Properties;
using System;
using System.IO;
using System.Management.Automation;

namespace PnP.PowerShell.Commands.Provisioning.Tenant
{
    [Cmdlet(VerbsData.Export, "ClientSidePage")]
    public class ExportClientSidePage : PnPWebCmdlet
    {
        [Parameter(Mandatory = true, ValueFromPipeline = true, Position = 0)]
        public ClientSidePagePipeBind Identity;

        [Parameter(Mandatory = false)]
        public SwitchParameter PersistBrandingFiles;

        [Parameter(Mandatory = false)]
        public string Out;

        [Parameter(Mandatory = false)]
        public SwitchParameter Force;

        [Parameter(Mandatory = false)]
        public ExtractConfigurationPipeBind Configuration;

        protected override void ProcessRecord()
        {
            ExtractConfiguration extractConfiguration = null;
            if (ParameterSpecified(nameof(Configuration)))
            {
                extractConfiguration = Configuration.GetConfiguration(SessionState.Path.CurrentFileSystemLocation.Path);
            }

            if (!string.IsNullOrEmpty(Out))
            {
                if (!Path.IsPathRooted(Out))
                {
                    Out = Path.Combine(SessionState.Path.CurrentFileSystemLocation.Path, Out);
                }
                if (System.IO.File.Exists(Out))
                {
                    if (Force || ShouldContinue(string.Format(Resources.File0ExistsOverwrite, Out), Resources.Confirm))
                    {
                        ExtractTemplate(new FileInfo(Out).DirectoryName, new FileInfo(Out).Name, extractConfiguration);
                    }
                }
                else
                {
                    ExtractTemplate(new FileInfo(Out).DirectoryName, new FileInfo(Out).Name, extractConfiguration);
                }
            }
            else
            {
                ExtractTemplate(null, null, extractConfiguration);
            }
        }


        private void ExtractTemplate(string dirName, string fileName, ExtractConfiguration configuration)
        {
            var outputTemplate = new ProvisioningTemplate();
            outputTemplate.Id = $"TEMPLATE-{Guid.NewGuid():N}".ToUpper();
            var helper = new PnP.Framework.Provisioning.ObjectHandlers.Utilities.ClientSidePageContentsHelper();
            ProvisioningTemplateCreationInformation ci = null;
            if (configuration != null)
            {
                ci = configuration.ToCreationInformation(SelectedWeb);
            }
            else
            {
                ci = new ProvisioningTemplateCreationInformation(SelectedWeb);
            }
            if (ParameterSpecified(nameof(PersistBrandingFiles)))
            {
                ci.PersistBrandingFiles = PersistBrandingFiles;
            }
            if (!string.IsNullOrEmpty(dirName))
            {
                var fileSystemConnector = new FileSystemConnector(dirName, "");
                ci.FileConnector = fileSystemConnector;
            }
            helper.ExtractClientSidePage(SelectedWeb, outputTemplate, ci, new PnP.Framework.Diagnostics.PnPMonitoredScope(), null, Identity.Name, false);

            if (!string.IsNullOrEmpty(fileName))
            {
                System.IO.File.WriteAllText(Path.Combine(dirName, fileName), outputTemplate.ToXML());
            }
            else
            {
                WriteObject(outputTemplate.ToXML());
            }
        }
    }

}