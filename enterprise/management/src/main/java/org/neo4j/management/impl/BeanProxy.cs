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
namespace Org.Neo4j.management.impl
{

	using Exceptions = Org.Neo4j.Helpers.Exceptions;

	public class BeanProxy
	{
		 private static readonly BeanProxy _instance = new BeanProxy();

		 public static T Load<T>( MBeanServerConnection mbs, Type beanInterface, ObjectName name )
		 {
				 beanInterface = typeof( T );
			  return _instance.makeProxy( mbs, beanInterface, name );
		 }

		 internal static ICollection<T> LoadAll<T>( MBeanServerConnection mbs, Type beanInterface, ObjectName query )
		 {
				 beanInterface = typeof( T );
			  ICollection<T> beans = new LinkedList<T>();
			  try
			  {
					foreach ( ObjectName name in mbs.queryNames( query, null ) )
					{
						 beans.Add( _instance.makeProxy( mbs, beanInterface, name ) );
					}
			  }
			  catch ( IOException )
			  {
					// fall through and return the empty collection...
			  }
			  return beans;
		 }

		 private readonly System.Reflection.MethodInfo _newMXBeanProxy;

		 private BeanProxy()
		 {
			  try
			  {
					Type jmx = Type.GetType( "javax.management.JMX" );
					this._newMXBeanProxy = Jmx.GetMethod( "newMXBeanProxy", typeof( MBeanServerConnection ), typeof( ObjectName ), typeof( Type ) );
			  }
			  catch ( Exception e ) when ( e is ClassNotFoundException || e is NoSuchMethodException )
			  {
					throw new Exception( e );
			  }
		 }

		 private T MakeProxy<T>( MBeanServerConnection mbs, Type beanInterface, ObjectName name )
		 {
				 beanInterface = typeof( T );
			  try
			  {
					return beanInterface.cast( _newMXBeanProxy.invoke( null, mbs, name, beanInterface ) );
			  }
			  catch ( InvocationTargetException exception )
			  {
					Exceptions.throwIfUnchecked( exception.TargetException );
					throw new Exception( exception.TargetException );
			  }
			  catch ( Exception exception )
			  {
					throw new System.NotSupportedException( "Creating Management Bean proxies requires Java 1.6", exception );
			  }
		 }
	}

}