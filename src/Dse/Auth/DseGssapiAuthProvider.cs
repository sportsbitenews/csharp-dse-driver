﻿//
//  Copyright (C) 2016 DataStax, Inc.
//
//  Please see the license for details:
//  http://www.datastax.com/terms/datastax-dse-driver-license-terms
//
#if !NETCORE
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using Dse;

namespace Dse.Auth
{
    /// <summary>
    /// Provides GSSAPI authenticator instances for clients to connect to DSE clusters secured with the
    /// DseAuthenticator.
    /// </summary>
    public class DseGssapiAuthProvider : IAuthProviderNamed
    {
        private static readonly byte[] Mechanism = Encoding.UTF8.GetBytes("GSSAPI");
        private static readonly byte[] InitialServerChallenge = Encoding.UTF8.GetBytes("GSSAPI-START");
        private string _name;
        private readonly string _service;
        private readonly Func<IPEndPoint, string> _hostNameResolver;

        /// <summary>
        /// Creates a new instance of <see cref="DseGssapiAuthProvider"/>.
        /// </summary>
        /// <param name="service">Name of the service. Defaults to &quot;dse&quot;</param>
        /// <param name="hostNameResolver">
        /// Method to resolve the host name given the endpoint. Defaults to <see cref="UseIpResolver(IPEndPoint)"/>.
        /// </param>
        public DseGssapiAuthProvider(string service = "dse", Func<IPEndPoint, string> hostNameResolver = null)
        {
            _service = service;
            _hostNameResolver = hostNameResolver ?? ReverseDnsResolver;
        }

        /// <summary>
        /// Returns a new <see cref="IAuthenticator"/> instance to handle authentication for a given endpoint.
        /// </summary>
        /// <exception cref="AuthenticationException">When the host name can not be resolved.</exception>
        public IAuthenticator NewAuthenticator(IPEndPoint host)
        {
            var hostName = _hostNameResolver(host);
            return new GssapiAuthenticator(_name, hostName, _service);
        }

        /// <inheritdoc />
        public void SetName(string name)
        {
            _name = name;
        }

        /// <summary>
        /// Returns the IP address of the endpoint as a string.
        /// </summary>
        public static string UseIpResolver(IPEndPoint endpoint)
        {
            return endpoint.Address.ToString();
        }

        /// <summary>
        /// Performs a reverse DNS query that resolves an IPv4 or IPv6 address to a hostname.
        /// </summary>
        public static string ReverseDnsResolver(IPEndPoint endpoint)
        {
            var hostEntry = Dns.GetHostEntry(endpoint.Address);
            if (hostEntry == null || hostEntry.HostName == null)
            {
                throw new AuthenticationException("Host name could not be resolved", endpoint);
            }
            return hostEntry.HostName;
        }

        private class GssapiAuthenticator : BaseAuthenticator, IDisposable
        {
            private readonly string _hostName;
            private readonly string _service;
            private readonly IGssapiClient _client;

            public GssapiAuthenticator(string authenticatorName, string hostName, string service) : 
                base(authenticatorName)
            {
                _hostName = hostName;
                _service = service;
                _client = GssapiClientFactory.CreateNew();
            }

            protected override byte[] GetMechanism()
            {
                return Mechanism;
            }

            protected override byte[] GetInitialServerChallenge()
            {
                return InitialServerChallenge;
            }

            public override byte[] InitialResponse()
            {
                _client.Init(_service, _hostName);
                if (!IsDseAuthenticator())
                {
                    //fallback to evaluate first challenge
                    return EvaluateChallenge(null);
                }
                //Start the SASL flow providing the name of the mechanism
                //DSE 5 or above
                return GetMechanism();
            }

            public override byte[] EvaluateChallenge(byte[] challenge)
            {
                return _client.EvaluateChallenge(challenge);
            }

            public void Dispose()
            {
                _client.Dispose();
            }
        }
    }
}
#endif