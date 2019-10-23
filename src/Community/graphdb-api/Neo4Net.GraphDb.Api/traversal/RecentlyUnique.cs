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
namespace Neo4Net.GraphDb.Traversal
{
	using Neo4Net.Helpers.Collections;

	/// <summary>
	/// Filters on items with a recency within limits of <seealso cref="DEFAULT_RECENT_SIZE"/>.
	/// </summary>
	internal class RecentlyUnique : AbstractUniquenessFilter
	{
		 private static readonly object _placeHolder = new object();
		 private const int DEFAULT_RECENT_SIZE = 10000;

		 private readonly LruCache<long, object> _recentlyVisited;

		 internal RecentlyUnique( PrimitiveTypeFetcher type, object parameter ) : base( type )
		 {
			  parameter = parameter != null ? parameter : DEFAULT_RECENT_SIZE;
			  _recentlyVisited = new LruCache<long, object>( "Recently visited", ( ( Number ) parameter ).intValue() );
		 }

		 public override bool Check( ITraversalBranch branch )
		 {
			  long id = Type.getId( branch );
			  bool add = _recentlyVisited.get( id ) == null;
			  if ( add )
			  {
					_recentlyVisited.put( id, _placeHolder );
			  }
			  return add;
		 }

		 public override bool CheckFull( IPath path )
		 {
			  // See GloballyUnique for comments.
			  return true;
		 }
	}

}