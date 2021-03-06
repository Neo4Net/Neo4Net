﻿using System.Collections.Generic;

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
	using IndexProgressor = Org.Neo4j.Storageengine.Api.schema.IndexProgressor;
	using Value = Org.Neo4j.Values.Storable.Value;

	internal abstract class NativeIndexProgressor<KEY, VALUE> : IndexProgressor where KEY : NativeIndexKey<KEY> where VALUE : NativeIndexValue
	{
		public abstract bool Next();
		 internal readonly RawCursor<Hit<KEY, VALUE>, IOException> Seeker;
		 internal readonly Org.Neo4j.Storageengine.Api.schema.IndexProgressor_NodeValueClient Client;
		 private readonly ICollection<RawCursor<Hit<KEY, VALUE>, IOException>> _toRemoveFromOnClose;
		 private bool _closed;

		 internal NativeIndexProgressor( RawCursor<Hit<KEY, VALUE>, IOException> seeker, Org.Neo4j.Storageengine.Api.schema.IndexProgressor_NodeValueClient client, ICollection<RawCursor<Hit<KEY, VALUE>, IOException>> toRemoveFromOnClose )
		 {
			  this.Seeker = seeker;
			  this.Client = client;
			  this._toRemoveFromOnClose = toRemoveFromOnClose;
		 }

		 public override void Close()
		 {
			  if ( !_closed )
			  {
					_closed = true;
					try
					{
						 Seeker.close();
						 _toRemoveFromOnClose.remove( Seeker );
					}
					catch ( IOException e )
					{
						 throw new UncheckedIOException( e );
					}
			  }
		 }

		 internal virtual Value[] ExtractValues( KEY key )
		 {
			  return Client.needsValues() ? key.asValues() : null;
		 }
	}

}