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
namespace Neo4Net.Kernel.Impl.Api.index
{
	using MutableIntSet = org.eclipse.collections.api.set.primitive.MutableIntSet;

	using NodePropertyAccessor = Neo4Net.Storageengine.Api.NodePropertyAccessor;
	using IEntityType = Neo4Net.Storageengine.Api.EntityType;
	using Value = Neo4Net.Values.Storable.Value;

	/// <summary>
	/// The PropertyLoader provides a stream-lined interface for getting multiple property values from a node in a single
	/// call. This can be used over the index provider API <seealso cref="NodePropertyAccessor"/> for better performance in these cases.
	/// </summary>
	public interface PropertyLoader
	{
		 /// <summary>
		 /// Loads set of properties for IEntity. For every target propertyId P, the value for this node is loaded. If the
		 /// IEntity has P, the onProperty methods will be called on the sink, with the correct value, and P will be removed
		 /// from propertyIds. If the IEntity lacks P no action will be taken. </summary>
		 /// <param name="entityId"> id of the IEntity to be loaded </param>
		 /// <param name="type"> IEntityType of the IEntity to load properties from </param>
		 /// <param name="propertyIds"> set of target property ids to load. Loaded properties are remove from this set. </param>
		 /// <param name="sink"> sink that will receive successfully loaded values for the target properties </param>
		 void LoadProperties( long IEntityId, IEntityType type, MutableIntSet propertyIds, PropertyLoader_PropertyLoadSink sink );
	}

	 public interface PropertyLoader_PropertyLoadSink
	 {
		  void OnProperty( int IEntityId, Value value );
	 }

}