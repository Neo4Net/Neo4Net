using System.Collections.Generic;

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
namespace Org.Neo4j.Storageengine.Api.txstate
{
	using IntIterable = org.eclipse.collections.api.IntIterable;
	using IntSets = org.eclipse.collections.impl.factory.primitive.IntSets;

	using Value = Org.Neo4j.Values.Storable.Value;

	/// <summary>
	/// Represents the property changes to a <seealso cref="NodeState node"/> or <seealso cref="RelationshipState relationship"/>:
	/// <ul>
	/// <li><seealso cref="addedProperties() Added properties"/>,</li>
	/// <li><seealso cref="removedProperties() removed properties"/>, and </li>
	/// <li><seealso cref="changedProperties() changed property values"/>.</li>
	/// </ul>
	/// </summary>
	public interface PropertyContainerState
	{
		 IEnumerator<StorageProperty> AddedProperties();

		 IEnumerator<StorageProperty> ChangedProperties();

		 IntIterable RemovedProperties();

		 IEnumerator<StorageProperty> AddedAndChangedProperties();

		 bool HasPropertyChanges();

		 bool IsPropertyChangedOrRemoved( int propertyKey );

		 Value PropertyValue( int propertyKey );
	}

	public static class PropertyContainerState_Fields
	{
		 public static readonly PropertyContainerState Empty = new PropertyContainerState_EmptyPropertyContainerState();
	}

	 public class PropertyContainerState_EmptyPropertyContainerState : PropertyContainerState
	 {
		  public override IEnumerator<StorageProperty> AddedProperties()
		  {
				return emptyIterator();
		  }

		  public override IEnumerator<StorageProperty> ChangedProperties()
		  {
				return emptyIterator();
		  }

		  public override IntIterable RemovedProperties()
		  {
				return IntSets.immutable.empty();
		  }

		  public override IEnumerator<StorageProperty> AddedAndChangedProperties()
		  {
				return emptyIterator();
		  }

		  public override bool HasPropertyChanges()
		  {
				return false;
		  }

		  public override bool IsPropertyChangedOrRemoved( int propertyKey )
		  {
				return false;
		  }

		  public override Value PropertyValue( int propertyKey )
		  {
				return null;
		  }
	 }

}