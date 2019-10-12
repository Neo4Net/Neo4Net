using System;
using System.Collections.Generic;

/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
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
namespace Neo4Net.management
{

	using Kernel = Neo4Net.Jmx.Kernel;
	using Primitives = Neo4Net.Jmx.Primitives;
	using StoreFile = Neo4Net.Jmx.StoreFile;
	using ConfigurationBean = Neo4Net.Jmx.impl.ConfigurationBean;
	using KernelProxy = Neo4Net.management.impl.KernelProxy;

	public sealed class Neo4jManager : KernelProxy, Kernel
	{
		 public static Neo4jManager Get()
		 {
			  return get( PlatformMBeanServer );
		 }

		 public static Neo4jManager Get( string kernelIdentifier )
		 {
			  return get( PlatformMBeanServer, kernelIdentifier );
		 }

		 public static Neo4jManager Get( JMXServiceURL url )
		 {
			  return Get( Connect( url, null, null ) );
		 }

		 public static Neo4jManager Get( JMXServiceURL url, string kernelIdentifier )
		 {
			  return Get( Connect( url, null, null ), kernelIdentifier );
		 }

		 public static Neo4jManager Get( JMXServiceURL url, string username, string password )
		 {
			  return Get( Connect( url, username, password ) );
		 }

		 public static Neo4jManager Get( JMXServiceURL url, string username, string password, string kernelIdentifier )
		 {
			  return Get( Connect( url, username, password ), kernelIdentifier );
		 }

		 private static MBeanServerConnection Connect( JMXServiceURL url, string username, string password )
		 {
			  IDictionary<string, object> environment = new Dictionary<string, object>();
			  if ( !string.ReferenceEquals( username, null ) && !string.ReferenceEquals( password, null ) )
			  {
					environment[JMXConnector.CREDENTIALS] = new string[]{ username, password };
			  }
			  else if ( !string.ReferenceEquals( username, null ) || !string.ReferenceEquals( password, null ) )
			  {
					throw new System.ArgumentException( "User name and password must either both be specified, or both be null." );
			  }
			  try
			  {
					try
					{
						 return JMXConnectorFactory.connect( url, environment ).MBeanServerConnection;
					}
					catch ( SecurityException )
					{
						 environment[RMIConnectorServer.RMI_CLIENT_SOCKET_FACTORY_ATTRIBUTE] = new SslRMIClientSocketFactory();
						 return JMXConnectorFactory.connect( url, environment ).MBeanServerConnection;
					}
			  }
			  catch ( IOException e )
			  {
					throw new System.InvalidOperationException( "Connection failed.", e );
			  }
		 }

		 public static Neo4jManager Get( MBeanServerConnection server )
		 {
			  try
			  {
					return get( server, server.queryNames( CreateObjectName( "*", typeof( Kernel ) ), null ) );
			  }
			  catch ( IOException e )
			  {
					throw new System.InvalidOperationException( "Connection failed.", e );
			  }
		 }

		 public static Neo4jManager Get( MBeanServerConnection server, string kernelIdentifier )
		 {
			  try
			  {
					return get( server, server.queryNames( CreateObjectName( kernelIdentifier, typeof( Kernel ) ), null ) );
			  }
			  catch ( IOException e )
			  {
					throw new System.InvalidOperationException( "Connection failed.", e );
			  }
		 }

		 public static Neo4jManager[] GetAll( MBeanServerConnection server )
		 {
			  try
			  {
					ISet<ObjectName> kernels = server.queryNames( CreateObjectName( "*", typeof( Kernel ) ), null );
					Neo4jManager[] managers = new Neo4jManager[kernels.Count];
					IEnumerator<ObjectName> it = kernels.GetEnumerator();
					for ( int i = 0; i < managers.Length; i++ )
					{
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
						 managers[i] = new Neo4jManager( server, it.next() );
					}
					return managers;
			  }
			  catch ( IOException e )
			  {
					throw new System.InvalidOperationException( "Connection failed.", e );
			  }
		 }

		 private static Neo4jManager Get( MBeanServerConnection server, ICollection<ObjectName> kernels )
		 {
			  if ( kernels.Count == 0 )
			  {
					throw new NoSuchElementException( "No matching Neo4j Graph Database running on server" );
			  }
			  else if ( kernels.Count == 1 )
			  {
					return new Neo4jManager( server, kernels.GetEnumerator().next() );
			  }
			  else
			  {
					throw new NoSuchElementException( "Too many matching Neo4j Graph Databases running on server" );
			  }
		 }

		 private readonly ObjectName _config;
		 private readonly Kernel _proxy;

		 public Neo4jManager( Kernel kernel ) : this( GetServer( kernel ), GetName( kernel ) )
		 {
		 }

		 private static MBeanServerConnection GetServer( Kernel kernel )
		 {
			  if ( kernel is Proxy )
			  {
					InvocationHandler handler = Proxy.getInvocationHandler( kernel );
					if ( handler is MBeanServerInvocationHandler )
					{
						 return ( ( MBeanServerInvocationHandler ) handler ).MBeanServerConnection;
					}
			  }
			  else if ( kernel is Neo4jManager )
			  {
					return ( ( Neo4jManager ) kernel ).Server;
			  }
			  throw new System.NotSupportedException( "Cannot get server for kernel: " + kernel );
		 }

