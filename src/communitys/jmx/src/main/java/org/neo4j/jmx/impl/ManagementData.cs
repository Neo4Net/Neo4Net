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
namespace Neo4Net.Jmx.impl
{

	using KernelData = Neo4Net.Kernel.@internal.KernelData;

	[Obsolete]
	public sealed class ManagementData
	{
		 private readonly KernelData _kernel;
		 private readonly ManagementSupport _support;
		 internal readonly ManagementBeanProvider Provider;

		 public ManagementData( ManagementBeanProvider provider, KernelData kernel, ManagementSupport support )
		 {
			  this.Provider = provider;
			  this._kernel = kernel;
			  this._support = support;
		 }

		 public KernelData KernelData
		 {
			 get
			 {
				  return _kernel;
			 }
		 }

		 public T ResolveDependency<T>( Type clazz )
		 {
				 clazz = typeof( T );
			  return _kernel.DataSourceManager.DataSource.DependencyResolver.resolveDependency( clazz );
		 }

		 internal ObjectName GetObjectName( params string[] extraNaming )
		 {
			  ObjectName name = _support.createObjectName( _kernel.instanceId(), Provider.beanInterface, extraNaming );
			  if ( name == null )
			  {
					throw new System.ArgumentException( Provider.beanInterface + " is not a Neo4j Management Bean interface" );
			  }
			  return name;
		 }

		 internal void Validate( Type implClass )
		 {
			  if ( !Provider.beanInterface.IsAssignableFrom( implClass ) )
			  {
					throw new System.InvalidOperationException( implClass + " does not implement " + Provider.beanInterface );
			  }
		 }
	}

}