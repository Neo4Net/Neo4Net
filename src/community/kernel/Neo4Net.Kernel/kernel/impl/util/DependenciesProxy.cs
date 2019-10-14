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
namespace Neo4Net.Kernel.impl.util
{

	using DependencyResolver = Neo4Net.Graphdb.DependencyResolver;

	/// <summary>
	/// Used to create dynamic proxies that implement dependency interfaces. Each method should have no arguments
	/// and return the type of the dependency desired. It will be mapped to a lookup in the provided <seealso cref="DependencyResolver"/>.
	/// Methods may also use a <seealso cref="Supplier"/> type for deferred lookups.
	/// </summary>
	public class DependenciesProxy
	{
		 private DependenciesProxy()
		 {
			  throw new AssertionError(); // no instances
		 }

		 /// <summary>
		 /// Create a dynamic proxy that implements the given interface and backs invocation with lookups into the given
		 /// dependency resolver.
		 /// </summary>
		 /// <param name="dependencyResolver"> original resolver to proxy </param>
		 /// <param name="dependenciesInterface"> interface to proxy </param>
		 /// @param <T> type of the interface </param>
		 /// <returns> a proxied <seealso cref="DependencyResolver"/> that will lookup dependencies in {@code dependencyResolver} based
		 /// on method names in the provided {@code dependenciesInterface} </returns>
		 public static T Dependencies<T>( DependencyResolver dependencyResolver, Type dependenciesInterface )
		 {
				 dependenciesInterface = typeof( T );
			  return dependenciesInterface.cast( Proxy.newProxyInstance( dependenciesInterface.ClassLoader, new Type[]{ dependenciesInterface }, new ProxyHandler( dependencyResolver ) ) );
		 }

		 private class ProxyHandler : InvocationHandler
		 {
			  internal DependencyResolver DependencyResolver;

			  internal ProxyHandler( DependencyResolver dependencyResolver )
			  {
					this.DependencyResolver = dependencyResolver;
			  }

			  public override object Invoke( object proxy, System.Reflection.MethodInfo method, object[] args )
			  {
					try
					{
						 if ( method.ReturnType.Equals( typeof( System.Func ) ) )
						 {
							  return DependencyResolver.provideDependency( ( Type )( ( ParameterizedType ) method.GenericReturnType ).ActualTypeArguments[0] );
						 }
						 else
						 {
							  return DependencyResolver.resolveDependency( method.ReturnType );
						 }
					}
					catch ( System.ArgumentException e )
					{
						 throw new UnsatisfiedDependencyException( e );
					}
			  }
		 }
	}

}