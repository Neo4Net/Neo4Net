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
	using MutableIntObjectMap = org.eclipse.collections.api.map.primitive.MutableIntObjectMap;
	using MutableLongSet = org.eclipse.collections.api.set.primitive.MutableLongSet;
	using IntObjectHashMap = org.eclipse.collections.impl.map.mutable.primitive.IntObjectHashMap;
	using LongHashSet = org.eclipse.collections.impl.set.mutable.primitive.LongHashSet;

	internal class LevelUnique : AbstractUniquenessFilter
	{
		 private readonly MutableIntObjectMap<MutableLongSet> _idsPerLevel = new IntObjectHashMap<MutableLongSet>();

		 internal LevelUnique( PrimitiveTypeFetcher type ) : base( type )
		 {
		 }

		 public override bool Check( ITraversalBranch branch )
		 {
			  int level = branch.Length;
			  MutableLongSet levelIds = _idsPerLevel.get( level );
			  if ( levelIds == null )
			  {
					levelIds = new LongHashSet();
					_idsPerLevel.put( level, levelIds );
			  }
			  return levelIds.add( Type.getId( branch ) );
		 }

		 public override bool CheckFull( IPath path )
		 {
			  throw new System.NotSupportedException( "Not implemented yet" );
		 }
	}

}