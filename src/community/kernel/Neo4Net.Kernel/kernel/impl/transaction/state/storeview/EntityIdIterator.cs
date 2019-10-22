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
namespace Neo4Net.Kernel.impl.transaction.state.storeview
{
	using PrimitiveLongResourceIterator = Neo4Net.Collections.PrimitiveLongResourceIterator;

	public interface IEntityIdIterator : PrimitiveLongResourceIterator
	{
		 /// <summary>
		 /// An <seealso cref="EntityIdIterator"/> is allowed to cache some ids ahead of it for performance reasons. Although during certain
		 /// points of execution there may be a need to bring this iterator fully up to date with concurrent changes.
		 /// Calling this method will invalidate any ids that have been cached ahead of it, e.g. read from store, so that
		 /// continued iteration after this call sees at least store updates from this point in time.
		 /// </summary>
		 void InvalidateCache();
	}

}