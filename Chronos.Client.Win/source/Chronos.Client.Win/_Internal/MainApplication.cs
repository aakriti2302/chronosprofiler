﻿using System;
using Chronos.Client.Win.ViewModels;
using Chronos.Client.Win.ViewModels.Home;

namespace Chronos.Client.Win
{
    internal sealed class MainApplication : ApplicationBase, IMainApplication
    {
        public MainApplication()
            : base(Guid.NewGuid())
        {
        }

        public MainApplication(bool processOwner)
            : base(Guid.NewGuid(), processOwner)
        {
        }

        protected override PageViewModel BuildMainViewModel()
        {
            return new HomePageViewModel(this);
        }

        protected override void ConfigureContainer(IContainer container)
        {
            container.RegisterInstance<IMainApplication>(this);
            base.ConfigureContainer(container);
        }

        protected override void RunInternal()
        {
            base.RunInternal();
            Host.ConnectionManager connectionManager = new Host.ConnectionManager();
            connectionManager.RestoreConnections(HostApplications, ApplicationSettings.HostConnections);
        }

        public override void Dispose()
        {
            base.Dispose();
            Host.ApplicationManager.Shutdown();
            Daemon.ApplicationManager.Shutdown();
        }

        protected override Extensibility.Catalog LoadCatalog()
        {
            return base.LoadCatalog();
        }
    }
}
