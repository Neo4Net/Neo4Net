using System.Collections.Generic;
using System.Text;

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
namespace Neo4Net.Kernel.impl.store
{

	using Neo4Net.Helpers.Collection;
	using SchemaDescriptor = Neo4Net.@internal.Kernel.Api.schema.SchemaDescriptor;

	public class MultipleUnderlyingStorageExceptions : UnderlyingStorageException
	{
		 public readonly ISet<Pair<SchemaDescriptor, UnderlyingStorageException>> Exceptions;

		 public MultipleUnderlyingStorageExceptions( ISet<Pair<SchemaDescriptor, UnderlyingStorageException>> exceptions ) : base( BuildMessage( exceptions ) )
		 {
			  this.Exceptions = Collections.unmodifiableSet( exceptions );

			  foreach ( Pair<SchemaDescriptor, UnderlyingStorageException> exception in exceptions )
			  {
					this.addSuppressed( exception.Other() );
			  }
		 }

		 private static string BuildMessage( ISet<Pair<SchemaDescriptor, UnderlyingStorageException>> exceptions )
		 {
			  StringBuilder builder = new StringBuilder();
			  builder.Append( "Errors when closing (flushing) index updaters:" );

			  foreach ( Pair<SchemaDescriptor, UnderlyingStorageException> pair in exceptions )
			  {
					builder.Append( format( " (%s) %s", pair.First().ToString(), pair.Other().Message ) );
			  }

			  return builder.ToString();
		 }

	}

}