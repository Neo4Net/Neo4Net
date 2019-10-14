using System;
using System.Collections.Generic;

/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4j Enterprise Edition. The included source
 * code can be redistributed and/or modified under the terms of the
 * GNU AFFERO GENERAL PUBLIC LICENSE Version 3
 * (http://www.fsf.org/licensing/licenses/agpl-3.0.html) with the
 * Commons Clause, as found in the associated LICENSE.txt file.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 *
 * Neo4j object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@neo4j.com
 *
 * More information is also available at:
 * https://neo4j.com/licensing/
 */
namespace Neo4Net.causalclustering.discovery
{
	using ObjectMapper = org.codehaus.jackson.map.ObjectMapper;
	using HttpClient = org.eclipse.jetty.client.HttpClient;
	using ContentResponse = org.eclipse.jetty.client.api.ContentResponse;
	using HttpHeader = org.eclipse.jetty.http.HttpHeader;
	using HttpMethod = org.eclipse.jetty.http.HttpMethod;
	using MimeTypes = org.eclipse.jetty.http.MimeTypes;
	using SslContextFactory = org.eclipse.jetty.util.ssl.SslContextFactory;


	using CausalClusteringSettings = Neo4Net.causalclustering.core.CausalClusteringSettings;
	using KubernetesType = Neo4Net.causalclustering.discovery.kubernetes.KubernetesType;
	using ServiceList = Neo4Net.causalclustering.discovery.kubernetes.ServiceList;
	using Status = Neo4Net.causalclustering.discovery.kubernetes.Status;
	using AdvertisedSocketAddress = Neo4Net.Helpers.AdvertisedSocketAddress;
	using Neo4Net.Helpers.Collections;
	using Config = Neo4Net.Kernel.configuration.Config;
	using Log = Neo4Net.Logging.Log;
	using LogService = Neo4Net.Logging.@internal.LogService;
	using PkiUtils = Neo4Net.Ssl.PkiUtils;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.codehaus.jackson.map.DeserializationConfig.Feature.FAIL_ON_UNKNOWN_PROPERTIES;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.util.Preconditions.checkState;

	public class KubernetesResolver : RemoteMembersResolver
	{
		 private readonly KubernetesClient _kubernetesClient;
		 private readonly HttpClient _httpClient;
		 private readonly Log _log;

		 private KubernetesResolver( LogService logService, Config config )
		 {
			  this._log = logService.GetInternalLog( this.GetType() );

			  SslContextFactory sslContextFactory = CreateSslContextFactory( config );
			  this._httpClient = new HttpClient( sslContextFactory );

			  string token = Read( config.Get( CausalClusteringSettings.kubernetes_token ) );
			  string @namespace = Read( config.Get( CausalClusteringSettings.kubernetes_namespace ) );

			  this._kubernetesClient = new KubernetesClient( logService, _httpClient, token, @namespace, config, RetryingHostnameResolver.DefaultRetryStrategy( config, logService.InternalLogProvider ) );
		 }

		 public static RemoteMembersResolver Resolver( LogService logService, Config config )
		 {
			  return new KubernetesResolver( logService, config );
		 }

		 private SslContextFactory CreateSslContextFactory( Config config )
		 {
			  File caCert = config.Get( CausalClusteringSettings.kubernetes_ca_crt );
			  try
			  {
					  using ( SecurePassword password = new SecurePassword( 16, new SecureRandom() ), Stream caCertStream = Files.newInputStream(caCert.toPath(), StandardOpenOption.READ) )
					  {
						KeyStore keyStore = LoadKeyStore( password, caCertStream );
      
						SslContextFactory sslContextFactory = new SslContextFactory();
						sslContextFactory.TrustStore = keyStore;
						sslContextFactory.TrustStorePassword = new string( password.Password() );
      
						return sslContextFactory;
					  }
			  }
			  catch ( Exception e )
			  {
					throw new System.InvalidOperationException( "Unable to load CA certificate for Kubernetes", e );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private java.security.KeyStore loadKeyStore(SecurePassword password, java.io.InputStream caCertStream) throws java.security.cert.CertificateException, java.security.KeyStoreException, java.io.IOException, java.security.NoSuchAlgorithmException
		 private KeyStore LoadKeyStore( SecurePassword password, Stream caCertStream )
		 {
			  CertificateFactory certificateFactory = CertificateFactory.getInstance( PkiUtils.CERTIFICATE_TYPE );
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.Collection<? extends java.security.cert.Certificate> certificates = certificateFactory.generateCertificates(caCertStream);
			  ICollection<Certificate> certificates = certificateFactory.generateCertificates( caCertStream );
			  checkState( certificates.Count > 0, "Expected non empty Kubernetes CA certificates" );
			  KeyStore keyStore = KeyStore.getInstance( KeyStore.DefaultType );
			  keyStore.load( null, password.Password() );

			  int idx = 0;
			  foreach ( Certificate certificate in certificates )
			  {
					keyStore.setCertificateEntry( "ca" + idx++, certificate );
			  }
			  return keyStore;
		 }

		 private string Read( File file )
		 {
			  try
			  {
					Optional<string> line = Files.lines( file.toPath() ).findFirst();

					if ( line.Present )
					{
						 return line.get();
					}
					else
					{
						 throw new System.InvalidOperationException( string.Format( "Expected file at {0} to have at least 1 line", file ) );
					}
			  }
			  catch ( IOException e )
			  {
					throw new System.ArgumentException( "Unable to read file " + file, e );
			  }
		 }

		 public override ICollection<T> Resolve<T>( System.Func<AdvertisedSocketAddress, T> transform )
		 {
			  try
			  {
					_httpClient.start();
					return _kubernetesClient.resolve( null ).Select( transform ).ToList();
			  }
			  catch ( Exception e )
			  {
					throw new System.InvalidOperationException( "Unable to query Kubernetes API", e );
			  }
			  finally
			  {
					try
					{
						 _httpClient.stop();
					}
					catch ( Exception e )
					{
						 _log.warn( "Unable to shut down HTTP client", e );
					}
			  }
		 }

		 /// <summary>
		 /// Interface requires a parameter for resolve() and resolveOnce() that is unused here. This boils down to the interface of RetryStrategy, which has
		 /// an INPUT type parameter. Not worth duplicating RetryStrategy for this one class.
		 /// See <a href="https://kubernetes.io/docs/reference/generated/kubernetes-api/v1.11/#list-service-v1-core">List Service</a>
		 /// </summary>
		  internal class KubernetesClient : RetryingHostnameResolver
		  {
			  internal const string PATH = "/api/v1/namespaces/%s/services";
			  internal readonly Log Log;
			  internal readonly Log UserLog;
			  internal readonly HttpClient HttpClient;
			  internal readonly string Token;
			  internal readonly string Namespace;
			  internal readonly string LabelSelector;
			  internal readonly ObjectMapper ObjectMapper;
			  internal readonly string PortName;
			  internal readonly AdvertisedSocketAddress KubernetesAddress;

			  internal KubernetesClient( LogService logService, HttpClient httpClient, string token, string @namespace, Config config, MultiRetryStrategy<AdvertisedSocketAddress, ICollection<AdvertisedSocketAddress>> retryStrategy ) : base( config, retryStrategy )
			  {
					this.Log = logService.GetInternalLog( this.GetType() );
					this.UserLog = logService.GetUserLog( this.GetType() );
					this.Token = token;
					this.Namespace = @namespace;

					this.KubernetesAddress = config.Get( CausalClusteringSettings.kubernetes_address );
					this.LabelSelector = config.Get( CausalClusteringSettings.kubernetes_label_selector );
					this.PortName = config.Get( CausalClusteringSettings.kubernetes_service_port_name );

					this.HttpClient = httpClient;
					this.ObjectMapper = ( new ObjectMapper() ).configure(FAIL_ON_UNKNOWN_PROPERTIES, false);
			  }

			  protected internal override ICollection<AdvertisedSocketAddress> ResolveOnce( AdvertisedSocketAddress ignored )
			  {
					try
					{
						 ContentResponse response = HttpClient.newRequest( KubernetesAddress.Hostname, KubernetesAddress.Port ).method( HttpMethod.GET ).scheme( "https" ).path( string.format( PATH, Namespace ) ).param( "labelSelector", LabelSelector ).header( HttpHeader.AUTHORIZATION, "Bearer " + Token ).accept( MimeTypes.Type.APPLICATION_JSON.asString() ).send();

						 Log.info( "Received from k8s api \n" + response.ContentAsString );

						 KubernetesType serviceList = ObjectMapper.readValue( response.Content, typeof( KubernetesType ) );

						 ICollection<AdvertisedSocketAddress> addresses = serviceList.Handle( new Parser( PortName, Namespace ) );

						 UserLog.info( "Resolved %s from Kubernetes API at %s namespace %s labelSelector %s", addresses, KubernetesAddress, Namespace, LabelSelector );

						 if ( addresses.Count == 0 )
						 {
							  Log.error( "Resolved empty hosts from Kubernetes API at %s namespace %s labelSelector %s", KubernetesAddress, Namespace, LabelSelector );
						 }

						 return addresses;
					}
					catch ( IOException e )
					{
						 Log.error( "Failed to parse result from Kubernetes API", e );
						 return Collections.emptySet();
					}
					catch ( Exception e ) when ( e is InterruptedException || e is ExecutionException || e is TimeoutException )
					{
						 Log.error( string.Format( "Failed to resolve hosts from Kubernetes API at {0} namespace {1} labelSelector {2}", KubernetesAddress, Namespace, LabelSelector ), e );
						 return Collections.emptySet();
					}
			  }
		  }

		 private class Parser : KubernetesType.Visitor<ICollection<AdvertisedSocketAddress>>
		 {
			  internal readonly string PortName;
			  internal readonly string Namespace;

			  internal Parser( string portName, string @namespace )
			  {
					this.PortName = portName;
					this.Namespace = @namespace;
			  }

			  public override ICollection<AdvertisedSocketAddress> Visit( Status status )
			  {
					string message = string.Format( "Unable to contact Kubernetes API. Status: {0}", status );
					throw new System.InvalidOperationException( message );
			  }

			  public override ICollection<AdvertisedSocketAddress> Visit( ServiceList serviceList )
			  {
					Stream<Pair<string, ServiceList.Service.ServiceSpec.ServicePort>> serviceNamePortStream = serviceList.Items().Where(this.notDeleted).flatMap(this.extractServicePort);

					return serviceNamePortStream.map( serviceNamePort => new AdvertisedSocketAddress( string.Format( "{0}.{1}.svc.cluster.local", serviceNamePort.first(), Namespace ), serviceNamePort.other().port() ) ).collect(Collectors.toSet());
			  }

			  internal virtual bool NotDeleted( ServiceList.Service service )
			  {
					return string.ReferenceEquals( service.Metadata().deletionTimestamp(), null );
			  }

			  internal virtual Stream<Pair<string, ServiceList.Service.ServiceSpec.ServicePort>> ExtractServicePort( ServiceList.Service service )
			  {
					return service.Spec().ports().Where(port => PortName.Equals(port.name())).Select(port => Pair.of(service.Metadata().name(), port));
			  }
		 }
	}

}