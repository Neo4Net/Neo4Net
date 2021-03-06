﻿using System;

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
namespace Org.Neo4j.Jmx.impl
{

	using KernelData = Org.Neo4j.Kernel.@internal.KernelData;

	[Obsolete]
	public abstract class Neo4jMBean : StandardMBean
	{
		 internal readonly ObjectName ObjectName;

		 protected internal Neo4jMBean( ManagementData management, bool isMXBean, params string[] extraNaming ) : base( management.Provider.beanInterface, isMXBean )
		 {
			  management.Validate( this.GetType() );
			  this.ObjectName = management.GetObjectName( extraNaming );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected Neo4jMBean(ManagementData management, String... extraNaming) throws javax.management.NotCompliantMBeanException
		 protected internal Neo4jMBean( ManagementData management, params string[] extraNaming ) : base( management.Provider.beanInterface )
		 {
			  management.Validate( this.GetType() );
			  this.ObjectName = management.GetObjectName( extraNaming );
		 }

		 /// <summary>
		 /// Constructor for <seealso cref="KernelBean"/> </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: Neo4jMBean(Class<org.neo4j.jmx.Kernel> beanInterface, org.neo4j.kernel.internal.KernelData kernel, ManagementSupport support) throws javax.management.NotCompliantMBeanException
		 internal Neo4jMBean( Type beanInterface, KernelData kernel, ManagementSupport support )
		 {
				 beanInterface = typeof( Kernel );
			  base( beanInterface );
			  this.ObjectName = support.CreateObjectName( kernel.InstanceId(), beanInterface );
		 }

		 /// <summary>
		 /// Constructor for <seealso cref="ConfigurationBean"/> </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: Neo4jMBean(String beanName, org.neo4j.kernel.internal.KernelData kernel, ManagementSupport support) throws javax.management.NotCompliantMBeanException
		 internal Neo4jMBean( string beanName, KernelData kernel, ManagementSupport support ) : base( typeof( DynamicMBean ) )
		 {
			  this.ObjectName = support.CreateObjectName( kernel.InstanceId(), beanName, false );
		 }

		 protected internal override string GetClassName( MBeanInfo info )
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final Class iface = this.getMBeanInterface();
			  Type iface = this.MBeanInterface;
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
			  return iface == null ? base.GetClassName( info ) : iface.FullName;
		 }

		 protected internal override string GetDescription( MBeanInfo info )
		 {
			  Description description = DescribeClass();
			  if ( description != null )
			  {
					return description.value();
			  }
			  return base.GetDescription( info );
		 }

		 protected internal override string GetDescription( MBeanAttributeInfo info )
		 {
			  Description description = DescribeMethod( info, "get", "is" );
			  if ( description != null )
			  {
					return description.value();
			  }
			  return base.GetDescription( info );
		 }

		 protected internal override string GetDescription( MBeanOperationInfo info )
		 {
			  Description description = DescribeMethod( info );
			  if ( description != null )
			  {
					return description.value();
			  }
			  return base.GetDescription( info );
		 }

		 protected internal override int GetImpact( MBeanOperationInfo info )
		 {
			  Description description = DescribeMethod( info );
			  if ( description != null )
			  {
					return description.impact();
			  }
			  return base.GetImpact( info );
		 }

		 private Description DescribeClass()
		 {
			  Description description = this.GetType().getAnnotation(typeof(Description));
			  if ( description == null )
			  {
					foreach ( Type iface in this.GetType().GetInterfaces() )
					{
						 description = iface.getAnnotation( typeof( Description ) );
						 if ( description != null )
						 {
							  break;
						 }
					}
			  }
			  return description;
		 }

		 private Description DescribeMethod( MBeanFeatureInfo info, params string[] prefixes )
		 {
			  Description description = DescribeMethod( this.GetType(), info.Name, prefixes );
			  if ( description == null )
			  {
					foreach ( Type iface in this.GetType().GetInterfaces() )
					{
						 description = DescribeMethod( iface, info.Name, prefixes );
						 if ( description != null )
						 {
							  break;
						 }
					}
			  }
			  return description;
		 }

		 private static Description DescribeMethod( Type type, string methodName, string[] prefixes )
		 {
			  if ( prefixes == null || prefixes.Length == 0 )
			  {
					try
					{
						 return type.GetMethod( methodName ).getAnnotation( typeof( Description ) );
					}
					catch ( Exception )
					{
						 return null;
					}
			  }
			  else
			  {
					foreach ( string prefix in prefixes )
					{
						 try
						 {
							  return type.GetMethod( prefix + methodName ).getAnnotation( typeof( Description ) );
						 }
						 catch ( Exception )
						 {
							  // continue to next
						 }
					}
					return null;
			  }
		 }
	}

}