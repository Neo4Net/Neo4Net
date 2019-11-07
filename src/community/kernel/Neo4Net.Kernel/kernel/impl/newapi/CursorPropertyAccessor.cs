﻿/*
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
namespace Neo4Net.Kernel.Impl.Newapi
{
	using NodeCursor = Neo4Net.Kernel.Api.Internal.NodeCursor;
	using PropertyCursor = Neo4Net.Kernel.Api.Internal.PropertyCursor;
	using Read = Neo4Net.Kernel.Api.Internal.Read;
	using IEntityNotFoundException = Neo4Net.Kernel.Api.Internal.Exceptions.EntityNotFoundException;
	using IOUtils = Neo4Net.Io.IOUtils;
	using EntityType = Neo4Net.Kernel.Api.StorageEngine.EntityType;
	using NodePropertyAccessor = Neo4Net.Kernel.Api.StorageEngine.NodePropertyAccessor;
	using Value = Neo4Net.Values.Storable.Value;
	using Values = Neo4Net.Values.Storable.Values;

	/// <summary>
	/// Generic single-threaded <seealso cref="NodePropertyAccessor"/> given a <seealso cref="NodeCursor"/> and <seealso cref="PropertyCursor"/>.
	/// </summary>
	internal class CursorPropertyAccessor : NodePropertyAccessor, IDisposable
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
//ORIGINAL LINE: public Neo4Net.values.storable.Value getNodePropertyValue(long nodeId, int propertyKeyId) throws Neo4Net.Kernel.Api.Internal.Exceptions.EntityNotFoundException
		 public override Value GetNodePropertyValue( long nodeId, int propertyKeyId )
		 {
			  _read.singleNode( nodeId, _nodeCursor );
			  if ( !_nodeCursor.next() )
			  {
					throw new IEntityNotFoundException( EntityType.NODE, nodeId );
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