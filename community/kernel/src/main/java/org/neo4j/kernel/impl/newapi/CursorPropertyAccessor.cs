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
namespace Org.Neo4j.Kernel.Impl.Newapi
{
	using NodeCursor = Org.Neo4j.@internal.Kernel.Api.NodeCursor;
	using PropertyCursor = Org.Neo4j.@internal.Kernel.Api.PropertyCursor;
	using Read = Org.Neo4j.@internal.Kernel.Api.Read;
	using EntityNotFoundException = Org.Neo4j.@internal.Kernel.Api.exceptions.EntityNotFoundException;
	using IOUtils = Org.Neo4j.Io.IOUtils;
	using EntityType = Org.Neo4j.Storageengine.Api.EntityType;
	using NodePropertyAccessor = Org.Neo4j.Storageengine.Api.NodePropertyAccessor;
	using Value = Org.Neo4j.Values.Storable.Value;
	using Values = Org.Neo4j.Values.Storable.Values;

	/// <summary>
	/// Generic single-threaded <seealso cref="NodePropertyAccessor"/> given a <seealso cref="NodeCursor"/> and <seealso cref="PropertyCursor"/>.
	/// </summary>
	internal class CursorPropertyAccessor : NodePropertyAccessor, AutoCloseable
	{
		 private readonly NodeCursor _nodeCursor;
		 private readonly PropertyCursor _propertyCursor;
		 private readonly Read _read;

		 internal CursorPropertyAccessor( NodeCursor nodeCursor, PropertyCursor propertyCursor, Read read )
		 {
			  this._nodeCursor = nodeCursor;
			  this._propertyCursor = propertyCursor;
			  this._read = read;
		 }

		 public override void Close()
		 {
			  IOUtils.closeAllSilently( _propertyCursor, _nodeCursor );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.neo4j.values.storable.Value getNodePropertyValue(long nodeId, int propertyKeyId) throws org.neo4j.internal.kernel.api.exceptions.EntityNotFoundException
		 public override Value GetNodePropertyValue( long nodeId, int propertyKeyId )
		 {
			  _read.singleNode( nodeId, _nodeCursor );
			  if ( !_nodeCursor.next() )
			  {
					throw new EntityNotFoundException( EntityType.NODE, nodeId );
			  }

			  _nodeCursor.properties( _propertyCursor );
			  while ( _propertyCursor.next() )
			  {
					if ( _propertyCursor.propertyKey() == propertyKeyId )
					{
						 return _propertyCursor.propertyValue();
					}
			  }
			  return Values.NO_VALUE;
		 }
	}

}