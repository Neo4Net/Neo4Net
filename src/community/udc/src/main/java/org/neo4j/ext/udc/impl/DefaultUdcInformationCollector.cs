using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

/*
 * Copyright © 2018-2020 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net.
 *
 * Neo4Net is free software: you can redistribute it and/or modify
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
namespace Neo4Net.Ext.Udc.impl
{

	using DependencyResolver = Neo4Net.GraphDb.DependencyResolver;
	using Neo4Net.GraphDb.config;
	using MapUtil = Neo4Net.Helpers.Collections.MapUtil;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using FileUtils = Neo4Net.Io.fs.FileUtils;
	using OsBeanUtil = Neo4Net.Io.os.OsBeanUtil;
	using NeoStoreDataSource = Neo4Net.Kernel.NeoStoreDataSource;
	using Config = Neo4Net.Kernel.configuration.Config;
	using IdGeneratorFactory = Neo4Net.Kernel.impl.store.id.IdGeneratorFactory;
	using IdType = Neo4Net.Kernel.impl.store.id.IdType;
	using DataSourceManager = Neo4Net.Kernel.impl.transaction.state.DataSourceManager;
	using UsageData = Neo4Net.Udc.UsageData;
	using UsageDataKeys = Neo4Net.Udc.UsageDataKeys;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.ext.udc.UdcConstants.CLUSTER_HASH;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.ext.udc.UdcConstants.DATABASE_MODE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.ext.udc.UdcConstants.DISTRIBUTION;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.ext.udc.UdcConstants.EDITION;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.ext.udc.UdcConstants.FEATURES;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.ext.udc.UdcConstants.HEAP_SIZE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.ext.udc.UdcConstants.ID;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.ext.udc.UdcConstants.LABEL_IDS_IN_USE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.ext.udc.UdcConstants.MAC;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.ext.udc.UdcConstants.NODE_IDS_IN_USE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.ext.udc.UdcConstants.NUM_PROCESSORS;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.ext.udc.UdcConstants.OS_PROPERTY_PREFIX;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.ext.udc.UdcConstants.PROPERTY_IDS_IN_USE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.ext.udc.UdcConstants.REGISTRATION;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.ext.udc.UdcConstants.RELATIONSHIP_IDS_IN_USE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.ext.udc.UdcConstants.REVISION;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.ext.udc.UdcConstants.SERVER_ID;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.ext.udc.UdcConstants.SOURCE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.ext.udc.UdcConstants.STORE_SIZE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.ext.udc.UdcConstants.TAGS;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.ext.udc.UdcConstants.TOTAL_MEMORY;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.ext.udc.UdcConstants.UDC_PROPERTY_PREFIX;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.ext.udc.UdcConstants.UNKNOWN_DIST;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.ext.udc.UdcConstants.USER_AGENTS;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.ext.udc.UdcConstants.VERSION;

	public class DefaultUdcInformationCollector : UdcInformationCollector
	{
		 private static readonly IDictionary<string, string> _jarNamesForTags = MapUtil.stringMap( "spring-", "spring", "(javax.ejb|ejb-jar)", "ejb", "(weblogic|glassfish|websphere|jboss)", "appserver", "openshift", "openshift", "cloudfoundry", "cloudfoundry", "(junit|testng)", "test", "jruby", "ruby", "clojure", "clojure", "jython", "python", "groovy", "groovy", "(tomcat|jetty)", "web", "spring-data-Neo4Net", "sdn" );

		 private readonly Config _config;
		 private readonly UsageData _usageData;

		 private string _storeId;

		 private NeoStoreDataSource _neoStoreDataSource;

		 internal DefaultUdcInformationCollector( Config config, DataSourceManager dataSourceManager, UsageData usageData )
		 {
			  this._config = config;
			  this._usageData = usageData;

			  if ( dataSourceManager != null )
			  {
					dataSourceManager.AddListener( new ListenerAnonymousInnerClass( this ) );
			  }
		 }

		 private class ListenerAnonymousInnerClass : DataSourceManager.Listener
		 {
			 private readonly DefaultUdcInformationCollector _outerInstance;

			 public ListenerAnonymousInnerClass( DefaultUdcInformationCollector outerInstance )
			 {
				 this.outerInstance = outerInstance;
			 }

			 public void registered( NeoStoreDataSource ds )
			 {
				  _outerInstance.storeId = ( ds.StoreId.RandomId ).ToString( "x" );

				  _outerInstance.neoStoreDataSource = ds;
			 }

			 public void unregistered( NeoStoreDataSource ds )
			 {
				  _outerInstance.storeId = null;

				  _outerInstance.neoStoreDataSource = null;
			 }
		 }

		 internal static string FilterVersionForUDC( string version )
		 {
			  if ( !version.Contains( "+" ) )
			  {
					return version;
			  }
			  return version.Substring( 0, version.IndexOf( '+' ) );
		 }

		 public virtual IDictionary<string, string> UdcParams
		 {
			 get
			 {
				  string classPath = ClassPath;
   
				  IDictionary<string, string> udcFields = new Dictionary<string, string>();
   
				  Add( udcFields, ID, _storeId );
				  Add( udcFields, VERSION, FilterVersionForUDC( _usageData.get( UsageDataKeys.version ) ) );
				  Add( udcFields, REVISION, FilterVersionForUDC( _usageData.get( UsageDataKeys.revision ) ) );
   
				  Add( udcFields, EDITION, _usageData.get( UsageDataKeys.edition ).name().ToLower() );
				  Add( udcFields, SOURCE, _config.get( UdcSettings.udc_source ) );
				  Add( udcFields, REGISTRATION, _config.get( UdcSettings.udc_registration_key ) );
				  Add( udcFields, DATABASE_MODE, _usageData.get( UsageDataKeys.operationalMode ).name() );
				  Add( udcFields, SERVER_ID, _usageData.get( UsageDataKeys.serverId ) );
				  Add( udcFields, USER_AGENTS, ToCommaString( _usageData.get( UsageDataKeys.clientNames ) ) );
   
				  Add( udcFields, TAGS, DetermineTags( _jarNamesForTags, classPath ) );
				  Add( udcFields, CLUSTER_HASH, DetermineClusterNameHash() );
   
				  Add( udcFields, MAC, DetermineMacAddress() );
				  Add( udcFields, DISTRIBUTION, DetermineOsDistribution() );
				  Add( udcFields, NUM_PROCESSORS, DetermineNumberOfProcessors() );
				  Add( udcFields, TOTAL_MEMORY, DetermineTotalMemory() );
				  Add( udcFields, HEAP_SIZE, DetermineHeapSize() );
   
				  Add( udcFields, NODE_IDS_IN_USE, DetermineNodesIdsInUse() );
				  Add( udcFields, RELATIONSHIP_IDS_IN_USE, DetermineRelationshipIdsInUse() );
				  Add( udcFields, LABEL_IDS_IN_USE, DetermineLabelIdsInUse() );
				  Add( udcFields, PROPERTY_IDS_IN_USE, DeterminePropertyIdsInUse() );
   
				  Add( udcFields, FEATURES, _usageData.get( UsageDataKeys.features ).asHex() );
   
				  AddStoreFileSizes( udcFields );
   
	//JAVA TO C# CONVERTER TODO TASK: There is no .NET Dictionary equivalent to the Java 'putAll' method:
				  udcFields.putAll( DetermineSystemProperties() );
				  return udcFields;
			 }
		 }

		 /// <summary>
		 /// Register store file sizes, sans transaction logs
		 /// </summary>
		 private void AddStoreFileSizes( IDictionary<string, string> udcFields )
		 {
			  if ( _neoStoreDataSource == null )
			  {
					return;
			  }
			  DependencyResolver dependencyResolver = _neoStoreDataSource.DependencyResolver;
			  FileSystemAbstraction fileSystem = dependencyResolver.ResolveDependency( typeof( FileSystemAbstraction ) );
			  long databaseSize = FileUtils.size( fileSystem, _neoStoreDataSource.DatabaseLayout.databaseDirectory() );
			  Add( udcFields, STORE_SIZE, databaseSize );
		 }

		 private static string DetermineOsDistribution()
		 {
			  if ( System.Properties.getProperty( "os.name", "" ).Equals( "Linux" ) )
			  {
					return SearchForPackageSystems();
			  }
			  else
			  {
					return UNKNOWN_DIST;
			  }
		 }

		 internal static string SearchForPackageSystems()
		 {
			  try
			  {
					if ( Directory.Exists( "/bin/rpm" ) || File.Exists( "/bin/rpm" ) )
					{
						 return "rpm";
					}
					if ( Directory.Exists( "/usr/bin/dpkg" ) || File.Exists( "/usr/bin/dpkg" ) )
					{
						 return "dpkg";
					}
			  }
			  catch ( Exception )
			  {
					// ignore
			  }
			  return UNKNOWN_DIST;
		 }

		 private int? DetermineClusterNameHash()
		 {
			  try
			  {
					Type settings = Type.GetType( "org.Neo4Net.cluster.ClusterSettings" );
					Setting setting = ( Setting ) settings.GetField( "cluster_name" ).get( null );
					object name = _config.get( setting );
					return name != null ? Math.Abs( name.GetHashCode() % int.MaxValue ) : null;
			  }
			  catch ( Exception )
			  {
					return null;
			  }
		 }

		 private static string DetermineTags( IDictionary<string, string> jarNamesForTags, string classPath )
		 {
			  StringBuilder result = new StringBuilder();
			  foreach ( KeyValuePair<string, string> entry in jarNamesForTags.SetOfKeyValuePairs() )
			  {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.regex.Pattern pattern = java.util.regex.Pattern.compile(entry.getKey());
					Pattern pattern = Pattern.compile( entry.Key );
					if ( pattern.matcher( classPath ).find() )
					{
						 result.Append( "," ).Append( entry.Value );
					}
			  }
			  if ( result.Length == 0 )
			  {
					return null;
			  }
			  return result.substring( 1 );
		 }

		 private static string ClassPath
		 {
			 get
			 {
				  RuntimeMXBean runtime = ManagementFactory.RuntimeMXBean;
				  return runtime.ClassPath;
			 }
		 }

		 private static string DetermineMacAddress()
		 {
			  string formattedMac = "0";
			  try
			  {
					InetAddress address = InetAddress.LocalHost;
					NetworkInterface ni = NetworkInterface.getByInetAddress( address );
					if ( ni != null )
					{
						 sbyte[] mac = ni.HardwareAddress;
						 if ( mac != null )
						 {
							  StringBuilder sb = new StringBuilder( mac.Length * 2 );
							  Formatter formatter = new Formatter( sb );
							  foreach ( sbyte b in mac )
							  {
									formatter.format( "%02x", b );
							  }
							  formattedMac = sb.ToString();
						 }
					}
			  }
			  catch ( Exception )
			  {
					//
			  }

			  return formattedMac;
		 }

		 private static int DetermineNumberOfProcessors()
		 {
			  return Runtime.Runtime.availableProcessors();
		 }

		 private static long DetermineTotalMemory()
		 {
			  return OsBeanUtil.TotalPhysicalMemory;
		 }

		 private static long DetermineHeapSize()
		 {
			  return ManagementFactory.MemoryMXBean.HeapMemoryUsage.Used;
		 }

		 private long DetermineNodesIdsInUse()
		 {
			  return GetNumberOfIdsInUse( IdType.NODE );
		 }

		 private long DetermineLabelIdsInUse()
		 {
			  return GetNumberOfIdsInUse( IdType.LABEL_TOKEN );
		 }

		 private long DeterminePropertyIdsInUse()
		 {
			  return GetNumberOfIdsInUse( IdType.PROPERTY );
		 }

		 private long DetermineRelationshipIdsInUse()
		 {
			  return GetNumberOfIdsInUse( IdType.RELATIONSHIP );
		 }

		 private long GetNumberOfIdsInUse( IdType type )
		 {
			  return _neoStoreDataSource.DependencyResolver.resolveDependency( typeof( IdGeneratorFactory ) ).get( type ).NumberOfIdsInUse;
		 }

		 private static string ToCommaString( object values )
		 {
			  StringBuilder result = new StringBuilder();
			  if ( values is System.Collections.IEnumerable )
			  {
					foreach ( object agent in ( System.Collections.IEnumerable ) values )
					{
						 if ( agent == null )
						 {
							  continue;
						 }
						 if ( result.Length > 0 )
						 {
							  result.Append( "," );
						 }
						 result.Append( agent );
					}
			  }
			  else
			  {
					result.Append( values );
			  }
			  return result.ToString();
		 }

		 private static void Add( IDictionary<string, string> udcFields, string name, object value )
		 {
			  if ( value == null )
			  {
					return;
			  }
			  string str = value.ToString().Trim();
			  if ( str.Length == 0 )
			  {
					return;
			  }
			  udcFields[name] = str;
		 }

		 private static string RemoveUdcPrefix( string propertyName )
		 {
			  if ( propertyName.StartsWith( UDC_PROPERTY_PREFIX, StringComparison.Ordinal ) )
			  {
					return propertyName.Substring( UDC_PROPERTY_PREFIX.length() + 1 );
			  }
			  return propertyName;
		 }

		 private static string SanitizeUdcProperty( string propertyValue )
		 {
			  return propertyValue.Replace( ' ', '_' );
		 }

		 private static IDictionary<string, string> DetermineSystemProperties()
		 {
			  IDictionary<string, string> relevantSysProps = new Dictionary<string, string>();
			  Properties sysProps = System.Properties;
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.Iterator<?> sysPropsNames = sysProps.propertyNames();
			  IEnumerator<object> sysPropsNames = sysProps.propertyNames();
			  while ( sysPropsNames.MoveNext() )
			  {
					string sysPropName = ( string ) sysPropsNames.Current;
					if ( sysPropName.StartsWith( UDC_PROPERTY_PREFIX, StringComparison.Ordinal ) || sysPropName.StartsWith( OS_PROPERTY_PREFIX, StringComparison.Ordinal ) )
					{
						 string propertyValue = sysProps.getProperty( sysPropName );
						 relevantSysProps[RemoveUdcPrefix( sysPropName )] = SanitizeUdcProperty( propertyValue );
					}
			  }
			  return relevantSysProps;
		 }

		 public virtual string StoreId
		 {
			 get
			 {
				  return _storeId;
			 }
		 }

	}

}