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
namespace Neo4Net.Server.database
{
	using HttpContext = com.sun.jersey.api.core.HttpContext;
	using ComponentContext = com.sun.jersey.core.spi.component.ComponentContext;
	using ComponentScope = com.sun.jersey.core.spi.component.ComponentScope;
	using AbstractHttpContextInjectable = com.sun.jersey.server.impl.inject.AbstractHttpContextInjectable;
	using Injectable = com.sun.jersey.spi.inject.Injectable;


	public abstract class InjectableProvider<E> : AbstractHttpContextInjectable<E>, com.sun.jersey.spi.inject.InjectableProvider<Context, Type<E>>
	{
		 public readonly Type<E> T;

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public static <E> InjectableProvider<? extends E> providerForSingleton(final E component, final Class<E> componentClass)
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
		 public static InjectableProvider<E> ProviderForSingleton<E>( E component, Type componentClass )
		 {
				 componentClass = typeof( E );
			  return new InjectableProviderAnonymousInnerClass( componentClass, component );
		 }

		 private class InjectableProviderAnonymousInnerClass : InjectableProvider<E>
		 {
			 private E _component;

			 public InjectableProviderAnonymousInnerClass( Type componentClass, E component ) : base( componentClass )
			 {
				 this._component = component;
			 }

			 public override E getValue( HttpContext httpContext )
			 {
				  return _component;
			 }
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public static <E> InjectableProvider<? extends E> providerFromSupplier(final System.Func<E> supplier, final Class<E> componentClass)
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
		 public static InjectableProvider<E> ProviderFromSupplier<E>( System.Func<E> supplier, Type componentClass )
		 {
				 componentClass = typeof( E );
			  return new InjectableProviderAnonymousInnerClass2( componentClass, supplier );
		 }

		 private class InjectableProviderAnonymousInnerClass2 : InjectableProvider<E>
		 {
			 private System.Func<E> _supplier;

			 public InjectableProviderAnonymousInnerClass2( Type componentClass, System.Func<E> supplier ) : base( componentClass )
			 {
				 this._supplier = supplier;
			 }

			 public override E getValue( HttpContext httpContext )
			 {
				  return _supplier();
			 }
		 }

		 public InjectableProvider( Type t )
		 {
				 t = typeof( E );
			  this.T = t;
		 }

		 public override Injectable<E> GetInjectable( ComponentContext ic, Context a, Type c )
		 {
				 c = typeof( E );
			  if ( c.Equals( T ) )
			  {
					return Injectable;
			  }

			  return null;
		 }

		 public virtual Injectable<E> GetInjectable()
		 {
			  return this;
		 }

		 public override ComponentScope Scope
		 {
			 get
			 {
				  return ComponentScope.PerRequest;
			 }
		 }
	}

}