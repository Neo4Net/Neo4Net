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
namespace Neo4Net.management.impl
{

	using ManagementInterface = Neo4Net.Jmx.ManagementInterface;

	/// <summary>
	/// Does not have any public methods - since the public interface of
	/// <seealso cref="org.neo4j.management.Neo4jManager"/> should be defined completely in
	/// that class.
	/// </summary>
	public abstract class KernelProxy
	{
		 internal const string KERNEL_BEAN_TYPE = "org.neo4j.jmx.Kernel";
		 protected internal const string KERNEL_BEAN_NAME = "Kernel";
		 internal const string MBEAN_QUERY = "MBeanQuery";
		 protected internal readonly MBeanServerConnection Server;
		 protected internal readonly ObjectName Kernel;

		 protected internal KernelProxy( MBeanServerConnection server, ObjectName kernel )
		 {
			  string className = null;
			  try
			  {
					className = server.getMBeanInfo( kernel ).ClassName;
			  }
			  catch ( Exception )
			  {
					// fall through
			  }
			  if ( !KERNEL_BEAN_TYPE.Equals( className ) )
			  {
					throw new System.ArgumentException( "The specified ObjectName does not represent a Neo4j Kernel bean in the specified MBean server." );
			  }
			  this.Server = server;
			  this.Kernel = kernel;
		 }

		 protected internal virtual IList<object> AllBeans()
		 {
			  IList<object> beans = new List<object>();
			  IEnumerable<ObjectInstance> mbeans;
			  try
			  {
					mbeans = Server.queryMBeans( MbeanQuery(), null );
			  }
			  catch ( IOException )
			  {
					return beans;
			  }
			  foreach ( ObjectInstance instance in mbeans )
			  {
					string className = instance.ClassName;
					Type beanType = null;
					try
					{
						 if ( !string.ReferenceEquals( className, null ) )
						 {
							  beanType = Type.GetType( className );
						 }
					}
					catch ( Exception ignored ) when ( ignored is Exception || ignored is LinkageError )
					{
						 // fall through
					}
					if ( beanType != null )
					{
						 try
						 {
							  beans.Add( BeanProxy.Load( Server, beanType, instance.ObjectName ) );
						 }
						 catch ( Exception )
						 {
							  // fall through
						 }
					}
			  }
			  return beans;
		 }

		 private ObjectName AssertExists( ObjectName name )
		 {
			  try
			  {
					if ( !Server.queryNames( name, null ).Empty )
					{
						 return name;
					}
			  }
			  catch ( IOException )
			  {
					// fall through
			  }
			  throw new NoSuchElementException( "No MBeans matching " + name );
		 }

		 protected internal virtual T GetBean<T>( Type beanInterface )
		 {
				 beanInterface = typeof( T );
			  return BeanProxy.Load( Server, beanInterface, CreateObjectName( beanInterface ) );
		 }

		 protected internal virtual ICollection<T> GetBeans<T>( Type beanInterface )
		 {
				 beanInterface = typeof( T );
			  return BeanProxy.LoadAll( Server, beanInterface, CreateObjectNameQuery( beanInterface ) );
		 }

		 private ObjectName CreateObjectNameQuery( Type beanInterface )
		 {
			  return CreateObjectNameQuery( MbeanQuery(), beanInterface );
		 }

		 private ObjectName CreateObjectName( Type beanInterface )
		 {
			  return AssertExists( CreateObjectName( MbeanQuery(), beanInterface ) );
		 }

		 protected internal virtual ObjectName CreateObjectName( string beanName )
		 {
			  return AssertExists( CreateObjectName( MbeanQuery(), beanName, false ) );
		 }

		 protected internal virtual ObjectName MbeanQuery()
		 {
			  try
			  {
					return ( ObjectName ) Server.getAttribute( Kernel, MBEAN_QUERY );
			  }
			  catch ( Exception cause )
			  {
					throw new System.InvalidOperationException( "Could not get MBean query.", cause );
			  }
		 }

		 protected internal static ObjectName CreateObjectName( string kernelIdentifier, Type beanInterface )
		 {
			  return CreateObjectName( kernelIdentifier, BeanName( beanInterface ) );
		 }

		 protected internal static ObjectName CreateObjectName( string kernelIdentifier, string beanName, params string[] extraNaming )
		 {
			  Dictionary<string, string> properties = new Dictionary<string, string>();
			  properties["instance"] = "kernel#" + kernelIdentifier;
			  return CreateObjectName( "org.neo4j", properties, beanName, false, extraNaming );
		 }

		 internal static ObjectName CreateObjectNameQuery( string kernelIdentifier, string beanName, params string[] extraNaming )
		 {
			  Dictionary<string, string> properties = new Dictionary<string, string>();
			  properties["instance"] = "kernel#" + kernelIdentifier;
			  return CreateObjectName( "org.neo4j", properties, beanName, true, extraNaming );
		 }

		 internal static ObjectName CreateObjectName( ObjectName query, Type beanInterface )
		 {
			  return CreateObjectName( query, BeanName( beanInterface ), false );
		 }

		 internal static ObjectName CreateObjectNameQuery( ObjectName query, Type beanInterface )
		 {
			  return CreateObjectName( query, BeanName( beanInterface ), true );
		 }

		 private static ObjectName CreateObjectName( ObjectName query, string beanName, bool isQuery )
		 {
			  Dictionary<string, string> properties = new Dictionary<string, string>( query.KeyPropertyList );
			  return CreateObjectName( query.Domain, properties, beanName, isQuery );
		 }

		 internal static string BeanName( Type beanInterface )
		 {
			  if ( beanInterface.IsInterface )
			  {
					ManagementInterface management = beanInterface.getAnnotation( typeof( ManagementInterface ) );
					if ( management != null )
					{
						 return management.name();
					}
			  }
			  throw new System.ArgumentException( beanInterface + " is not a Neo4j Management Been interface" );
		 }

		 private static ObjectName CreateObjectName( string domain, Dictionary<string, string> properties, string beanName, bool query, params string[] extraNaming )
		 {
			  properties["name"] = beanName;
			  for ( int i = 0; i < extraNaming.Length; i++ )
			  {
					properties["name" + i] = extraNaming[i];
			  }
			  ObjectName result;
			  try
			  {
					result = new ObjectName( domain, properties );
					if ( query )
					{
						 result = ObjectName.getInstance( result.ToString() + ",*" );
					}
			  }
			  catch ( MalformedObjectNameException )
			  {
					return null;
			  }
			  return result;
		 }
	}

}