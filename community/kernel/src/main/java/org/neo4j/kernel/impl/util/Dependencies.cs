using System;
using System.Collections.Generic;

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
namespace Org.Neo4j.Kernel.impl.util
{
	using RichIterable = org.eclipse.collections.api.RichIterable;
	using MutableList = org.eclipse.collections.api.list.MutableList;
	using MutableListMultimap = org.eclipse.collections.api.multimap.list.MutableListMultimap;
	using Multimaps = org.eclipse.collections.impl.factory.Multimaps;


	using DependencyResolver = Org.Neo4j.Graphdb.DependencyResolver;
	using Iterables = Org.Neo4j.Helpers.Collection.Iterables;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") public class Dependencies extends org.neo4j.graphdb.DependencyResolver_Adapter implements DependencySatisfier
	public class Dependencies : Org.Neo4j.Graphdb.DependencyResolver_Adapter, DependencySatisfier
	{
		 private readonly DependencyResolver _parent;
		 private readonly MutableListMultimap<Type, object> _typeDependencies = Multimaps.mutable.list.empty();

		 public Dependencies()
		 {
			  _parent = null;
		 }

		 public Dependencies( DependencyResolver parent )
		 {
			  Objects.requireNonNull( parent );
			  this._parent = parent;
		 }

		 public override T ResolveDependency<T>( Type type, SelectionStrategy selector )
		 {
				 type = typeof( T );
			  RichIterable options = _typeDependencies.get( type );
			  if ( options.notEmpty() )
			  {
					return selector.select( type, ( IEnumerable<T> ) options );
			  }

			  // Try parent
			  if ( _parent != null )
			  {
					return _parent.resolveDependency( type, selector );
			  }

			  // Out of options
			  throw new UnsatisfiedDependencyException( type );
		 }

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: public <T> Iterable<? extends T> resolveTypeDependencies(Class<T> type)
		 public override IEnumerable<T> ResolveTypeDependencies<T>( Type type )
		 {
				 type = typeof( T );
			  MutableList<T> options = ( MutableList<T> ) _typeDependencies.get( type );
			  if ( _parent != null )
			  {
					return Iterables.concat( options, _parent.resolveTypeDependencies( type ) );
			  }
			  return options;
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public <T> System.Func<T> provideDependency(final Class<T> type, final SelectionStrategy selector)
		 public override System.Func<T> ProvideDependency<T>( Type type, SelectionStrategy selector )
		 {
				 type = typeof( T );
			  return () => ResolveDependency(type, selector);
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public <T> System.Func<T> provideDependency(final Class<T> type)
		 public override System.Func<T> ProvideDependency<T>( Type type )
		 {
				 type = typeof( T );
			  return () => ResolveDependency(type);
		 }

		 public override T SatisfyDependency<T>( T dependency )
		 {
			  // File this object under all its possible types
			  Type type = dependency.GetType();
			  do
			  {
					_typeDependencies.put( type, dependency );

					// Add as all interfaces
					Type[] interfaces = type.GetInterfaces();
					AddInterfaces( interfaces, dependency );

					type = type.BaseType;
			  } while ( type != null );

			  return dependency;
		 }

		 public virtual void SatisfyDependencies( params object[] dependencies )
		 {
			  foreach ( object dependency in dependencies )
			  {
					SatisfyDependency( dependency );
			  }
		 }

		 private void AddInterfaces<T>( Type[] interfaces, T dependency )
		 {
			  foreach ( Type type in interfaces )
			  {
					_typeDependencies.put( type, dependency );
					AddInterfaces( type.GetInterfaces(), dependency );
			  }
		 }
	}

}