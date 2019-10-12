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
	using IndexEntryConflictException = Org.Neo4j.Kernel.Api.Exceptions.index.IndexEntryConflictException;
	using Value = Org.Neo4j.Values.Storable.Value;
	using ValueTuple = Org.Neo4j.Values.Storable.ValueTuple;

	internal class ThrowingConflictDetector<KEY, VALUE> : ConflictDetectingValueMerger<KEY, VALUE, Value[]> where KEY : NativeIndexKey<KEY> where VALUE : NativeIndexValue
	{
		 internal ThrowingConflictDetector( bool compareEntityIds ) : base( compareEntityIds )
		 {
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void doReportConflict(long existingNodeId, long addedNodeId, org.neo4j.values.storable.Value[] values) throws org.neo4j.kernel.api.exceptions.index.IndexEntryConflictException
		 internal override void DoReportConflict( long existingNodeId, long addedNodeId, Value[] values )
		 {
			  throw new IndexEntryConflictException( existingNodeId, addedNodeId, ValueTuple.of( values ) );
		 }
	}

}