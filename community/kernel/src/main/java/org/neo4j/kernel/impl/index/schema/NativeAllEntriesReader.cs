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
	using Org.Neo4j.Helpers.Collection;
	using Org.Neo4j.Helpers.Collection;
	using Org.Neo4j.Index.@internal.gbptree;
	using Org.Neo4j.Index.@internal.gbptree;
	using Org.Neo4j.Index.@internal.gbptree;

	public class NativeAllEntriesReader<KEY, VALUE> : BoundedIterable<long> where KEY : NativeIndexKey<KEY> where VALUE : NativeIndexValue
	{
		 private readonly GBPTree<KEY, VALUE> _tree;
		 private readonly Layout<KEY, VALUE> _layout;
		 private RawCursor<Hit<KEY, VALUE>, IOException> _seeker;

		 internal NativeAllEntriesReader( GBPTree<KEY, VALUE> tree, Layout<KEY, VALUE> layout )
		 {
			  this._tree = tree;
			  this._layout = layout;
		 }

		 public override IEnumerator<long> Iterator()
		 {
			  KEY from = _layout.newKey();
			  from.initialize( long.MinValue );
			  from.initValuesAsLowest();
			  KEY to = _layout.newKey();
			  to.initialize( long.MaxValue );
			  to.initValuesAsHighest();
			  try
			  {
					CloseSeeker();
					_seeker = _tree.seek( from, to );
					return new PrefetchingIteratorAnonymousInnerClass( this );
			  }
			  catch ( IOException e )
			  {
					throw new UncheckedIOException( e );
			  }
		 }

		 private class PrefetchingIteratorAnonymousInnerClass : PrefetchingIterator<long>
		 {
			 private readonly NativeAllEntriesReader<KEY, VALUE> _outerInstance;

			 public PrefetchingIteratorAnonymousInnerClass( NativeAllEntriesReader<KEY, VALUE> outerInstance )
			 {
				 this.outerInstance = outerInstance;
			 }

			 protected internal override long? fetchNextOrNull()
			 {
				  try
				  {
						return _outerInstance.seeker.next() ? _outerInstance.seeker.get().key().EntityId : null;
				  }
				  catch ( IOException e )
				  {
						throw new UncheckedIOException( e );
				  }
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void closeSeeker() throws java.io.IOException
		 private void CloseSeeker()
		 {
			  if ( _seeker != null )
			  {
					_seeker.close();
					_seeker = null;
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void close() throws Exception
		 public override void Close()
		 {
			  CloseSeeker();
		 }

		 public override long MaxCount()
		 {
			  return Org.Neo4j.Helpers.Collection.BoundedIterable_Fields.UNKNOWN_MAX_COUNT;
		 }
	}

}