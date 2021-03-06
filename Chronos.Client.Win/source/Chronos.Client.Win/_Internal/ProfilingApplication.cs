﻿using Chronos.Client.Win.ViewModels;
using Chronos.Client.Win.ViewModels.Profiling;
using Chronos.Extensibility;
using System;
using System.Collections.Generic;

namespace Chronos.Client.Win
{
    internal sealed class ProfilingApplication : ApplicationBase, IProfilingApplication, ILifetimeSponsor
    {
        private ISession _session;
        private ConfigurationSettings _configurationSettings;
        private readonly Guid _sessionUid;

        public ProfilingApplication(Guid sessionUid)
            : base(sessionUid.ReverseBits())
        {
            _sessionUid = sessionUid;
        }

        public ProfilingApplication(Guid sessionUid, bool processOwner)
            : base(sessionUid.ReverseBits(), processOwner)
        {
            _sessionUid = sessionUid;
        }

        public IProfilingTimer ProfilingTimer { get; private set; }

        protected override PageViewModel BuildMainViewModel()
        {
            return new ProfilingViewModel(this);
        }

        protected override void ConfigureContainer(IContainer container)
        {
            container.RegisterInstance<IProfilingApplication>(this);
            base.ConfigureContainer(container);
        }

        protected override void RunInternal()
        {
            base.RunInternal();
            RunProfilingTypes();
            RunProductivities();
        }

        private void RunProfilingTypes()
        {
            ProfilingTypeSettingsCollection profilingTypesSettings = _configurationSettings.ProfilingTypesSettings;
            List<IProfilingTypeAdapter> adapters = new List<IProfilingTypeAdapter>();
            foreach (ProfilingTypeSettings profilingTypeSettings in profilingTypesSettings)
            {
                IProfilingType profilingType = ProfilingTypes[profilingTypeSettings.Uid];
                IProfilingTypeAdapter adapter = profilingType.GetWinAdapter();
                adapters.Add(adapter);
            }
            foreach (IProfilingTypeAdapter adapter in adapters)
            {
                IServiceConsumer serviceConsumer = adapter as IServiceConsumer;
                if (serviceConsumer != null)
                {
                    serviceConsumer.ExportServices(_session.ServiceContainer);
                }
            }
            foreach (IProfilingTypeAdapter adapter in adapters)
            {
                IServiceConsumer serviceConsumer = adapter as IServiceConsumer;
                if (serviceConsumer != null)
                {
                    serviceConsumer.ImportServices(_session.ServiceContainer);
                }
            }
        }

        private void RunProductivities()
        {
            List<IProductivityAdapter> adapters = new List<IProductivityAdapter>();
            foreach (IProductivity productivity in Productivities)
            {
                IProductivityAdapter adapter = productivity.GetWinAdapter();
                adapters.Add(adapter);
            }
            foreach (IProductivityAdapter adapter in adapters)
            {
                IServiceConsumer serviceConsumer = adapter as IServiceConsumer;
                if (serviceConsumer != null)
                {
                    serviceConsumer.ExportServices(_session.ServiceContainer);
                }
            }
            foreach (IProductivityAdapter adapter in adapters)
            {
                IServiceConsumer serviceConsumer = adapter as IServiceConsumer;
                if (serviceConsumer != null)
                {
                    serviceConsumer.ImportServices(_session.ServiceContainer);
                }
            }
        }

        protected override IServiceContainer CreateServiceContainer()
        {
            return _session.ServiceContainer;
        }

        //TODO: reimplement
        protected override Catalog LoadCatalog()
        {
            Catalog catalog = base.LoadCatalog();
            _session = FindAndStartSession(_sessionUid);
            if (_session == null)
            {
                throw new TempException();
            }
            _configurationSettings = _session.GetConfigurationSettings();
            catalog = CatalogFilter.Filter(catalog, _configurationSettings);
            return catalog;
        }

        private ISession FindAndStartSession(Guid sessionUid)
        {
            ISession session = null;
            Host.IApplicationCollection hostApplications = new Host.ApplicationCollection();
            //TODO: bad design - you need to reload the list of Host Applications from settings
            hostApplications.ConnectLocal(false);
            foreach (Host.IApplication hostApplication in hostApplications)
            {
                if (hostApplication.Sessions.Contains(sessionUid))
                {
                    session = hostApplication.Sessions[sessionUid];
                    break;
                }
            }
            if (session != null)
            {
                session.StartDecoding(this);
            }
            ProfilingTimer = session.ProfilingTimer;
            //hostApplications.TryDispose();
            return session;
        }

        public void FlushData()
        {
            _session.FlushData();
        }

        public bool ShouldStayAlive()
        {
            return true;
        }
    }
}
