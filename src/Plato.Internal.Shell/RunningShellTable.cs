﻿using System;
using System.Collections.Generic;
using System.Threading;
using Plato.Internal.Shell.Abstractions.Models;

namespace Plato.Internal.Shell
{
    public class RunningShellTable : IRunningShellTable
    {
        private readonly Dictionary<string, ShellSettings> _shellsByHostAndPrefix =
            new Dictionary<string, ShellSettings>(StringComparer.OrdinalIgnoreCase);

        private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();

        private ShellSettings _single;
        private ShellSettings _default;

        public void Add(ShellSettings settings)
        {
            _lock.EnterWriteLock();
            try
            {
                // _single is set when there is only a single tenant
                if (_single != null)
                {
                    _single = null;
                }
                else
                {
                    _single = settings;
                }

                if (ShellHelper.DefaultShellName == settings.Name)
                {
                    _default = settings;
                }

                var hostAndPrefix = GetHostAndPrefix(settings);
                _shellsByHostAndPrefix[hostAndPrefix] = settings;
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public void Remove(ShellSettings settings)
        {
            _lock.EnterWriteLock();
            try
            {
                var hostAndPrefix = GetHostAndPrefix(settings);
                _shellsByHostAndPrefix.Remove(hostAndPrefix);

                if (_default == settings)
                {
                    _default = null;
                }

                if (_single == settings)
                {
                    _single = null;
                }
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public ShellSettings Match(string host, string appRelativePath)
        {
            _lock.EnterReadLock();
            try
            {
                if (_single != null)
                {
                    return _single;
                }

                var hostAndPrefix = GetHostAndPrefix(host, appRelativePath);
                ShellSettings result;
                if (!_shellsByHostAndPrefix.TryGetValue(hostAndPrefix, out result))
                {
                    var noHostAndPrefix = GetHostAndPrefix("", appRelativePath);
                    if (!_shellsByHostAndPrefix.TryGetValue(noHostAndPrefix, out result))
                    {
                        result = _default;
                    }
                }

                return result;
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public IDictionary<string, ShellSettings> ShellsByHostAndPrefix
        {
            get
            {
                return _shellsByHostAndPrefix;
            }
        }
        

        private string GetHostAndPrefix(string host, string appRelativePath)
        {
            // removing the port from the host
            var hostLength = host.IndexOf(':');
            if (hostLength != -1)            
                host = host.Substring(0, hostLength);
            
            // appRelativePath starts with /
            int firstSegmentIndex = appRelativePath.IndexOf('/', 1);
            if (firstSegmentIndex > -1)            
                return host + appRelativePath.Substring(0, firstSegmentIndex);            
            else            
                return host + appRelativePath;            

        }

        private string GetHostAndPrefix(ShellSettings shellSettings)
        {
            return shellSettings.RequestedUrlHost + "/" + shellSettings.RequestedUrlPrefix;
        }
    }
}
