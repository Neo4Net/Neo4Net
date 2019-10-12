using System.Diagnostics;

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
namespace Org.Neo4j.Kernel.Impl.Api
{

	using ExecutingQuery = Org.Neo4j.Kernel.api.query.ExecutingQuery;

	internal abstract class ExecutingQueryList
	{
		 internal abstract Stream<ExecutingQuery> Queries();

		 internal abstract ExecutingQueryList Push( ExecutingQuery newExecutingQuery );

		 internal ExecutingQueryList Remove( ExecutingQuery executingQuery )
		 {
			  return Remove( null, executingQuery );
		 }

		 internal abstract ExecutingQueryList Remove( ExecutingQuery parent, ExecutingQuery target );

		 internal abstract T top<T>( System.Func<ExecutingQuery, T> accessor );

		 internal abstract void WaitsFor( ExecutingQuery query );

		 internal static readonly ExecutingQueryList EMPTY = new ExecutingQueryListAnonymousInnerClass();

		 private class ExecutingQueryListAnonymousInnerClass : ExecutingQueryList
		 {
			 internal override Stream<ExecutingQuery> queries()
			 {
				  return Stream.empty();
			 }

			 internal override ExecutingQueryList push( ExecutingQuery newExecutingQuery )
			 {
				  return new Entry( newExecutingQuery, this );
			 }

			 internal override ExecutingQueryList remove( ExecutingQuery parent, ExecutingQuery target )
			 {
				  return this;
			 }

			 internal override T top<T>( System.Func<ExecutingQuery, T> accessor )
			 {
				  return null;
			 }

			 internal override void waitsFor( ExecutingQuery query )
			 {
			 }
		 }

		 private class Entry : ExecutingQueryList
		 {
			  internal readonly ExecutingQuery Query;
			  internal readonly ExecutingQueryList Next;

			  internal Entry( ExecutingQuery query, ExecutingQueryList next )
			  {
					this.Query = query;
					this.Next = next;
			  }

			  internal override Stream<ExecutingQuery> Queries()
			  {
					Stream.Builder<ExecutingQuery> builder = Stream.builder();
					ExecutingQueryList entry = this;
					while ( entry != EMPTY )
					{
						 Entry current = ( Entry ) entry;
						 builder.accept( current.Query );
						 entry = current.Next;
					}
					return builder.build();
			  }

			  internal override ExecutingQueryList Push( ExecutingQuery newExecutingQuery )
			  {
					Debug.Assert( newExecutingQuery.InternalQueryId() > Query.internalQueryId() );
					WaitsFor( newExecutingQuery );
					return new Entry( newExecutingQuery, this );
			  }

			  internal override ExecutingQueryList Remove( ExecutingQuery parent, ExecutingQuery target )
			  {
					if ( target.Equals( Query ) )
					{
						 Next.waitsFor( parent );
						 return Next;
					}
					else
					{
						 ExecutingQueryList removed = Next.remove( parent, target );
						 if ( removed == Next )
						 {
							  return this;
						 }
						 else
						 {
							  return new Entry( Query, removed );
						 }
					}
			  }

			  internal override T Top<T>( System.Func<ExecutingQuery, T> accessor )
			  {
					return accessor( Query );
			  }

			  internal override void WaitsFor( ExecutingQuery child )
			  {
					this.Query.waitsForQuery( child );
			  }
		 }
	}

}