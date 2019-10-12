using System;
using System.Collections.Generic;
using System.Threading;

/*
 * Copyright (c) 2002-2019 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
 *
 * This file is part of Neo4j.
 *
 * Neo4j is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */
namespace Org.Neo4j.Server.web
{
	using Request = org.eclipse.jetty.server.Request;
	using RequestLog = org.eclipse.jetty.server.RequestLog;
	using Server = org.eclipse.jetty.server.Server;
	using ServerConnector = org.eclipse.jetty.server.ServerConnector;
	using HandlerCollection = org.eclipse.jetty.server.handler.HandlerCollection;
	using HandlerList = org.eclipse.jetty.server.handler.HandlerList;
	using MovedContextHandler = org.eclipse.jetty.server.handler.MovedContextHandler;
	using RequestLogHandler = org.eclipse.jetty.server.handler.RequestLogHandler;
	using SessionHandler = org.eclipse.jetty.server.session.SessionHandler;
	using FilterHolder = org.eclipse.jetty.servlet.FilterHolder;
	using ServletContextHandler = org.eclipse.jetty.servlet.ServletContextHandler;
	using BlockingArrayQueue = org.eclipse.jetty.util.BlockingArrayQueue;
	using Resource = org.eclipse.jetty.util.resource.Resource;
	using QueuedThreadPool = org.eclipse.jetty.util.thread.QueuedThreadPool;
	using WebAppContext = org.eclipse.jetty.webapp.WebAppContext;


	using ListenSocketAddress = Org.Neo4j.Helpers.ListenSocketAddress;
	using PortBindException = Org.Neo4j.Helpers.PortBindException;
	using NetworkConnectionTracker = Org.Neo4j.Kernel.api.net.NetworkConnectionTracker;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using Log = Org.Neo4j.Logging.Log;
	using LogProvider = Org.Neo4j.Logging.LogProvider;
	using Org.Neo4j.Server.database;
	using Org.Neo4j.Server.plugins;
	using SslSocketConnectorFactory = Org.Neo4j.Server.security.ssl.SslSocketConnectorFactory;
	using SslPolicy = Org.Neo4j.Ssl.SslPolicy;


	/// <summary>
	/// This class handles the configuration and runtime management of a Jetty web server. The server is restartable.
	/// </summary>
	public class Jetty9WebServer : WebServer
	{
		 private const int JETTY_THREAD_POOL_IDLE_TIMEOUT = 60000;

		 public static readonly ListenSocketAddress DefaultAddress = new ListenSocketAddress( "0.0.0.0", 80 );

		 private bool _wadlEnabled;
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private java.util.Collection<org.neo4j.server.database.InjectableProvider<?>> defaultInjectables;
		 private ICollection<InjectableProvider<object>> _defaultInjectables;
		 private System.Action<Server> _jettyCreatedCallback;
		 private RequestLog _requestLog;

		 private Server _jetty;
		 private HandlerCollection _handlers;
		 private ListenSocketAddress _httpAddress = DefaultAddress;
		 private ListenSocketAddress _httpsAddress;

		 private ServerConnector _httpConnector;
		 private ServerConnector _httpsConnector;

		 private readonly Dictionary<string, string> _staticContent = new Dictionary<string, string>();
		 private readonly IDictionary<string, JaxRsServletHolderFactory> _jaxRSPackages = new Dictionary<string, JaxRsServletHolderFactory>();
		 private readonly IDictionary<string, JaxRsServletHolderFactory> _jaxRSClasses = new Dictionary<string, JaxRsServletHolderFactory>();
		 private readonly IList<FilterDefinition> _filters = new List<FilterDefinition>();

		 private int _jettyMaxThreads = 1;
		 private SslPolicy _sslPolicy;
		 private readonly SslSocketConnectorFactory _sslSocketFactory;
		 private readonly HttpConnectorFactory _connectorFactory;
		 private readonly Log _log;