		 private static ObjectName GetName( Kernel kernel )
		 {
			  if ( kernel is Proxy )
			  {
					InvocationHandler handler = Proxy.getInvocationHandler( kernel );
					if ( handler is MBeanServerInvocationHandler )
					{
						 return ( ( MBeanServerInvocationHandler ) handler ).ObjectName;
					}
			  }
			  else if ( kernel is Neo4jManager )
			  {
					return ( ( Neo4jManager ) kernel ).Kernel;
			  }
			  throw new System.NotSupportedException( "Cannot get name for kernel: " + kernel );
		 }

		 private Neo4jManager( MBeanServerConnection server, ObjectName kernel ) : base( server, kernel )
		 {
			  this._config = CreateObjectName( ConfigurationBean.CONFIGURATION_MBEAN_NAME );
			  this._proxy = GetBean( typeof( Kernel ) );
		 }

		 public LockManager LockManagerBean
		 {
			 get
			 {
				  return GetBean( typeof( LockManager ) );
			 }
		 }

		 public IndexSamplingManager IndexSamplingManagerBean
		 {
			 get
			 {
				  return GetBean( typeof( IndexSamplingManager ) );
			 }
		 }

		 public MemoryMapping MemoryMappingBean
		 {
			 get
			 {
				  return GetBean( typeof( MemoryMapping ) );
			 }
		 }

		 public Primitives PrimitivesBean
		 {
			 get
			 {
				  return GetBean( typeof( Primitives ) );
			 }
		 }

		 public StoreFile StoreFileBean
		 {
			 get
			 {
				  return GetBean( typeof( StoreFile ) );
			 }
		 }

		 public TransactionManager TransactionManagerBean
		 {
			 get
			 {
				  return GetBean( typeof( TransactionManager ) );
			 }
		 }

		 public PageCache PageCacheBean
		 {
			 get
			 {
				  return GetBean( typeof( PageCache ) );
			 }
		 }

		 ///  @deprecated high availability database/edition is deprecated in favour of causal clustering. It will be removed in next major release. 
		 [Obsolete("high availability database/edition is deprecated in favour of causal clustering. It will be removed in next major release.")]
		 public HighAvailability HighAvailabilityBean
		 {
			 get
			 {
				  return GetBean( typeof( HighAvailability ) );
			 }
		 }

		 ///  @deprecated high availability database/edition is deprecated in favour of causal clustering. It will be removed in next major release. 
		 [Obsolete("high availability database/edition is deprecated in favour of causal clustering. It will be removed in next major release.")]
		 public BranchedStore BranchedStoreBean
		 {
			 get
			 {
				  return GetBean( typeof( BranchedStore ) );
			 }
		 }

		 public object GetConfigurationParameter( string key )
		 {
			  try
			  {
					return Server.getAttribute( _config, key );
			  }
			  catch ( AttributeNotFoundException )
			  {
					return null;
			  }
			  catch ( Exception e )
			  {
					throw new System.InvalidOperationException( "Could not access the configuration bean", e );
			  }
		 }

		 public IDictionary<string, object> Configuration
		 {
			 get
			 {
	//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
	//ORIGINAL LINE: final String[] keys;
				  string[] keys;
	//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
	//ORIGINAL LINE: final javax.management.AttributeList attributes;
				  AttributeList attributes;
				  try
				  {
						MBeanAttributeInfo[] keyInfo = Server.getMBeanInfo( _config ).Attributes;
						keys = new string[keyInfo.Length];
						for ( int i = 0; i < keys.Length; i++ )
						{
							 keys[i] = keyInfo[i].Name;
						}
						attributes = Server.getAttributes( _config, keys );
				  }
				  catch ( Exception e )
				  {
						throw new System.InvalidOperationException( "Could not access the configuration bean", e );
				  }
				  IDictionary<string, object> configuration = new Dictionary<string, object>();
				  for ( int i = 0; i < keys.Length; i++ )
				  {
						configuration[keys[i]] = attributes.get( i );
   
				  }
				  return configuration;
			 }
		 }

		 public override IList<object> AllBeans()
		 {
			  IList<object> beans = base.AllBeans();
			  Kernel kernel = null;
			  foreach ( object bean in beans )
			  {
					if ( bean is Kernel )
					{
						 kernel = ( Kernel ) bean;
					}
			  }
			  if ( kernel != null )
			  {
					beans.Remove( kernel );
			  }
			  return beans;
		 }

		 public DateTime KernelStartTime
		 {
			 get
			 {
				  return _proxy.KernelStartTime;
			 }
		 }

		 public string KernelVersion
		 {
			 get
			 {
				  return _proxy.KernelVersion;
			 }
		 }

		 public ObjectName MBeanQuery
		 {
			 get
			 {
				  return _proxy.MBeanQuery;
			 }
		 }

		 public DateTime StoreCreationDate
		 {
			 get
			 {
				  return _proxy.StoreCreationDate;
			 }
		 }

		 public string DatabaseName
		 {
			 get
			 {
				  return _proxy.DatabaseName;
			 }
		 }

		 public string StoreId
		 {
			 get
			 {
				  return _proxy.StoreId;
			 }
		 }

		 public long StoreLogVersion
		 {
			 get
			 {
				  return _proxy.StoreLogVersion;
			 }
		 }

		 public bool ReadOnly
		 {
			 get
			 {
				  return _proxy.ReadOnly;
			 }
		 }
	}

}