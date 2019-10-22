using System;
using System.Collections.Generic;

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
namespace Neo4Net.Jmx.impl
{

	using Service = Neo4Net.Helpers.Service;

	[Obsolete]
	public class ManagementSupport
	{
		 public static ManagementSupport Load()
		 {
			  ManagementSupport support = new ManagementSupport();
			  foreach ( ManagementSupport candidate in Service.load( typeof( ManagementSupport ) ) )
			  {
					// Can we know that there aren't going to be multiple instances?
					support = candidate;
			  }
			  return support;
		 }

		 protected internal virtual MBeanServer MBeanServer
		 {
			 get
			 {
				  return PlatformMBeanServer;
			 }
		 }

		 /// <summary>
		 /// Create a proxy for the specified bean.
		 /// </summary>
		 /// @param <T> The type of the bean to create. </param>
		 /// <param name="kernel"> the kernel that the proxy should be created for. </param>
		 /// <param name="beanInterface"> the bean type to create the proxy for. </param>
		 /// <returns> a new proxy for the specified bean. </returns>
		 protected internal virtual T MakeProxy<T>( KernelBean kernel, ObjectName name, Type beanInterface )
		 {
				 beanInterface = typeof( T );
			  throw new System.NotSupportedException( "Cannot create management bean proxies." );
		 }

		 internal ICollection<T> GetProxiesFor<T>( Type beanInterface, KernelBean kernel )
		 {
				 beanInterface = typeof( T );
			  ICollection<T> result = new List<T>();
			  ObjectName query = CreateObjectNameQuery( kernel.InstanceId, beanInterface );
			  foreach ( ObjectName name in MBeanServer.queryNames( query, null ) )
			  {
					result.Add( MakeProxy( kernel, name, beanInterface ) );
			  }
			  return result;
		 }

		 protected internal virtual bool SupportsMxBeans()
		 {
			  return false;
		 }

		 public ObjectName CreateObjectName( string instanceId, Type beanInterface, params string[] extraNaming )
		 {
			  return createObjectName( instanceId, GetBeanName( beanInterface ), false, extraNaming );
		 }

		 private ObjectName CreateObjectNameQuery( string instanceId, Type beanInterface )
		 {
			  return CreateObjectName( instanceId, GetBeanName( beanInterface ), true );
		 }

		 public ObjectName CreateMBeanQuery( string instanceId )
		 {
			  return CreateObjectName( instanceId, "*", false );
		 }

		 protected internal virtual string GetBeanName( Type beanInterface )
		 {
			  return BeanName( beanInterface );
		 }

		 protected internal virtual ObjectName CreateObjectName( string instanceId, string beanName, bool query, params string[] extraNaming )
		 {
			  Dictionary<string, string> properties = new Dictionary<string, string>();
			  properties["instance"] = "kernel#" + instanceId;
			  properties["name"] = beanName;
			  for ( int i = 0; i < extraNaming.Length; i++ )
			  {
					properties["name" + i] = extraNaming[i];
			  }
			  try
			  {
					return new ObjectName( "org.Neo4Net", properties );
			  }
			  catch ( MalformedObjectNameException )
			  {
					return null;
			  }
		 }

		 internal static string BeanName( Type iface )
		 {
			  if ( iface.IsInterface )
			  {
					ManagementInterface management = iface.getAnnotation( typeof( ManagementInterface ) );
					if ( management != null )
					{
						 return management.name();
					}
			  }
			  throw new System.ArgumentException( iface + " is not a Neo4Net Management Been interface" );
		 }
	}

}