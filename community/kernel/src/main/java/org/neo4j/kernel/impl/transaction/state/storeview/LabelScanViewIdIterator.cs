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
namespace Org.Neo4j.Kernel.impl.transaction.state.storeview
{
	using PrimitiveLongResourceIterator = Org.Neo4j.Collection.PrimitiveLongResourceIterator;
	using StorageEntityScanCursor = Org.Neo4j.Storageengine.Api.StorageEntityScanCursor;
	using LabelScanReader = Org.Neo4j.Storageengine.Api.schema.LabelScanReader;

	/// <summary>
	/// Node id iterator used during index population when we go over node ids indexed in label scan store.
	/// </summary>
	internal class LabelScanViewIdIterator<CURSOR> : EntityIdIterator where CURSOR : Org.Neo4j.Storageengine.Api.StorageEntityScanCursor
	{
		 private readonly int[] _labelIds;
		 private readonly LabelScanReader _labelScanReader;
		 private readonly CURSOR _entityCursor;

		 private PrimitiveLongResourceIterator _idIterator;
		 private long _lastReturnedId = -1;

		 internal LabelScanViewIdIterator( LabelScanReader labelScanReader, int[] labelIds, CURSOR entityCursor )
		 {
			  this._labelScanReader = labelScanReader;
			  this._entityCursor = entityCursor;
			  this._idIterator = labelScanReader.NodesWithAnyOfLabels( labelIds );
			  this._labelIds = labelIds;
		 }

		 public override void Close()
		 {
			  _labelScanReader.close();
		 }

		 public override bool HasNext()
		 {
			  return _idIterator.hasNext();
		 }

		 public override long Next()
		 {
			  long next = _idIterator.next();
			  _entityCursor.single( next );
			  _entityCursor.next();
			  _lastReturnedId = next;
			  return next;
		 }

		 public override void InvalidateCache()
		 {
			  this._idIterator.close();
			  this._idIterator = _labelScanReader.nodesWithAnyOfLabels( _lastReturnedId, _labelIds );
		 }
	}

}