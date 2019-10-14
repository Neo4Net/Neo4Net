using System;

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
namespace Neo4Net.Jmx
{

	using GraphDatabaseService = Neo4Net.Graphdb.GraphDatabaseService;
	using JmxKernelExtension = Neo4Net.Jmx.impl.JmxKernelExtension;
	using GraphDatabaseAPI = Neo4Net.Kernel.Internal.GraphDatabaseAPI;

	[Obsolete]
	public class JmxUtils
	{
		 private static readonly MBeanServer _mbeanServer = ManagementFactory.PlatformMBeanServer;

		 public static ObjectName GetObjectName( GraphDatabaseService db, string name )
		 {
			  if ( !( db is GraphDatabaseAPI ) )
			  {
					throw new System.ArgumentException( "Can only resolve object names for embedded Neo4j database " + "instances, eg. instances created by GraphDatabaseFactory or HighlyAvailableGraphDatabaseFactory." );
			  }
			  ObjectName neoQuery = ( ( GraphDatabaseAPI )db ).DependencyResolver.resolveDependency( typeof( JmxKernelExtension ) ).getSingleManagementBean( typeof( Kernel ) ).MBeanQuery;

			  string instance = neoQuery.getKeyProperty( "instance" );
			  string domain = neoQuery.Domain;
			  try
			  {
					return new ObjectName( format( "%s:instance=%s,name=%s", domain, instance, name ) );
			  }
			  catch ( MalformedObjectNameException e )
			  {
					throw new Exception( e );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") public static <T> T getAttribute(javax.management.ObjectName objectName, String attribute)
		 public static T GetAttribute<T>( ObjectName objectName, string attribute )
		 {
			  try
			  {
					return ( T ) _mbeanServer.getAttribute( objectName, attribute );
			  }
			  catch ( Exception e )
			  {
					throw new Exception( e );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") public static <T> T invoke(javax.management.ObjectName objectName, String attribute, Object[] params, String[] signature)
		 public static T Invoke<T>( ObjectName objectName, string attribute, object[] @params, string[] signature )
		 {
			  try
			  {
					return ( T ) _mbeanServer.invoke( objectName, attribute, @params, signature );
			  }
			  catch ( Exception e )
			  {
					throw new Exception( e );
			  }
		 }
	}

}