		 public Jetty9WebServer( LogProvider logProvider, Config config, NetworkConnectionTracker connectionTracker )
		 {
			  this._log = logProvider.getLog( this.GetType() );
			  _sslSocketFactory = new SslSocketConnectorFactory( connectionTracker, config );
			  _connectorFactory = new HttpConnectorFactory( connectionTracker, config );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void start() throws Exception
		 public override void Start()
		 {
			  if ( _jetty == null )
			  {
					VerifyAddressConfiguration();

					JettyThreadCalculator jettyThreadCalculator = new JettyThreadCalculator( _jettyMaxThreads );
					_jetty = new Server( CreateQueuedThreadPool( jettyThreadCalculator ) );

					if ( _httpAddress != null )
					{
						 _httpConnector = _connectorFactory.createConnector( _jetty, _httpAddress, jettyThreadCalculator );
						 _jetty.addConnector( _httpConnector );
					}

					if ( _httpsAddress != null )
					{
						 if ( _sslPolicy == null )
						 {
							  throw new Exception( "HTTPS set to enabled, but no SSL policy provided" );
						 }
						 _httpsConnector = _sslSocketFactory.createConnector( _jetty, _sslPolicy, _httpsAddress, jettyThreadCalculator );
						 _jetty.addConnector( _httpsConnector );
					}

					if ( _jettyCreatedCallback != null )
					{
						 _jettyCreatedCallback.accept( _jetty );
					}
			  }

			  _handlers = new HandlerList();
			  _jetty.Handler = _handlers;
			  _handlers.addHandler( new MovedContextHandler() );

			  LoadAllMounts();

			  if ( _requestLog != null )
			  {
					LoadRequestLogging();
			  }

			  StartJetty();
		 }

		 private static QueuedThreadPool CreateQueuedThreadPool( JettyThreadCalculator jtc )
		 {
			  BlockingQueue<ThreadStart> queue = new BlockingArrayQueue<ThreadStart>( jtc.MinThreads, jtc.MinThreads, jtc.MaxCapacity );
			  QueuedThreadPool threadPool = new QueuedThreadPool( jtc.MaxThreads, jtc.MinThreads, JETTY_THREAD_POOL_IDLE_TIMEOUT, queue );
			  threadPool.ThreadPoolBudget = null; // mute warnings about Jetty thread pool size
			  return threadPool;
		 }

		 public override void Stop()
		 {
			  if ( _jetty != null )
			  {
					try
					{
						 _jetty.stop();
					}
					catch ( Exception e )
					{
						 throw new Exception( e );
					}
					try
					{
						 _jetty.join();
					}
					catch ( InterruptedException )
					{
						 _log.warn( "Interrupted while waiting for Jetty to stop" );
					}
					_jetty = null;
			  }
		 }

		 public virtual ListenSocketAddress HttpAddress
		 {
			 set
			 {
				  _httpAddress = value;
			 }
		 }

		 public virtual ListenSocketAddress HttpsAddress
		 {
			 set
			 {
				  _httpsAddress = value;
			 }
		 }

		 public virtual SslPolicy SslPolicy
		 {
			 set
			 {
				  _sslPolicy = value;
			 }
		 }

		 public virtual int MaxThreads
		 {
			 set
			 {
				  _jettyMaxThreads = value;
			 }
		 }

		 public override void AddJAXRSPackages<T1>( IList<string> packageNames, string mountPoint, ICollection<T1> injectables )
		 {
			  // We don't want absolute URIs at this point
			  mountPoint = EnsureRelativeUri( mountPoint );
			  mountPoint = TrimTrailingSlashToKeepJettyHappy( mountPoint );

			  JaxRsServletHolderFactory factory = _jaxRSPackages.computeIfAbsent( mountPoint, k => new JaxRsServletHolderFactory.Packages() );
			  factory.Add( packageNames, injectables );

			  _log.debug( "Adding JAXRS packages %s at [%s]", packageNames, mountPoint );
		 }

		 public override void AddJAXRSClasses<T1>( IList<string> classNames, string mountPoint, ICollection<T1> injectables )
		 {
			  // We don't want absolute URIs at this point
			  mountPoint = EnsureRelativeUri( mountPoint );
			  mountPoint = TrimTrailingSlashToKeepJettyHappy( mountPoint );

			  JaxRsServletHolderFactory factory = _jaxRSClasses.computeIfAbsent( mountPoint, k => new JaxRsServletHolderFactory.Classes() );
			  factory.Add( classNames, injectables );

			  _log.debug( "Adding JAXRS classes %s at [%s]", classNames, mountPoint );
		 }

		 public virtual bool WadlEnabled
		 {
			 set
			 {
				  this._wadlEnabled = value;
			 }
		 }

		 public virtual ICollection<T1> DefaultInjectables<T1>
		 {
			 set
			 {
				  this._defaultInjectables = value;
			 }
		 }

		 public virtual System.Action<Server> JettyCreatedCallback
		 {
			 set
			 {
				  this._jettyCreatedCallback = value;
			 }
		 }

		 public override void RemoveJAXRSPackages( IList<string> packageNames, string serverMountPoint )
		 {
			  JaxRsServletHolderFactory factory = _jaxRSPackages[serverMountPoint];
			  if ( factory != null )
			  {
					factory.Remove( packageNames );
			  }
		 }

		 public override void RemoveJAXRSClasses( IList<string> classNames, string serverMountPoint )
		 {
			  JaxRsServletHolderFactory factory = _jaxRSClasses[serverMountPoint];
			  if ( factory != null )
			  {
					factory.Remove( classNames );
			  }
		 }

		 public override void AddFilter( Filter filter, string pathSpec )
		 {
			  _filters.Add( new FilterDefinition( filter, pathSpec ) );
		 }

		 public override void RemoveFilter( Filter filter, string pathSpec )
		 {
			  _filters.removeIf( current => current.matches( filter, pathSpec ) );
		 }

		 public override void AddStaticContent( string contentLocation, string serverMountPoint )
		 {
			  _staticContent[serverMountPoint] = contentLocation;
		 }

		 public override void RemoveStaticContent( string contentLocation, string serverMountPoint )
		 {
			  _staticContent.Remove( serverMountPoint );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void invokeDirectly(String targetPath, javax.servlet.http.HttpServletRequest request, javax.servlet.http.HttpServletResponse response) throws java.io.IOException, javax.servlet.ServletException
		 public override void InvokeDirectly( string targetPath, HttpServletRequest request, HttpServletResponse response )
		 {
			  _jetty.handle( targetPath, ( Request ) request, request, response );
		 }

		 public virtual RequestLog RequestLog
		 {
			 set
			 {
				  this._requestLog = value;
			 }
		 }

		 public virtual Server Jetty
		 {
			 get
			 {
				  return _jetty;
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void startJetty() throws Exception
		 private void StartJetty()
		 {
			  try
			  {
					_jetty.start();
			  }
			  catch ( IOException e )
			  {
					throw new PortBindException( _httpAddress, _httpsAddress, e );
			  }
		 }

		 public virtual InetSocketAddress LocalHttpAddress
		 {
			 get
			 {
				  return GetAddress( "HTTP", _httpConnector );
			 }
		 }

		 public virtual InetSocketAddress LocalHttpsAddress
		 {
			 get
			 {
				  return GetAddress( "HTTPS", _httpsConnector );
			 }
		 }

		 private void LoadAllMounts()
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.SortedSet<String> mountpoints = new java.util.TreeSet<>(java.util.Comparator.reverseOrder());
			  SortedSet<string> mountpoints = new SortedSet<string>( System.Collections.IComparer.reverseOrder() );

			  mountpoints.addAll( _staticContent.Keys );
			  mountpoints.addAll( _jaxRSPackages.Keys );
			  mountpoints.addAll( _jaxRSClasses.Keys );

			  foreach ( string contentKey in mountpoints )
			  {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final boolean isStatic = staticContent.containsKey(contentKey);
					bool isStatic = _staticContent.ContainsKey( contentKey );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final boolean isJaxrsPackage = jaxRSPackages.containsKey(contentKey);
					bool isJaxrsPackage = _jaxRSPackages.ContainsKey( contentKey );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final boolean isJaxrsClass = jaxRSClasses.containsKey(contentKey);
					bool isJaxrsClass = _jaxRSClasses.ContainsKey( contentKey );

					if ( CountSet( isStatic, isJaxrsPackage, isJaxrsClass ) > 1 )
					{
						 throw new Exception( format( "content-key '%s' is mapped more than once", contentKey ) );
					}
					else if ( isStatic )
					{
						 LoadStaticContent( contentKey );
					}
					else if ( isJaxrsPackage )
					{
						 LoadJAXRSPackage( contentKey );
					}
					else if ( isJaxrsClass )
					{
						 LoadJAXRSClasses( contentKey );
					}
					else
					{
						 throw new Exception( format( "content-key '%s' is not mapped", contentKey ) );
					}
			  }
		 }

		 private int CountSet( params bool[] booleans )
		 {
			  int count = 0;
			  foreach ( bool @bool in booleans )
			  {
					if ( @bool )
					{
						 count++;
					}
			  }
			  return count;
		 }

		 private void LoadRequestLogging()
		 {
			  // This makes the request log handler decorate whatever other handlers are already set up
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.eclipse.jetty.server.handler.RequestLogHandler requestLogHandler = new HttpChannelOptionalRequestLogHandler();
			  RequestLogHandler requestLogHandler = new HttpChannelOptionalRequestLogHandler();
			  requestLogHandler.RequestLog = _requestLog;
			  requestLogHandler.Server = _jetty;
			  requestLogHandler.Handler = _jetty.Handler;
			  _jetty.Handler = requestLogHandler;
		 }

		 private string TrimTrailingSlashToKeepJettyHappy( string mountPoint )
		 {
			  if ( mountPoint.Equals( "/" ) )
			  {
					return mountPoint;
			  }

			  if ( mountPoint.EndsWith( "/", StringComparison.Ordinal ) )
			  {
					mountPoint = mountPoint.Substring( 0, mountPoint.Length - 1 );
			  }
			  return mountPoint;
		 }

		 private string EnsureRelativeUri( string mountPoint )
		 {
			  try
			  {
					URI result = new URI( mountPoint );
					if ( result.Absolute )
					{
						 return result.Path;
					}
					else
					{
						 return result.ToString();
					}
			  }
			  catch ( URISyntaxException )
			  {
					_log.debug( "Unable to translate [%s] to a relative URI in ensureRelativeUri(String mountPoint)", mountPoint );
					return mountPoint;
			  }
		 }

		 private void LoadStaticContent( string mountPoint )
		 {
			  string contentLocation = _staticContent[mountPoint];
			  try
			  {
					SessionHandler sessionHandler = new SessionHandler();
					sessionHandler.Server = Jetty;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.eclipse.jetty.webapp.WebAppContext staticContext = new org.eclipse.jetty.webapp.WebAppContext();
					WebAppContext staticContext = new WebAppContext();
					staticContext.Server = Jetty;
					staticContext.ContextPath = mountPoint;
					staticContext.SessionHandler = sessionHandler;
					staticContext.setInitParameter( "org.eclipse.jetty.servlet.Default.dirAllowed", "false" );
					URL resourceLoc = this.GetType().ClassLoader.getResource(contentLocation);
					if ( resourceLoc != null )
					{
						 URL url = resourceLoc.toURI().toURL();
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.eclipse.jetty.util.resource.Resource resource = org.eclipse.jetty.util.resource.Resource.newResource(url);
						 Resource resource = Resource.newResource( url );
						 staticContext.BaseResource = resource;

						 AddFiltersTo( staticContext );
						 staticContext.addFilter( new FilterHolder( new StaticContentFilter() ), "/*", EnumSet.of(DispatcherType.REQUEST, DispatcherType.FORWARD) );

						 _handlers.addHandler( staticContext );
					}
					else
					{
						 _log.warn( "No static content available for Neo4j Server at %s. management console may not be available.", AddressConfigurationDescription() );
					}
			  }
			  catch ( Exception e )
			  {
					_log.error( "Unknown error loading static content", e );
					Console.WriteLine( e.ToString() );
					Console.Write( e.StackTrace );
					throw new Exception( e );
			  }
		 }

		 private void LoadJAXRSPackage( string mountPoint )
		 {
			  LoadJAXRSResource( mountPoint, _jaxRSPackages[mountPoint] );
		 }

		 private void LoadJAXRSClasses( string mountPoint )
		 {
			  LoadJAXRSResource( mountPoint, _jaxRSClasses[mountPoint] );
		 }

		 private void LoadJAXRSResource( string mountPoint, JaxRsServletHolderFactory jaxRsServletHolderFactory )
		 {
			  SessionHandler sessionHandler = new SessionHandler();
			  sessionHandler.Server = Jetty;
			  _log.debug( "Mounting servlet at [%s]", mountPoint );
			  ServletContextHandler jerseyContext = new ServletContextHandler();
			  jerseyContext.Server = Jetty;
			  jerseyContext.ErrorHandler = new NeoJettyErrorHandler();
			  jerseyContext.ContextPath = mountPoint;
			  jerseyContext.SessionHandler = sessionHandler;
			  jerseyContext.addServlet( jaxRsServletHolderFactory.Create( _defaultInjectables, _wadlEnabled ), "/*" );
			  AddFiltersTo( jerseyContext );
			  _handlers.addHandler( jerseyContext );
		 }

		 private void AddFiltersTo( ServletContextHandler context )
		 {
			  foreach ( FilterDefinition filterDef in _filters )
			  {
					context.addFilter( new FilterHolder( filterDef.Filter ), filterDef.PathSpec, EnumSet.allOf( typeof( DispatcherType ) ) );
			  }
		 }

		 private static InetSocketAddress GetAddress( string name, ServerConnector connector )
		 {
			  if ( connector == null )
			  {
					throw new System.InvalidOperationException( name + " connector is not configured" );
			  }
			  return new InetSocketAddress( connector.Host, connector.LocalPort );
		 }

		 private void VerifyAddressConfiguration()
		 {
			  if ( _httpAddress == null && _httpsAddress == null )
			  {
					throw new System.InvalidOperationException( "Either HTTP or HTTPS address must be configured to run the server" );
			  }
		 }

		 private string AddressConfigurationDescription()
		 {
			  return Stream.of( _httpAddress, _httpsAddress ).filter( Objects.nonNull ).map( object.toString ).collect( joining( ", " ) );
		 }

		 private class FilterDefinition
		 {
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly Filter FilterConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly string PathSpecConflict;

			  internal FilterDefinition( Filter filter, string pathSpec )
			  {
					this.FilterConflict = filter;
					this.PathSpecConflict = pathSpec;
			  }

			  public virtual bool Matches( Filter filter, string pathSpec )
			  {
					return filter == this.FilterConflict && pathSpec.Equals( this.PathSpecConflict );
			  }

			  public virtual Filter Filter
			  {
				  get
				  {
						return FilterConflict;
				  }
			  }
			  internal virtual string PathSpec
			  {
				  get
				  {
						return PathSpecConflict;
				  }
			  }
		 }
	}

}