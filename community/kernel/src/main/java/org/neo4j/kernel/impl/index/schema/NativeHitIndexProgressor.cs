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
namespace Org.Neo4j.Kernel.Impl.Index.Schema
{

	using Org.Neo4j.Cursor;
	using Org.Neo4j.Index.@internal.gbptree;
	using Value = Org.Neo4j.Values.Storable.Value;

	public class NativeHitIndexProgressor<KEY, VALUE> : NativeIndexProgressor<KEY, VALUE> where KEY : NativeIndexKey<KEY> where VALUE : NativeIndexValue
	{
		 internal NativeHitIndexProgressor( RawCursor<Hit<KEY, VALUE>, IOException> seeker, Org.Neo4j.Storageengine.Api.schema.IndexProgressor_NodeValueClient client, ICollection<RawCursor<Hit<KEY, VALUE>, IOException>> toRemoveFromOnClose ) : base( seeker, client, toRemoveFromOnClose )
		 {
		 }

		 public override bool Next()
		 {
			  try
			  {
					while ( seeker.next() )
					{
						 KEY key = seeker.get().key();
						 Value[] values = extractValues( key );
						 if ( AcceptValue( values ) && client.acceptNode( key.EntityId, values ) )
						 {
							  return true;
						 }
					}
					return false;
			  }
			  catch ( IOException e )
			  {
					throw new UncheckedIOException( e );
			  }
		 }

		 protected internal virtual bool AcceptValue( Value[] values )
		 {
			  return true;
		 }
	}

}