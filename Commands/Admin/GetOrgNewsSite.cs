﻿using Microsoft.SharePoint.Client;

using PnP.PowerShell.Commands.Base;
using System.Management.Automation;

namespace PnP.PowerShell.Commands.Admin
{
    [Cmdlet(VerbsCommon.Get, "OrgNewsSite")]
    public class GetOrgNewsSite : PnPAdminCmdlet
    {
        protected override void ExecuteCmdlet()
        {
            var results = Tenant.GetOrgNewsSites();
            ClientContext.ExecuteQueryRetry();
            WriteObject(results, true);
        }
    }
}